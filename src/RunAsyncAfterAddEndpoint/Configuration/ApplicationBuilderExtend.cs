using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RunAsyncAfterAddEndpoint.apis;
using RunAsyncAfterAddEndpoint.Core;
using RunAsyncAfterAddEndpoint.Models.dto;
using RunAsyncAfterAddEndpoint.Route;

namespace RunAsyncAfterAddEndpoint.Configuration
{
    public static class ApplicationBuilderExtend
    {
        public static IEndpointRouteBuilder UseAddMinalAPI(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/token", (TokenService tokenService) =>
            {
                return tokenService.GenerateAccessToken("1");
            }).WithOpenApi(x => new Microsoft.OpenApi.Models.OpenApiOperation()
            {
                Description = "获取令牌"
            });

            app.MapPost("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, RouteEntityDto routeEntity) =>
            {
                if (await service.AddRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithOpenApi(x => new Microsoft.OpenApi.Models.OpenApiOperation()
            {
                Description = "增加新路由"
            });

            app.MapPut("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, RouteEntityDto routeEntity) =>
            {
                if (await service.UpdateRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithOpenApi(x => new Microsoft.OpenApi.Models.OpenApiOperation()
            {
                Description = "更新指定路由"
            });

            app.MapDelete("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, int id) =>
            {
                if (await service.DeleteRouteAsync(id))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithOpenApi(x => new Microsoft.OpenApi.Models.OpenApiOperation()
            {
                Description = "删除指定路由"
            });

            return app;
        }
    }
}
