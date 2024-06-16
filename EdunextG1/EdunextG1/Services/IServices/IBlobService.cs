namespace EdunextG1.Services.IServices
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(IFormFile file);
        Task DeleteBlobAsync(string blobUrl);
    }
}
