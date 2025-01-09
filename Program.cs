using AuthentificationService.Application.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomConfiguration(builder.Environment);
builder.ConfigureLogging();

builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.AddJwtAuthorization(builder.Configuration);
builder.Services.AddMiddlearesAndSwagger();

var app = builder.Build();
app.AddSwagger();
app.AddMiddlewares();
app.Run();

