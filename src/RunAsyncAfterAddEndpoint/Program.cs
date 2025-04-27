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
using RunAsyncAfterAddEndpoint.AppSetting;
using RunAsyncAfterAddEndpoint.database;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<DynamicEndpointDataSource>();
builder.Services.AddSingleton<RouteEntity>();
builder.Services.AddSingleton<AppSetting>();
builder.Services.AddSingleton<DapperHelper>();
builder.Services.AddScoped<ApiTemplate>();
builder.Services.AddScoped<EndpointFactoryHelper>();
builder.Services.AddScoped<EndpointFactory>();
builder.Services.AddScoped<TokenService>();

// �����֤������ JWT Bearer Ϊ����
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

// �����Ȩ����
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

//��Ӷ˵���º�̨
builder.Services.AddHostedService<EndpointHostedService>();

//��Ӵ����������񣨿�ɾ��������ѡ�񴥷���ʽ��
builder.Services.AddHostedService<CheckRouteData>();

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

// ��ȡ��̬����Դʵ��
var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();

// ����̬����Դ����˵�����Դ����
app.UseEndpoints(endpoints =>
{
    endpoints.DataSources.Add(dynamicDataSource);
});
app.Run();

class CheckRouteData(IServiceProvider service, RouteEntity routeEntity) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = service.CreateScope();
        EndpointFactory endpointFactory = scope.ServiceProvider.GetRequiredService<EndpointFactory>();
        string oldFileHash = string.Empty;
        while (true)
        {
            string newFileHash = GetFileHash("database/routeData.json");

            if (newFileHash != oldFileHash)
            {
                await routeEntity.NotificationChangeAsync(endpointFactory);
                oldFileHash = newFileHash;
            }
            await Task.Delay(2000);
        }
    }

    /// <summary>
    /// ��ȡ�ļ�Hashֵ
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string GetFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }
}