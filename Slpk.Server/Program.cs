using Microsoft.AspNetCore.Mvc;
using Slpk.Server.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ISlpkFileService, SlpkFileService>();
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapGet("api/{slpk}/SceneServer",
async ([FromRoute] string slpk, [FromServices] ISlpkFileService slpkFileService) =>
{
    var slpkFullPath = slpkFileService.GetFullPath(slpk);
    if (string.IsNullOrEmpty(slpkFullPath)) return Results.NotFound($"Can't found SLPK: {slpk}");

    var buffer = await slpkFileService.ReadAsync(slpkFullPath, "3dSceneLayer.json.gz");

    return Results.Ok(new
    {
        ServiceName = slpk,
        Name = slpk,
        CurrentVersion = 10.6,
        ServiceVersion = "1.6",
        SupportedBindings = new[] { "REST" },
        Layers = JsonSerializer.Deserialize<object>(buffer!)
    });
});

app.MapGet("api/{slpk}/SceneServer/layers/{layer}/nodes/{node}/geometries/0",
async ([FromRoute] string slpk, [FromRoute] string layer, [FromRoute] string node, [FromServices] ISlpkFileService slpkFileService) =>
{
    var slpkFullPath = slpkFileService.GetFullPath(slpk);
    if (string.IsNullOrEmpty(slpkFullPath)) return Results.NotFound($"Can't found SLPK: {slpk}");

    var buffer = await slpkFileService.ReadAsync(slpkFullPath, $"nodes/{node}/geometries/0.bin.gz");

    return Results.Bytes(buffer!);
});

app.Run();
