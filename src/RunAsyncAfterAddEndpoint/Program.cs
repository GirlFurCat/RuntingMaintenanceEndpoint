using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using RunAsyncAfterAddEndpoint;
using RunAsyncAfterAddEndpoint.apis;
using RunAsyncAfterAddEndpoint.Configuration;
using RunAsyncAfterAddEndpoint.BackgroundServices;
using RunAsyncAfterAddEndpoint.Helpers;
using RunAsyncAfterAddEndpoint.Models;
using RunAsyncAfterAddEndpoint.Route;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using RunAsyncAfterAddEndpoint.EFCore;
using Microsoft.EntityFrameworkCore;
using RunAsyncAfterAddEndpoint.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<DynamicEndpointDataSource>();
builder.Services.AddSingleton<RouteEntityService>();
builder.Services.AddSingleton<AppSetting>();
builder.Services.AddSingleton<DapperHelper>();
builder.Services.AddScoped<RouteEntity>();
builder.Services.AddScoped<ApiTemplate>();
builder.Services.AddScoped<EndpointFactoryHelper>();
builder.Services.AddScoped<EndpointFactory>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<RouteAggregateRoot>();
builder.Services.AddScoped<RouteDBContext>();

// 添加认证服务（以 JWT Bearer 为例）
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer")!,
        ValidAudience = builder.Configuration.GetValue<string>("JwtSettings:Audience")!,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SecretKey")!))
    };
});

// 添加授权服务
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

//添加端点更新后台
builder.Services.AddHostedService<EndpointHostedService>();

//添加触发更新任务（可删除，自行选择触发方式）
builder.Services.AddHostedService<CheckRouteDatabgService>();

//添加DBContext
builder.Services.AddDbContext<RouteDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAddMinalAPI();

// 获取动态数据源实例
var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();

// 将动态数据源加入端点数据源集合
app.UseEndpoints(endpoints =>
{
    endpoints.DataSources.Add(dynamicDataSource);
});
app.Run();