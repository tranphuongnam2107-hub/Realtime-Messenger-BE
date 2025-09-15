using System.Text;
using DataAccess.Context;
using DataAccess.DAOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Mapper;
using MongoDB.Driver;
using Repositories.Implement;
using Repositories.Interface;
using Services.Config.CloudinaryConfig;
using Services.Hubs;
using Services.Implement;
using Services.Interface;
using Services.PaswordHashing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("MongoDBSetting"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDBSetting>>().Value
);
builder.Services.AddSingleton<MongoDBContext>();
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var context = sp.GetRequiredService<MongoDBContext>();
    return context.Database!;
});


//DAO DI
builder.Services.AddAutoMapper(typeof(BaseMapper).Assembly);
builder.Services.AddScoped<PasswordHasher>();

builder.Services.AddScoped<AccountDAO>();
builder.Services.AddScoped<RoleDAO>();
builder.Services.AddScoped<ChatDAO>();
builder.Services.AddScoped<MessageDAO>();


//REPOSITORY DI
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

//SERVICE DI
builder.Services.AddScoped<CloudinaryHelper>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMessageService, MessageService>();

//CẤU HÌNH CLOUDINARY
builder.Services.Configure<CloudinarySetting>(
    builder.Configuration.GetSection("CloudinarySettings")
);
builder.Services.AddSingleton<CloudinaryConnection>();

// Cấu hình CORS nếu cần (cho phép client kết nối SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://127.0.0.1:5500", "https://localhost:7218") // thay bằng đúng origin đang test
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // khi dùng SignalR
    });
});

builder.Services.AddSignalR();

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();

//SECURITY
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = tokenValidationParameters;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer {token}' vào ô bên dưới (không có dấu ngoặc kép)",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    await next();
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.Run();
