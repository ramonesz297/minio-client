using Minio.Client.Http.Model;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Minio.Client.Http
{
    public interface IMinioHttpClient
    {
        Task<bool> BucketExistAsync(string bucketName, CancellationToken cancellationToken = default);
        Task<string> CompleteMulipartUploadAsync(string bucketName, string objectName, CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default);
        Task<ObjectInformation> CopyAsync(CopyRequest copyRequest, CancellationToken cancellationToken = default);
        Task<ObjectInformation> CoreCopyAsync(MultipartCopyRequest copyRequest, CancellationToken cancellationToken = default);
        Task<ObjectInformation> CorePutObjectAsync(string bucketName, string objectName, MinioFileRequest file, CancellationToken cancellationToken = default);
        Task<ObjectInformation> CorePutObjectAsync(string bucketName, string objectName, MinioFileRequest file, bool leaveOpen, CancellationToken cancellationToken = default);
        Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default);
        Task<ListAllMyBucketsResult> GetBucketsAsync(CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
        Task<ObjectInformation> GetObjectInfoAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
        Task<string> InitializeMulipartUploadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
        HttpRequestMessage PresignedGetObjectRequest(string bucketName, string objectName, int expires);
        Uri PresignedGetObjectUrl(string bucketName, string objectName, int expires);
        HttpRequestMessage PresignedPutObjectRequest(string bucketName, string objectName, int expires);
        Uri PresignedPutObjectUrl(string bucketName, string objectName, int expires);
        Task<ObjectInformation> PutObjectAsync(string bucketName, string objectName, MinioFileRequest file, CancellationToken cancellationToken = default);
        Task<ObjectInformation> PutObjectAsync(string bucketName, string objectName, MinioFileRequest file, bool leaveOpen, CancellationToken cancellationToken = default);
        Task RemoveBucketAsync(string bucketName, CancellationToken cancellationToken = default);
        Task RemoveObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
        Task RemoveUploadAsync(string bucketName, string objectName, string uploadId, CancellationToken cancellationToken = default);
    }
}