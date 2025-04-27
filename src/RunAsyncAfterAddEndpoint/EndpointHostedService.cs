
using RunAsyncAfterAddEndpoint.apis;
using RunAsyncAfterAddEndpoint.Route;
using System.Text.Json;

namespace RunAsyncAfterAddEndpoint
{
    public class EndpointHostedService(DynamicEndpointDataSource _endpointDataSource, RouteEntity routeEntitys, IServiceProvider service) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = service.CreateScope();
            ApiTemplate apiTemplate = scope.ServiceProvider.GetRequiredService<ApiTemplate>();
            EndpointFactory endpointFactory = scope.ServiceProvider.GetRequiredService<EndpointFactory>();

            routeEntitys.ChangeDataActive += RouteEntitys_ChangeDataActive;
        }

        private void RouteEntitys_ChangeDataActive((List<RouteEntity> addEndpoint, List<RouteEntity> removeEndpoint) obj, EndpointFactory endpointFactory)
        {
            //移除旧端点
            obj.removeEndpoint.ForEach(x => _endpointDataSource.RemoveEndpoint($"{x.method}-{x.path}"));

            //添加新端点
            obj.addEndpoint.ForEach(async x => _endpointDataSource.AddEndpoint(await endpointFactory.CreateAsync(x)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
