namespace VmesteGO.Dto.Requests;

public class ImageUploadConfirmationRequest
{
    public required string ImageKey { get; set; } 
    public int OrderIndex { get; set; }
}