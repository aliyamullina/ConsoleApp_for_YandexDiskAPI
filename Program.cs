using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YandexDisk.Client;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace ConsoleApp_for_YandexDiskAPI
{
    /// <summary>
    /// Информация о файле
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// Информация о файле
        /// </summary>
        /// <param name="name">Название файла с расширением</param>
        /// <param name="path">Полный путь к файлу</param>
        public FileDetails(string name, string path)
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Название файла с расширением
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полный путь к файлу
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// Информация о путях загрузки
    /// </summary>
    public class UploadDetails
    {
        /// <summary>
        /// Информация о путях загрузки
        /// </summary>
        /// <param name="source">Путь к локальной папке</param>
        /// <param name="receiver">Папка на Яндекс.Диске</param>
        public UploadDetails(string source, string receiver)
        {
            Source = source;
            Receiver = receiver;
        }

        /// <summary>
        /// Путь к локальной папке
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Папка на Яндекс.Диске
        /// </summary>
        public string Receiver { get; set; }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            GetDirectoryPaths(out UploadDetails options);

            GetFindFiles(out List<FileDetails> filesList, in options);

            GetAuthorization(out string oauthToken);
            IDiskApi diskApi = new DiskHttpApi(oauthToken);

            await CreateDirectoryAsync(diskApi, options);

            GetShowMessage("Файл", "Статус");

            foreach (FileDetails file in filesList)
            {
                await GetStartAsync(options, diskApi, file);
            }
        }

        /// <summary>
        /// Загрузка файлов на Яндекс.Диск
        /// </summary>
        /// <param name="options"></param>
        /// <param name="diskApi"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task GetStartAsync(UploadDetails options, IDiskApi diskApi, FileDetails file)
        {
            // формирование запроса пути
            Link link = await diskApi.Files.GetUploadLinkAsync(options.Receiver + "/" + file.Name, true).ConfigureAwait(false);

            GetShowMessage(file.Name, "Идет загрузка");

            using (FileStream fileStream = File.OpenRead(file.Path))
            {
                // загрузка файлов по указанному пути
                await diskApi.Files.UploadAsync(link, fileStream);
            }

            GetShowMessage(file.Name, "Загружен");
        }

        /// <summary>
        /// Авторизация, нужно получить ключ при регистрации приложения
        /// </summary>
        /// <param name="oauthToken"></param>
        private static void GetAuthorization(out string oauthToken) => oauthToken = "";

        /// <summary>
        /// Запрос путей
        /// </summary>
        /// <param name="options"></param>
        private static void GetDirectoryPaths(out UploadDetails options)
        {
            Console.WriteLine("Откуда загрузить файлы (полный путь):");
            string source = Console.ReadLine();

            Console.WriteLine("\nКуда сохранить файлы:");
            string receiver = Console.ReadLine();

            options = new UploadDetails(source, receiver);
        }

        /// <summary>
        /// Получение информации о файлах по указанному пути
        /// </summary>
        /// <param name="filesList"></param>
        /// <param name="options"></param>
        private static void GetFindFiles(out List<FileDetails> filesList, in UploadDetails options)
        {
            filesList = new List<FileDetails>();

            DirectoryInfo foundFiles = new DirectoryInfo(options.Source);
            FileInfo[] files = foundFiles.GetFiles("*.*");

            foreach (var file in files)
            {
                filesList.Add(new FileDetails(file.Name, file.FullName));
            }
        }

        /// <summary>
        /// Создается папка на Яндекс.Диске
        /// </summary>
        /// <param name="diskApi"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static async Task CreateDirectoryAsync(IDiskApi diskApi, UploadDetails options)
        {
            await diskApi.Commands.CreateDictionaryAsync(options.Receiver);
        }

        private static void GetShowMessage(string NameText, string StatusText)
        {
            Console.WriteLine("{0,-20} {1,-20}", NameText, StatusText);
        }
    }

}

