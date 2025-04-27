using Microsoft.EntityFrameworkCore;
using RunAsyncAfterAddEndpoint.Core;
using RunAsyncAfterAddEndpoint.EFCore;
using System.Text.Json;

namespace RunAsyncAfterAddEndpoint.Route
{
    /// <summary>
    /// 路由实体
    /// </summary>
    public class RouteEntity()
    {
        public required int id { get; set; }
        public required string path { get; set; }

        public required string method { get; set; }

        public required string sql { get; set; }

        public required Dictionary<string, string> parameter { get; set; }

        public required string response { get; set; }

        public required bool authorization { get; set; }

        public required string version { get; set; }

        public required string introduction { get; set; }

        public required string createdBy { get; set; }

        public required DateTime createdAt { get; set; }
    }

    /// <summary>
    /// 路由实体
    /// </summary>
    public class RouteAggregateRoot(RouteEntityService service, RouteDBContext routeDB) : IAggregateRoot
    {
        public async Task<List<RouteEntity>> ToListAsync() => await routeDB.RouteEntities.ToListAsync();

        /// <summary>
        /// 通知修改
        /// </summary>
        public async Task NotificationChangeAsync(EndpointFactory endpointFactory) => await service.NotificationChangeAsync(endpointFactory);
    }
}
