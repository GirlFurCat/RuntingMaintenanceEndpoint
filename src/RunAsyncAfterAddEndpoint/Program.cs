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
builder.Services.AddHostedService<CheckRouteDatabgService>();

//���DBContext
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

// ��ȡ��̬����Դʵ��
var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();

// ����̬����Դ����˵�����Դ����
app.UseEndpoints(endpoints =>
{
    endpoints.DataSources.Add(dynamicDataSource);
});
app.Run();