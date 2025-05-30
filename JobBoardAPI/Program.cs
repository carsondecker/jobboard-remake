using JobBoardAPI.Config;
using JobBoardAPI.Data;
using JobBoardAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

DotNetEnv.Env.Load();

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json")
    .Build();

var connection_string = config["CONN_STRING"] ?? throw new Exception("could not get db connection string from .env");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connection_string, sqlOptions => sqlOptions.EnableRetryOnFailure())
);

var secret = config["JWT_SECRET"] ?? throw new Exception("could not get JWT secret from .env");

var jwtSettings = new JwtSettings
{
    Secret = secret,
    Issuer = config["Jwt:Issuer"] ?? throw new Exception("could not get JWT issuer from appsettings.json"),
    Audience = config["Jwt:Audience"] ?? throw new Exception("could not get JWT audience from appsettings.json"),
    ExpiryMinutes = int.Parse(config["Jwt:ExpiresInMinutes"] ?? throw new Exception("could not get JWT audience from appsettings.json"))
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidAudience = jwtSettings.Audience,
            ValidIssuer = jwtSettings.Issuer
        };
    });

var app = builder.Build();

// TODO: Change to add migrations later
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
