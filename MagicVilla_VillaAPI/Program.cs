using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Connect with Database
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
   option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

//Add serilog logger
//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
//    .WriteTo.File("log/villaLogs.txt", rollingInterval: RollingInterval.Day).CreateLogger();

//builder.Host.UseSerilog();

builder.Services.AddControllers(option =>
{
    //to not accept xml format for example
    option.ReturnHttpNotAcceptable= true;
} ).AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//AddSingleton is created when the application starts and this object will be used everytime the application requests an implementation
//AddScoped  means for every request it will create a new object
//AddTransient means every time that object is accessed it will be create different object and assign it where is needed
builder.Services.AddSingleton<ILogging, Logging>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
