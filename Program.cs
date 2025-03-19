using lamlai.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAPI_FlowerShopSWP.Configurations;


var builder = WebApplication.CreateBuilder(args);

// Register IMemoryCache
builder.Services.AddMemoryCache();

// Hiển thị chuỗi kết nối để debug
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

// Đăng ký DbContext với Dependency Injection
builder.Services.AddDbContext<TestContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Thêm logging để xem các câu lệnh SQL
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(origin => true) // Cho phép tất cả origin trong development
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Cho phép credentials (cookies, auth headers)
    });
});

// Add services to the container.a
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Không phân biệt hoa thường
    options.JsonSerializerOptions.WriteIndented = true; // Format JSON đẹp hơn

    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Đăng ký Cloudinary Settings
builder.Services.Configure<SWP391.Models.CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoService
builder.Services.AddScoped<SWP391.Services.IPhotoService, SWP391.Services.PhotoService>();

// Register VNPayConfig
builder.Services.Configure<VNPayConfig>(builder.Configuration.GetSection("VNPay"));

// Register VNPayService
builder.Services.AddScoped<VNPayService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Thêm middleware xử lý lỗi
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseHttpsRedirection();

// Use CORS before auth and endpoints
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Thêm middleware phục vụ tệp tĩnh từ wwwroot
app.UseStaticFiles();

app.MapControllers();

// Add endpoint để test kết nối
app.MapGet("/api/test", () => "Backend is running!");

app.Run();
