using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RunAsyncAfterAddEndpoint.apis;
using RunAsyncAfterAddEndpoint.Core;
using RunAsyncAfterAddEndpoint.Route;

namespace RunAsyncAfterAddEndpoint
{
    public static class ApplicationBuilderExtend
    {
        public static IEndpointRouteBuilder UseAddMinalAPI(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/token", (TokenService tokenService) =>
            {
                return tokenService.GenerateAccessToken("1");
            })
            .WithName("GetWeatherForecast");

            app.MapPost("/api/admin/route", async ([FromServices]RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory  ,[FromForm]RouteEntity routeEntity) =>
            {
                if (await service.AddRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            });
            
            app.MapPut("/api/admin/route", async ([FromServices]RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory  ,[FromForm]RouteEntity routeEntity) =>
            {
                if (await service.UpdateRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            });
            
            app.MapDelete("/api/admin/route", async ([FromServices]RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory  ,int id) =>
            {
                if (await service.DeleteRouteAsync(id))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            });
            return app;
        }
    }
}
