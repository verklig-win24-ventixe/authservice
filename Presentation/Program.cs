using Microsoft.AspNetCore.Identity;
using Presentation.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Presentation.Data.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();

var keyVaultUrl = "https://verklig-ventixe-keyvault.vault.azure.net/";
builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());

var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
KeyVaultSecret dbSecret = await client.GetSecretAsync("DbConnectionString-Ventixe");
KeyVaultSecret jwtKeySecret = await client.GetSecretAsync("JwtPrivateKey");

builder.Services.AddDbContext<AuthDbContext>(x => x.UseSqlServer(dbSecret.Value));

var rsa = RSA.Create();
rsa.ImportFromPem(jwtKeySecret.Value.ToCharArray());

var issuer = builder.Configuration["JwtIssuer"];
var audience = builder.Configuration["JwtAudience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = issuer,
    ValidAudience = audience,
    IssuerSigningKey = new RsaSecurityKey(rsa)
  };
});

builder.Services.AddIdentity<UserEntity, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequiredLength = 8;
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
string[] roles = ["User"];

foreach (var role in roles)
{
  if (!await roleManager.RoleExistsAsync(role))
  {
    await roleManager.CreateAsync(new IdentityRole(role));
  }
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API");
  c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();