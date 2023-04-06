using Microsoft.Extensions.Options;
using Minio.Client.Http.Extensions;
using Minio.Client.Http.Internal;
using Minio.Client.Http.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Minio.Client.Http
{
    public class MinioHttpClient : IMinioHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsSnapshot<MinioOptions> _options;

        public MinioHttpClient(HttpClient httpClient, IOptionsSnapshot<MinioOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<ListAllMyBucketsResult> GetBucketsAsync(CancellationToken cancellationToken = default)
        {
            using var result = await _httpClient.GetAsync("", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return await result.Content.ReadAsAsync<ListAllMyBucketsResult>().ConfigureAwait(false);
        }

        public async Task<bool> BucketExistAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            using var result = await _httpClient.HeadAsync(new ObjectIdentifire(bucketName).ToString(), cancellationToken).ConfigureAwait(false);
            return result.IsSuccessStatusCode;
        }

        public async Task RemoveBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            using var result = await _httpClient.DeleteAsync(new ObjectIdentifire(bucketName).ToString(), cancellationToken).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
        }

        public async Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutWithEmptyBodyAsync(new ObjectIdentifire(bucketName).ToString(), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> ObjectExistAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {

            using var response = await _httpClient.HeadAsync($"{new ObjectIdentifire(bucketName, objectName)}", cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }


        public async IAsyncEnumerable<ObjectInformation> GetObjectsAsync(GetObjectsRequests requests, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ListBucketResult listBucketResult = null;
            var query = requests;
            do
            {
                using var result = await _httpClient.GetAsync(query.ToString(), cancellationToken).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                listBucketResult = await result.Content.ReadAsAsync<ListBucketResult>().ConfigureAwait(false);

                foreach (var item in listBucketResult.Contents)
                {
                    yield return new ObjectInformation(WebUtility.UrlDecode(item.Key), item.Size, item.LastModified, item.ETag.Trim('"'));
                }

                if (listBucketResult.IsTruncated)
                {
                    var lastKey = listBucketResult.Contents?.LastOrDefault()?.Key;

                    query = new GetObjectsRequests(bucket: requests.Bucket,
                                                      recursive: requests.Recursive,
                                                      prefix: requests.Prefix,
                                                      maxKeys: requests.MaxKeys,
                                                      marker: string.IsNullOrEmpty(listBucketResult.NextMarker) ? HttpUtility.UrlDecode(lastKey) : HttpUtility.UrlDecode(listBucketResult.NextMarker),
                                                      useV2: requests.UseV2,
                                                      versions: requests.Versions,
                                                      continuationToken: listBucketResult.NextContinuationToken,
                                                      versionIdMarker: listBucketResult.VersionIdMarker);
                }

            } while (listBucketResult.IsTruncated);

        }

        public async Task<HttpResponseMessage> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                requestUri: $"{new ObjectIdentifire(bucketName, objectName)}",
                completionOption: HttpCompletionOption.ResponseHeadersRead,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }


        public async Task RemoveObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"{new ObjectIdentifire(bucketName, objectName)}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ObjectInformation> GetObjectInfoAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.HeadAsync($"{new ObjectIdentifire(bucketName, objectName)}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var metadata = response.Headers.Where(x => x.Key.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase))
                 .ToDictionary(x => x.Key, x => x.Value);

            return new ObjectInformation(objectName,
                response.Content.Headers.ContentLength.Value,
                response.Content.Headers?.LastModified,
                response.Headers.ETag?.Tag.Trim('"'),
                response.Content.Headers.ContentType?.ToString(),
                metadata);

        }

        public HttpRequestMessage PresignedGetObjectRequest(string bucketName, string objectName, int expires)
        {
            return new HttpRequestMessage(HttpMethod.Get, new Uri(_httpClient.BaseAddress, $"{new ObjectIdentifire(bucketName, objectName)}")).PresignUrl(expires, _options.Value);
        }

        public Uri PresignedGetObjectUrl(string bucketName, string objectName, int expires)
        {
            return new Uri(_httpClient.BaseAddress, $"{new ObjectIdentifire(bucketName, objectName)}").PresignUrl(HttpMethod.Get, expires, _options.Value);
        }

        public HttpRequestMessage PresignedPutObjectRequest(string bucketName, string objectName, int expires)
        {
            return new HttpRequestMessage(HttpMethod.Put, new Uri(_httpClient.BaseAddress, $"{new ObjectIdentifire(bucketName, objectName)}")).PresignUrl(expires, _options.Value);
        }

        public Uri PresignedPutObjectUrl(string bucketName, string objectName, int expires)
        {
            return new Uri(_httpClient.BaseAddress, $"{new ObjectIdentifire(bucketName, objectName)}").PresignUrl(HttpMethod.Put, expires, _options.Value);
        }

        public async Task<ObjectInformation> CorePutObjectAsync(string bucketName, string objectName, MinioFileRequest file, CancellationToken cancellationToken = default)
        {
            return await CorePutObjectAsync(bucketName, objectName, file, false, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ObjectInformation> CorePutObjectAsync(string bucketName, string objectName, MinioFileRequest file, bool leaveOpen, CancellationToken cancellationToken = default)
        {
            using var content = new MinioStreamContent(file.Stream, leaveOpen);

            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            using var response = await _httpClient.PutAsync($"{new ObjectIdentifire(bucketName, objectName)}{file}", content, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return new ObjectInformation(objectName,
               response.RequestMessage.Content.Headers.ContentLength.Value,
               null,
               response.Headers.ETag?.Tag.Trim('"'),
               file.ContentType);
        }

        public async Task<ObjectInformation> PutObjectAsync(string bucketName, string objectName, MinioFileRequest file, CancellationToken cancellationToken = default)
        {
            return await PutObjectAsync(bucketName, objectName, file, false, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<ObjectInformation> PutObjectAsync(string bucketName, string objectName, MinioFileRequest file, bool leaveOpen, CancellationToken cancellationToken = default)
        {
            if (file.MaxSingleSizeUpload.GetValueOrDefault(_options.Value.MaxSingleSizeUpload) < file.Stream.Length)
            {
                using var partialStream = new PartialStream(file.Stream, leaveOpen, file.PartSize ?? _options.Value.DefaultMultipartSize);

                var uploadId = await InitializeMulipartUploadAsync(bucketName, objectName, cancellationToken).ConfigureAwait(false);

                var completeRequest = new CompleteMultipartUploadRequest(uploadId);
                try
                {
                    do
                    {
                        var result = await CorePutObjectAsync(bucketName, objectName,
                            new MinioFileRequest(partialStream, file.ContentType, file.Metadata, uploadId, partialStream.CurrentPart), true, cancellationToken).ConfigureAwait(false);

                        completeRequest.Add(partialStream.CurrentPart, result.ETag);

                    } while (partialStream.TryExtend());

                    var etag = await CompleteMulipartUploadAsync(bucketName, objectName, completeRequest, cancellationToken).ConfigureAwait(false);

                    return new ObjectInformation(objectName, file.Stream.Length, null, etag, file.ContentType);
                }
                catch (Exception)
                {
                    await RemoveUploadAsync(bucketName, objectName, uploadId, cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                return await CorePutObjectAsync(bucketName, objectName, file, leaveOpen, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task RemoveUploadAsync(string bucketName, string objectName, string uploadId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"{new ObjectIdentifire(bucketName, objectName)}?uploadId={uploadId}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task<string> CompleteMulipartUploadAsync(string bucketName, string objectName, CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default)
        {
            List<XElement> parts = new List<XElement>();

            foreach (var item in request.ETags)
            {
                parts.Add(new XElement("Part", new XElement("PartNumber", item.Key), new XElement("ETag", item.Value)));
            }

            var completeMultipartUploadXml = new XElement("CompleteMultipartUpload", parts);

            var bodyString = completeMultipartUploadXml.ToString();

            var content = new StringContent(bodyString, System.Text.Encoding.UTF8, "application/xml");

            using var response = await _httpClient.PostAsync($"{new ObjectIdentifire(bucketName, objectName)}?uploadId={request.UploadId}", content, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.Headers.ETag.ToString().Trim('"');
        }

        public virtual async Task<string> InitializeMulipartUploadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostWithEmptyBodyAsync($"{new ObjectIdentifire(bucketName, objectName)}?uploads=", cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<InitiateMultipartUploadResult>().ConfigureAwait(false);

            return result.UploadId;
        }

        public virtual async Task<ObjectInformation> CopyAsync(CopyRequest copyRequest, CancellationToken cancellationToken = default)
        {
            var source = await GetObjectInfoAsync(copyRequest.SourceBucketName, copyRequest.SourceObjectName, cancellationToken).ConfigureAwait(false);

            copyRequest.PopulateHeaders(source.Metadata);

            if (copyRequest.MaxObjectCopySize.GetValueOrDefault(MinioLimitation.MaxObjectCopySize) < source.Size)
            {
                var uploadId = await InitializeMulipartUploadAsync(
                    copyRequest.DestinationBucketName, copyRequest.DestinationObjectName, cancellationToken).ConfigureAwait(false);


                var sizer = new PartSizer(source.Size, _options.Value.DefaultMultipartSize);
                var completeRequest = new CompleteMultipartUploadRequest(uploadId);

                foreach (var part in sizer.GetParts())
                {
                    var result = await CoreCopyAsync(new MultipartCopyRequest(copyRequest, uploadId, part), cancellationToken).ConfigureAwait(false);
                    completeRequest.Add(part.PartId, result.ETag);
                }

                var etag = await CompleteMulipartUploadAsync(copyRequest.DestinationBucketName, copyRequest.DestinationObjectName, completeRequest, cancellationToken)
                        .ConfigureAwait(false);

                return new ObjectInformation(copyRequest.DestinationObjectName, source.Size, null, etag, source.ContentType, source.Metadata);
            }
            else
            {
                var result = await CoreCopyAsync(new MultipartCopyRequest(copyRequest), cancellationToken).ConfigureAwait(false);

                return new ObjectInformation(copyRequest.DestinationObjectName, source.Size, null, result.ETag, source.ContentType, source.Metadata);
            }
        }

        public virtual async Task<ObjectInformation> CoreCopyAsync(MultipartCopyRequest copyRequest, CancellationToken cancellationToken = default)
        {
            var destination = new ObjectIdentifire(copyRequest.Request.DestinationBucketName, copyRequest.Request.DestinationObjectName);
            var source = new ObjectIdentifire(copyRequest.Request.SourceBucketName, copyRequest.Request.SourceObjectName);

            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{destination}{copyRequest}")
            {
                Content = new StringContent("", System.Text.Encoding.UTF8),
            };

            request.SetEmptyRequest();

            request.Headers.TryAddWithoutValidation("x-amz-copy-source", source.ToString());

            foreach (var item in copyRequest.Request.GetHeaders())
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value);
            }

            foreach (var item in copyRequest.Request.Conditions.GetHeaders())
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value);
            }

            if (copyRequest.Part.HasValue)
            {
                request.Headers.TryAddWithoutValidation("x-amz-copy-source-range", $"bytes={copyRequest.Part.Value.From}-{copyRequest.Part.Value.To}");
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            if (copyRequest.Part.HasValue)
            {
                var result = await response.Content.ReadAsAsync<CopyPartResult>().ConfigureAwait(false);
                return new ObjectInformation(copyRequest.Request.DestinationObjectName, -1, result.LastModified, result.ETag.Trim('"'), null, null);
            }
            else
            {
                return new ObjectInformation(copyRequest.Request.DestinationObjectName, -1, null, response.Headers.ETag.Tag.Trim('"'), null, null);
            }
        }
    }
}