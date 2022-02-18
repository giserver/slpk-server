using Slpk.Server.Services;
using Xunit;

namespace Slpk.Server.Test
{
    public class SlpkFileServiceTest
    {
        private readonly ISlpkFileService slpkFileService;

        public SlpkFileServiceTest(ISlpkFileService slpkFileService)
        {
            this.slpkFileService = slpkFileService;
        }

        [Fact]
        public void ReadInnerFile2Buffer()
        {
            var ret = slpkFileService.ReadAsync("1.slpk", "metadata.json").Result;
            Assert.NotNull(ret);
        }

        [Fact]
        public void ReadInnerGZipFile2Buffer()
        {
            var ret = slpkFileService.ReadAsync("1.slpk", "nodes/0/3dNodeIndexDocument.json.gz").Result;
            Assert.NotNull(ret);
        }

        [Fact]
        public void ReadErrorPathInnerFileIsNull()
        {
            var ret = slpkFileService.ReadAsync("1.slpk", "nodes/0/3dNodeIndexDocument.json.gz.xxx").Result;
            Assert.True(ret == null);
        }
    }
}