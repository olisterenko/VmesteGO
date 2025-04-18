namespace VmesteGO.Options;

public class S3Options
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
}