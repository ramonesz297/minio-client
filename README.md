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

## MinioOptions

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


## benchamrk

### GetObjectInfo
|     Method |                  Job |              Runtime |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated |
|----------- |--------------------- |--------------------- |---------:|---------:|---------:|------:|--------:|----------:|
|  Restsharp |             .NET 6.0 |             .NET 6.0 | 531.3 ms | 10.49 ms | 19.19 ms |  1.00 |    0.00 |     87 KB |
| HttpClient |             .NET 6.0 |             .NET 6.0 | 176.0 ms |  3.43 ms |  6.19 ms |  0.33 |    0.02 |     16 KB |
|            |                      |                      |          |          |          |       |         |           |
|  Restsharp | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 175.4 ms |  2.54 ms |  1.99 ms |  1.00 |    0.00 |     85 KB |
| HttpClient | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 184.1 ms |  4.21 ms | 12.01 ms |  1.05 |    0.04 |     55 KB |

### Get presigned url
|         Method |                  Job |              Runtime |     Mean |    Error |   StdDev | Ratio |  Gen 0 | Allocated |
|--------------- |--------------------- |--------------------- |---------:|---------:|---------:|------:|-------:|----------:|
|      Restsharp |             .NET 6.0 |             .NET 6.0 | 468.6 ns |  6.05 ns |  5.37 ns |  1.00 | 0.0757 |     238 B |
| HttpClient_url |             .NET 6.0 |             .NET 6.0 | 213.4 ns |  0.35 ns |  0.27 ns |  0.45 | 0.0302 |      95 B |
|                |                      |                      |          |          |          |       |        |           |
|      Restsharp | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 874.1 ns | 12.45 ns | 11.04 ns |  1.00 | 0.1428 |     451 B |
| HttpClient_url | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 445.9 ns |  8.40 ns | 10.32 ns |  0.51 | 0.0745 |     235 B |

