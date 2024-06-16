namespace EdunextG1.Services.IServices
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(IFormFile file);
        Task<string> UploadBlobWithContentTypeAsync(IFormFile file, string contentType);
        Task DeleteBlobAsync(string blobUrl);
    }
}
