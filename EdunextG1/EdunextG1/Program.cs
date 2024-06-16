using EdunextG1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EdunextG1.Helper;
using Azure.Storage.Blobs;
using EdunextG1.Services.IServices;
using EdunextG1.Repository.IRepository;
using EdunextG1.Repository;
using EdunextG1.Middleware;
using EdunextG1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL
builder.Services.AddDbContext<DatabaseContext>(options => 
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectDB"));
});

// JWT 
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JWT")["Key"]);
builder.Services.AddAuthentication(x =>
{
    // Default Authentication Scheme
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(j =>
{
    // ---- Turn off HTTPS for development
    j.RequireHttpsMetadata = false;
    // ---- Save Token "JWT" in to Attribute of "Authorization" by AuthenticationProperties
    j.SaveToken = true;
    j.TokenValidationParameters = new TokenValidationParameters
    {
        // =--------------------- Verify the Token Key 
        ValidateIssuerSigningKey = true,
        // ** Thiết lập khóa bí mật để giải mã token
        IssuerSigningKey = new SymmetricSecurityKey(key),
        // = --------------------- Verify the Token Issuer
        ValidateIssuer = false,
        // = --------------------- Verify the Token Audience
        ValidateAudience = false,
        // = --------------------- Turn On Token Lifetime
        ValidateLifetime = true,
        // = --------------------- Verify the Token Lifetime
        // = --------------------- Set the Clock Skew
        ClockSkew = TimeSpan.Zero
    };
});

// Azure BlobServiceClient cho AzureBlobStorage
builder.Services.AddSingleton(ab =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlob")
));


// Dependency Injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBlobService, BlobService>();

// ---------------------------

// Đăng ký JwtHelper
builder.Services.AddSingleton<JWT>();

// --- Configuration for all Http access API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// --- Middlewares 
app.UseMiddleware<AdminMiddleware>();
// ---------------

app.MapControllers();

app.Run();
