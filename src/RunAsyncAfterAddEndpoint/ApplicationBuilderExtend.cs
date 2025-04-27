using RunAsyncAfterAddEndpoint.apis;

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
            return app;
        }
    }
}
