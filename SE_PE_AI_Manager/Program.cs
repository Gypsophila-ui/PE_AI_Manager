var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

// 启用跨域中间件
app.UseCors("AllowVueApp");

// 只在非开发环境启用HTTPS重定向
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

//app.Run();



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:5001", "http://118.25.145.4:5001")
//                  .AllowAnyMethod()
//                  .AllowAnyHeader()
//                  .AllowCredentials();
//        });
//});

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


//if (builder.Environment.IsDevelopment())
//{
//    builder.WebHost.UseUrls("http://*:5001"); // 明确指定开发端口
//}
//// 根据环境配置URL
//if (!builder.Environment.IsDevelopment())
//{
//    builder.WebHost.UseUrls("http://*:5001");
//}

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCors("AllowSpecificOrigin");

//// 生产环境建议移除HTTPS重定向或正确配置
//// app.UseHttpsRedirection();

//app.UseAuthorization();
//app.MapControllers();
//app.Run();