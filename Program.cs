using AuthentificationService.Application.Configurations;
using AuthentificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomConfiguration(builder.Environment);
builder.ConfigureLogging();

builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.AddJwtAuthorization(builder.Configuration);
builder.Services.AddMiddlearesAndSwagger();

builder.Services.AddEventPublisher();

var app = builder.Build();
app.AddSwagger();
app.AddMiddlewares();
app.Run();

