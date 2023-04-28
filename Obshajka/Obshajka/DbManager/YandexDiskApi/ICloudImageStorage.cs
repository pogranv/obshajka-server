namespace Obshajka.YandexDiskApi
{
    public interface ICloudImageStorage
    {
        public Task<string> UploadImageAndGetLink(IFormFile image);
    }
}
