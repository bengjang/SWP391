using lamlai.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với Dependency Injection
builder.Services.AddDbContext<TestContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthorization();

app.MapControllers();

// Add endpoint để test kết nối
app.MapGet("/api/test", () => "Backend is running!");

app.Run();
