using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CleannetCode_bot
{
    public class StorageFileService : IStorageService
    {
        private readonly ILogger<StorageFileService> logger;
        private readonly IConfiguration config;
        private static readonly string _directory = "./FileStorage/";
        private static readonly string _fileName = "data.json";

        private readonly JsonSerializerOptions optionsJson = new();

        public StorageFileService(ILogger<StorageFileService> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;

            optionsJson = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
        }
        public Task AddObject(object obj, Type type, string metodName, CancellationToken cts)
        {
            if (obj is null) return Task.CompletedTask;
            if (type is null) return Task.CompletedTask;
            if (metodName is null) return Task.CompletedTask;

            var json = JsonSerializer.Serialize(obj, type, optionsJson);

            string dir = $"{_directory}{metodName}/";
            Directory.CreateDirectory(dir);
            string fileName = DateTime.Now.ToString("ddMMyyyy_HHmmss_ffff") + "-" + _fileName;
            string path = Path.Combine(dir, fileName);
            File.WriteAllTextAsync(path, json, cts);

            logger.LogInformation("{DateTime:dd.MM.yyyy HH:mm:ss:ffff}\tnew file created\t{dir}\t{fileName}", DateTime.Now, dir, fileName);

            return Task.CompletedTask;
        }
    }
}