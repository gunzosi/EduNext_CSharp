using EdunextG1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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


// ---------------

app.MapControllers();

app.Run();
