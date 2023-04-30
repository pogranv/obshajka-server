using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

using Obshajka.YandexDiskApi;
using static System.Net.Mime.MediaTypeNames;

namespace Obshajka.YandexDisk
{
    public class YandexDisk : ICloudImageStorage
    {
        private static DiskHttpApi s_api;

        private const string _folderName = "Obshajka_Advertisement_Images";

        static YandexDisk()
        {
            var token = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
            .GetSection("YandexDiskConnectionStrings")["token"];
            s_api = new DiskHttpApi(token);
            CreateImagesDirectoryIfNotExists();
        }

        public async Task<string> UploadImageAndGetLink(IFormFile image)
        {
            string pathToImage = MakePathToImage(Path.GetExtension(image.FileName));
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var link = await s_api.Files.GetUploadLinkAsync(pathToImage, overwrite: true);
                await s_api.Files.UploadAsync(link, ms);
            }

            var linkForImage = await s_api.Files.GetDownloadLinkAsync(pathToImage);
            return linkForImage.Href;
        }

        private string MakePathToImage(string extention)
        {
            return $"/{_folderName}/{Guid.NewGuid()}.{extention}";
        }

        private static async void CreateImagesDirectoryIfNotExists()
        {
            var rootFolderData = await s_api.MetaInfo.GetInfoAsync(new ResourceRequest
            {
                Path = "/"
            });

            if (!rootFolderData.Embedded.Items.Any(i => i.Type == ResourceType.Dir && i.Name.Equals(_folderName)))
            {
                await s_api.Commands.CreateDictionaryAsync("/" + _folderName);
            }
        }
    }
}
