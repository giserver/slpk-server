using System.IO.Compression;

namespace Slpk.Server.Services
{
    public interface ISlpkFileService
    {
        public string? GetFullPath(string fileName);

        Task<byte[]?> ReadAsync(string filePath, string innerPath);
    }

    public class SlpkFileService : ISlpkFileService
    {
        private readonly IConfiguration configuration;

        public SlpkFileService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string? GetFullPath(string fileName)
        {
            var fullPath = Path.Combine(configuration.GetSection("Slpk:Dir").Value, fileName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        public async Task<byte[]?> ReadAsync(string filePath, string innerPath)
        {
            if (!File.Exists(filePath)) return null;

            using var zip = ZipFile.Open(filePath, ZipArchiveMode.Read);
            var entry = zip.GetEntry(innerPath);

            if (entry == null) return null;

            using var stream = entry.Open();

            if (innerPath.EndsWith(".gz"))
            {
                using var destStream = new MemoryStream();
                using var srcStream = new GZipStream(stream, CompressionMode.Decompress);
                await srcStream.CopyToAsync(destStream);
                return destStream.ToArray();
            }
            
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);
            return buffer;
        }
    }
}
