//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowSpecificOrigin",
////        policy =>
////        {
////            // 允许的前端源，使用IP地址
////            policy.WithOrigins("http://localhost:5001", "http://118.25.145.4:5001") // 假设前端通过IP地址的5001端口访问
////                  .AllowAnyMethod() // 允许所有HTTP方法
////                  .AllowAnyHeader() // 允许所有头
////                  .AllowCredentials(); // 如果需要凭证（如cookies）
////        });
////});

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
////builder.WebHost.UseUrls("http://*:5001"); 

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5001", "http://118.25.145.4:5001")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://*:5001"); // 明确指定开发端口
}
// 根据环境配置URL
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://*:5001");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

// 生产环境建议移除HTTPS重定向或正确配置
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();