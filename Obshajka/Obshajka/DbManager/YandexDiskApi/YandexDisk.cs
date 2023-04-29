using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

using Obshajka.YandexDiskApi;

namespace Obshajka.YandexDisk
{
    public class YandexDisk : ICloudImageStorage
    {
        private static DiskHttpApi s_api;

        private const string _folderName = "Obshajka_Advertisement_Images";
        private const string _token = "y0_AgAEA7qkJY7BAAkpoAAAAADcsWWswDVBwOCvSB6glBJthBDT9av8wi4";

        static YandexDisk()
        {
            s_api = new DiskHttpApi(_token);
            CreateImagesDirectoryIfNotExists();
        }

        public async Task<string> UploadImageAndGetLink(IFormFile image)
        {
            string pathToImage = MakePathToImage(Path.GetExtension(image.FileName));
            var link = await s_api.Files.GetUploadLinkAsync(pathToImage, overwrite: true);

            try // TODO: удалить
            {
                using (var ms = new MemoryStream())
                {
                    await image.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    await s_api.Files.UploadAsync(link, ms);
                }
            } catch (Exception ex)
            {
                Console.WriteLine("ATTENTION!!!!");
                Console.WriteLine(ex.Message);
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
