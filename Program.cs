using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using systrack_api.Converters;
using System.Text;
using SystrackApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 11))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builderPolicy =>
    {
        builderPolicy.WithOrigins("https://witty-grass-0ea828d03.4.azurestaticapps.net")
                     .AllowAnyHeader()
                     .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1z/uLrPhxqSBMfgArAQpslMwlbOAUVdVU3PB1onVDKc=")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Systrack API", 
        Version = "v1",
        Description = "API",
        Contact = new OpenApiContact
        {
            Name = "sysTrack",
            Email = "contact@systrack.com",
            Url = new Uri("https://witty-grass-0ea828d03.4.azurestaticapps.net"),
        }
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Systrack API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapControllers();

app.Run();
