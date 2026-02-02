using API_Service.AppData;
using API_Service.AppData.DataService;
using API_Service.Models.Entities;
using API_Service.RepositoryLayer.Interface;
using API_Service.RepositoryLayer.Repository;
using DataContext.DataProvider;
using DataContext.DataService;
using DataContext.SampleData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.AddLog4Net();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy to allow MVC web app to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:44328") // Your web app URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<IDataProvider, AccountData>();
builder.Services.AddSingleton<IDataService, AccountData>();
builder.Services.AddSingleton<IService<User>, SampleDataService<User>>();
builder.Services.AddSingleton<IService<Account>, SampleDataService<Account>>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowWebApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
