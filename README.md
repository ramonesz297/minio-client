# minio-client

## Why use this http client?

 - This client implemented with ```System.Net.Http.HttpClient```
 - Less memory allocation
 - Ability to generate presigned url without ```async/await``` 

## How to use?

```
// asp net core

services.AddOptions().Configure<MinioOptions>(o=>
{
    o.AccessKey = "Q3AM3UQ867SPQQA43P2F";
    o.SecretKey = "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG";
});

services.AddHttpClient<MinioHttpClient>(o=>
{
    o.BaseAddress = new Uri("https://play.min.io");
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

```

## configure MinioOptions

- AccessKey
- SecretKey
- SessionToken
- Region (default = "us-east-1")
- MaxSingleSizeUpload (default = 5Mb, min = 5mb, max = 5Gb) 
  
  if uploaded object size =< ```MaxSingleSizeUpload```, client will not use multipart upload
 
- DefaultMultipartSize ( min = 5mb, max = 5Gb) 

## Available api methods

### Buckets
- GetBucketsAsync 
- BucketExistAsync
- RemoveBucketAsync
- CreateBucketAsync
### Objects
- GetObjectAsync
- RemoveObjectAsync
- GetObjectInfoAsync
- PutObjectAsync
- CopyAsync
- PresignedGetObjectRequest
- PresignedGetObjectUrl
- PresignedPutObjectRequest
## What is next?

- More unit tests
- Implement all available apis
- Add integration with ```Microsoft.Extensions.DependencyInjection```
