using Microsoft.AspNetCore.Mvc;
using Slpk.Server.Filters;
using Slpk.Server.Services;
using System.Text.Json;

namespace Slpk.Server.Controller
{
    [ApiController]
    //[ServiceFilter(typeof(SceneServerActionFilter))]
    [Route("api/{slpk}/SceneServer")]
    public class SceneServerController : ControllerBase
    {
        private readonly string slpkName;
        private readonly string slpkFullPath;
        private readonly ISlpkFileService slpkFileService;

        public record LayerNodeViewModel
        {
            public string Layer { get; set; }

            public string Node { get; set; }
        }

        public SceneServerController(IHttpContextAccessor httpContextAccessor, ISlpkFileService slpkFileService)
        {
            slpkName = httpContextAccessor.HttpContext?.GetRouteValue("slpk")?.ToString()!;
            slpkFullPath = slpkFileService.GetFullPath(slpkName);

            this.slpkFileService = slpkFileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetServerInfoAsync()
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, "3dSceneLayer.json.gz");

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
        public async Task<IActionResult> GetLayerInfoAsync()
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, "3dSceneLayer.json.gz");
            return Ok(JsonSerializer.Deserialize<object>(buffer!));
        }

        [HttpGet("layers/{layer}/nodes/{node}")]
        public async Task<IActionResult> GetNodeInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/3dNodeIndexDocument.json.gz");
            if (buffer == null)
                return NoContent();

            return Ok(JsonSerializer.Deserialize<object>(buffer));
        }

        [HttpGet("layers/{layer}/nodes/{node}/geometries/0")]
        public async Task<IActionResult> GetNodeGeometriesAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/geometries/0.bin.gz");
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
        public async Task<IActionResult> GetCTexturesInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/textures/0_0_1.bin.dds.gz");
            if (buffer == null)
                return NoContent();

            return File(buffer, "application/octet-stream; charset=binary");
        }

        [HttpGet("layers/{layer}/nodes/{node}/features/0")]
        public async Task<IActionResult> GetFeatureInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/features/0.json.gz");
            if (buffer == null)
                return NoContent();

            return Ok(JsonSerializer.Deserialize<object>(buffer));
        }

        [HttpGet("layers/{layer}/nodes/{node}/shared")]
        public async Task<IActionResult> GetSharedInfoAsync(string layer, string node)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/shared/sharedResource.json.gz");
            if (buffer == null)
                return NoContent();

            return Ok(JsonSerializer.Deserialize<object>(buffer));
        }

        [HttpGet("layers/{layer}/nodes/{node}/attributes/{attribute}/0")]
        public async Task<IActionResult> GetAttributeInfoAsync(string layer, string node, string attribute)
        {
            var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/attributes/{attribute}/0.bin.gz");
            if (buffer == null)
                return NoContent();

            return File(buffer, "application/octet-stream; charset=binary");
        }

    }
}
