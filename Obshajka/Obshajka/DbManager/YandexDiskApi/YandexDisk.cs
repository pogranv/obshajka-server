using Obshajka.YandexDiskApi;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace Obshajka.YandexDisk
{
    public class YandexDisk : ICloudImageStorage
    {
        private static DiskHttpApi _api;
        private const string _folderName = "Obshajka_Advertisement_Images";

        static YandexDisk()
        {
            _api = new DiskHttpApi("y0_AgAEA7qkJY7BAAkpoAAAAADcsWWswDVBwOCvSB6glBJthBDT9av8wi4");
            CreateImagesDirectoryIfNotExists();
        }

        public async Task<string> UploadImageAndGetLink(IFormFile image)
        {
            string pathToImage = MakePathToImage(Path.GetExtension(image.FileName));
            var link = await _api.Files.GetUploadLinkAsync(pathToImage, overwrite: true);

            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                await _api.Files.UploadAsync(link, ms);
            }

            var linkForImage = await _api.Files.GetDownloadLinkAsync(pathToImage);
            return linkForImage.Href;
        }

        private string MakePathToImage(string extention)
        {
            return $"/{_folderName}/{Guid.NewGuid()}.{extention}";
        }

        private static async void CreateImagesDirectoryIfNotExists()
        {
            var rootFolderData = await _api.MetaInfo.GetInfoAsync(new ResourceRequest
            {
                Path = "/"
            });
            if (!rootFolderData.Embedded.Items.Any(i => i.Type == ResourceType.Dir && i.Name.Equals(_folderName)))
            {
                await _api.Commands.CreateDictionaryAsync("/" + _folderName);
            }
        }
    }
}
