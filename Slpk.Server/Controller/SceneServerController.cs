using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Slpk.Server.Filters;
using Slpk.Server.Services;
using System.Text.Json;

namespace Slpk.Server.Controller
{
    [ApiController]
    [Route("api/{slpk}/SceneServer")]
    public class SceneServerController : ControllerBase
    {
        private readonly string slpkName;
        private readonly string slpkFullPath;
        private readonly ISlpkFileService slpkFileService;
        private readonly IMemoryCache memoryCache;

        public SceneServerController(IHttpContextAccessor httpContextAccessor, ISlpkFileService slpkFileService, IMemoryCache memoryCache)
        {
            slpkName = httpContextAccessor.HttpContext?.GetRouteValue("slpk")?.ToString()!;
            slpkFullPath = slpkFileService.GetFullPath(slpkName);

            this.slpkFileService = slpkFileService;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetServerInfoAsync()
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, "3dSceneLayer.json.gz", true);

            return Ok(new
            {
                ServiceName = slpkName,
                Name = slpkName,
                CurrentVersion = 10.6,
                ServiceVersion = "1.6",
                SupportedBindings = new[] { "REST" },
                Layers = JsonSerializer.Deserialize<object>(buffer!)
            });
        }

        [HttpGet("layers/0")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetLayerInfoAsync()
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, "3dSceneLayer.json.gz", true);
            return File(buffer!, "application/json");
        }

        [HttpGet("layers/{layer}/nodes/{node}")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetNodeInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/3dNodeIndexDocument.json.gz", true);
            if (buffer == null)
                return NoContent();

            return File(buffer!, "application/json");
        }

        [HttpGet("layers/{layer}/nodes/{node}/geometries/{geometryID}")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetNodeGeometriesAsync(string layer, string node, string geometryID)
        {
            var buffer = await memoryCache.GetOrCreateAsync($"{slpkName}-{node}-{geometryID}", async entry =>
            {
                var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/geometries/{geometryID}.bin.gz", true);
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(3));
                return buffer;
            });

            return File(buffer!, "application/octet-stream; charset=binary");
        }

        [HttpGet("layers/{layer}/nodes/{node}/textures/0_0")]
        [ResponseHeader("Content-Disposition", "attachment; filename=\"0_0.jpg\"")]
        public async Task<IActionResult> GetTexturesInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/textures/0_0.jpg");
            if (buffer == null)
                buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/textures/0_0.bin");

            if (buffer == null)
                return NoContent();

            return File(buffer, "image/jpeg");
        }
        [HttpGet("layers/{layer}/nodes/{node}/textures/0_0_1")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetCTexturesInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/textures/0_0_1.bin.dds.gz", true);
            if (buffer == null)
                return NoContent();

            return File(buffer, "application/octet-stream; charset=binary");
        }

        [HttpGet("layers/{layer}/nodes/{node}/features/0")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetFeatureInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/features/0.json.gz", true);
            if (buffer == null)
                return NoContent();

            return File(buffer!, "application/json");
        }

        [HttpGet("layers/{layer}/nodes/{node}/shared")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetSharedInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/shared/sharedResource.json.gz", true);
            if (buffer == null)
                return NoContent();

            return File(buffer!, "application/json");
        }

        [HttpGet("layers/{layer}/nodes/{node}/attributes/{attribute}/0")]
        //[ResponseHeader("Content-Encoding", "gzip")]
        public async Task<IActionResult> GetAttributeInfoAsync(string layer, string node, string attribute)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/attributes/{attribute}/0.bin.gz", true);
            if (buffer == null)
                return NoContent();

            return File(buffer, "application/octet-stream; charset=binary");
        }

    }
}
