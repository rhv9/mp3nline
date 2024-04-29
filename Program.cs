using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.X509Certificates;
using youtube_dl_api.DB;
using youtube_dl_api.youtubemanager;


class Program
{
    static public void Main(string[] args)
    {

        // Set YT API Key
        YoutubeManager.YTAPI_KEY = File.ReadAllText(@"./ytapikey");
        Console.WriteLine($"YT_API_KEY: {YoutubeManager.YTAPI_KEY}");

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Youtube DL API", Description = "Api for youtube downloading.", Version = "v1" });
        });


        var app = builder.Build();

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Youtube DL API V1");
        });


        app.MapGet("/burgers/{id}", (int id) => BurgerDB.GetBurger(id));
        app.MapGet("/burgers", () => BurgerDB.GetBurgers());
        app.MapPost("/burgers", (Burger burger) => BurgerDB.CreateBurger(burger));
        app.MapPut(" /burgers", (Burger burger) => BurgerDB.CreateBurger(burger));
        app.MapDelete("/burgers/{id}", (int id) => BurgerDB.RemoveBurger(id));
        app.MapPost("/print", (string text) => Console.WriteLine(text));
        app.MapGet("/yt-request-song", (string url) => YoutubeManager.RequestDownloadSong(url));
        app.MapGet("/yt-ping-song-status", (int id) => YoutubeManager.GetSongStatus(id));
        app.MapGet("/yt-get-finished-song", (int id) => YoutubeManager.GetFinishedSong(id));
        app.MapGet("/test", () => { Console.WriteLine("カワシマ"); });


        app.Run();

    }
} 
