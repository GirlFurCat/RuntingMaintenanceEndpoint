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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(builder =>
{
    builder.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Swagger API",
        Version = "v1"
    });
});

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
builder.Services.AddScoped<BuilderSwaggerDoc>();
builder.Services.AddScoped<RouteService>();

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

//���DBContext
builder.Services.AddDbContext<RouteDBContext>();

var app = builder.Build();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();

    app.UseSwagger();
    // ���� Swagger UI
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("Swagger/swagger.json", "Swagger API V1 Docs");
        //option.SwaggerEndpoint("/openapi/v1.json?t={timestamp}", "OpenAPI V1 Docs");
        option.RoutePrefix = string.Empty;
        option.DocumentTitle = "SparkTodo API";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAddMinalAPI();

// ��ȡ��̬����Դʵ��������̬����Դ����˵�����Դ����
var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();
app.UseEndpoints(endpoints =>
{
    endpoints.DataSources.Add(dynamicDataSource);
});

app.Run();