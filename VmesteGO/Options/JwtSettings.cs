namespace VmesteGO.Options;

public class JwtSettings
{
    public required string Key { get; set; }
    public int ExpirationMinutes { get; set; } = 30;
}