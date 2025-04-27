using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Primitives;
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

builder.Services.AddHostedService<AddEndpointHost>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
}
app.UseHttpsRedirection();

// 获取动态数据源实例
var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();

// 将动态数据源加入端点数据源集合
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.DataSources.Add(dynamicDataSource);
});
app.Run();

class AddEndpointHost(DynamicEndpointDataSource _endpointDataSource, RouteEntity routeEntitys, IServiceProvider service) : BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = service.CreateScope();
        ApiTemplate apiTemplate = scope.ServiceProvider.GetRequiredService<ApiTemplate>();
        EndpointFactory endpointFactory = scope.ServiceProvider.GetRequiredService<EndpointFactory>();

        while (true)
        {
            EndpointDataSource _EndpointDataSource = scope.ServiceProvider.GetRequiredService<EndpointDataSource>();
            try
            {
                //获取需要变更的端点
                (List<RouteEntity> addEndpoint, List<RouteEntity> removeEndpoint) = await routeEntitys.CheckUpdateAsync(stoppingToken);

                //移除旧端点
                removeEndpoint.ForEach(x => _endpointDataSource.RemoveEndpoint($"{x.method}-{x.path}"));

                //添加新端点
                addEndpoint.ForEach(async x => _endpointDataSource.AddEndpoint(await endpointFactory.CreateAsync(x)));

                await Task.Delay(3000);//假定3秒刷新一次路由
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonSerializer.Serialize(ex));
            }
        }
    }
}

