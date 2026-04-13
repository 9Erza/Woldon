using Woldon.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WakeOnLanSettings>(
    builder.Configuration.GetSection("WakeOnLanSettings"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();