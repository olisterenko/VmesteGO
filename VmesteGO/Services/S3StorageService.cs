using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VmesteGO.Options;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Services;

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;

    public S3StorageService(IOptions<S3Options> s3Options, IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
    }
    
    public async Task<string> GenerateSignedUploadUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3Options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = "image/jpg",
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public string GetImageUrl(string key)
    {
        return $"{_s3Options.EndpointUrl}/{_s3Options.BucketName}/{key}";
    }
    
    public async Task DeleteImageAsync(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _s3Options.BucketName,
            Key = key
        };
        await _s3Client.DeleteObjectAsync(deleteRequest);
    }
}