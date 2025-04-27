using RunAsyncAfterAddEndpoint.Route;
using System.Security.Cryptography;

namespace RunAsyncAfterAddEndpoint.BackgroundServices
{
    public class CheckRouteDatabgService(IServiceProvider service, RouteEntity routeEntity) : BackgroundService
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
