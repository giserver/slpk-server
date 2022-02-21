using Slpk.Server.Exceptions;
using System.IO.Compression;

namespace Slpk.Server.Services
{
    public interface ISlpkFileService
    {
        /// <summary>
        /// 获取项目入口完整路径
        /// 
        /// </summary>
        /// <param name="entryName">入口名，文件夹名称(.slpk解压文件夹名称)，亦 .slpk文件</param>
        /// <returns></returns>
        public string GetFullPath(string entryName);

        /// <summary>
        /// 读取项目文件
        /// </summary>
        /// <param name="projectPath">入口路径</param>
        /// <param name="innerPath">内部路径</param>
        /// <param name="deGzip">是否对Gzip类型文件解压</param>
        /// <returns></returns>
        Task<byte[]?> ReadAsync(string entryPath, string innerPath, bool deGzip = false);
    }

    public class SlpkFileService : ISlpkFileService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SlpkFileService> logger;

        public SlpkFileService(IConfiguration configuration,ILogger<SlpkFileService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public string GetFullPath(string entryName)
        {
            var fullPath = Path.Combine(configuration.GetSection("Slpk:Dir").Value, entryName);
            if (!ExistPath(fullPath,out var _))
                throw new BusnessException(404, $"Can't found SLPK: {entryName}");

            return fullPath;
        }

        public async Task<byte[]?> ReadAsync(string entryPath, string innerPath, bool deGzip = false)
        {
            if (!ExistPath(entryPath,out var isFile)) return null;
            
            // 获取对应文件的stream
            Stream? stream;
            ZipArchive? zip = null;

            if (isFile)
            {
                zip = ZipFile.Open(entryPath, ZipArchiveMode.Read);
                var entry = zip.GetEntry(innerPath);
                if (entry == null) return null;

                stream = entry.Open();
            }
            else
                stream = File.OpenRead(Path.Combine(entryPath, innerPath));

            // 读取文件内容
            try
            {
                // 对".gz"压缩文件选择式解压
                if (deGzip && innerPath.EndsWith(".gz"))
                {
                    using var destStream = new MemoryStream();
                    using var srcStream = new GZipStream(stream, CompressionMode.Decompress);
                    await srcStream.CopyToAsync(destStream);
                    return destStream.ToArray();
                }

                // todo：观察文件大小，考虑是否使用缓冲区
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer);
                return buffer;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "read slpk file error!");
                throw new BusnessException(500, "read slpk file error!", ex);
            }
            finally
            {
                stream?.Dispose();
                zip?.Dispose();
            }
        }

        /// <summary>
        /// 判断路径是否存在
        /// </summary>
        /// <param name="path">路径 (文件或文件夹)</param>
        /// <param name="isFile">路径是否指向文件</param>
        /// <returns></returns>
        private static bool ExistPath(string path, out bool isFile)
        {
            isFile = false;

            if (File.Exists(path))
            {
                isFile = true;
                return true;
            }

            return Directory.Exists(path);
        }
    }
}
