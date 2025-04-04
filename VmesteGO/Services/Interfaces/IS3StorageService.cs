namespace VmesteGO.Services.Interfaces;

public interface IS3StorageService
{
    Task<string> GenerateSignedUploadUrl(string userId);
    string GetImageUrl(string userId);
    Task DeleteImageAsync(string key);
}