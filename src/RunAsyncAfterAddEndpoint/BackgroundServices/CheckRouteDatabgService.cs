using RunAsyncAfterAddEndpoint.Route;
using System.Security.Cryptography;

namespace RunAsyncAfterAddEndpoint.BackgroundServices
{
    public class CheckRouteDatabgService(IServiceProvider service) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scope = service.CreateScope();
            EndpointFactory endpointFactory = scope.ServiceProvider.GetRequiredService<EndpointFactory>();
            RouteAggregateRoot routeRoot = scope.ServiceProvider.GetRequiredService<RouteAggregateRoot>();
            string oldFileHash = string.Empty;
            while (true)
            {
                string newFileHash = GetFileHash("database/routeData.json");

                if (newFileHash != oldFileHash)
                {
                    await routeRoot.NotificationChangeAsync(endpointFactory);
                    oldFileHash = newFileHash;
                }
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 获取文件Hash值
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
}
