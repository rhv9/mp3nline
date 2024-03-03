using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using youtube_dl_api.DB;
using youtube_dl_api.youtubemanager;


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
app.MapGet("/yt-get-song", (string url) => YoutubeManager.GetSong(url));


app.Run();
