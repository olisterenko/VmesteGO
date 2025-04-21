using System.Security.Cryptography;
using System.Text;
using Amazon.Runtime.Internal.Auth;
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

    public Task<string> GenerateSignedUploadUrl(string key)
    {
        var host = _s3Options.Host;
        var region = _s3Options.Region;
        var service = "s3";

        var now = DateTime.UtcNow;
        var datestamp = now.ToString("yyyyMMdd");
        var timestamp = now.ToString("yyyyMMddTHHmmssZ");

        var canonicalQueryString =
            $"X-Amz-Algorithm=AWS4-HMAC-SHA256" +
            $"&X-Amz-Credential={Uri.EscapeDataString(_s3Options.AccessKey + "/" + datestamp + "/ru-central1/s3/aws4_request")}" +
            $"&X-Amz-Date={timestamp}" +
            $"&X-Amz-Expires=3600" +
            $"&X-Amz-SignedHeaders=host";

        var canonicalRequest =
            $"PUT\n" +
            $"/{_s3Options.BucketName}/{key}\n" +
            $"{canonicalQueryString}\n" +
            $"host:{host}\n\n" +
            $"host\n" +
            $"UNSIGNED-PAYLOAD";

        var hashedCanonicalRequest = HashSHA256Hex(canonicalRequest);

        var stringToSign =
            $"AWS4-HMAC-SHA256\n" +
            $"{timestamp}\n" +
            $"{datestamp}/ru-central1/s3/aws4_request\n" +
            $"{hashedCanonicalRequest}";

        var signingKey = GetSignatureKey(_s3Options.SecretKey, datestamp, region, service);
        var signatureBytes = HmacSHA256(signingKey, stringToSign);
        var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

        return Task.FromResult($"https://{host}/{_s3Options.BucketName}/{key}?" +
                               $"{canonicalQueryString}" +
                               $"&X-Amz-Signature={signature}");
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

    static byte[] HmacSHA256(byte[] key, string data)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

    static string HashSHA256Hex(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    static byte[] GetSignatureKey(string secretKey, string dateStamp, string regionName, string serviceName)
    {
        var kSecret = Encoding.UTF8.GetBytes("AWS4" + secretKey);
        var kDate = HmacSHA256(kSecret, dateStamp);
        var kRegion = HmacSHA256(kDate, regionName);
        var kService = HmacSHA256(kRegion, serviceName);
        var kSigning = HmacSHA256(kService, "aws4_request");
        return kSigning;
    }
}