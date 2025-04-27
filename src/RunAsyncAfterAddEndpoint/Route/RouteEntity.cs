using System.Runtime.CompilerServices;
using System.Text.Json;

namespace RunAsyncAfterAddEndpoint.Route
{
    /// <summary>
    /// 路由实体
    /// </summary>
    public class RouteEntity
    {
        public required string path { get; set; }

        public required string method { get; set; }

        public required string sql { get; set; }

        public required Dictionary<string, string> parameter { get; set; }

        public required string response { get; set; }

        public required bool authorization { get; set; }

        public required string version { get; set; }

        public required string introduction { get; set; }

        public required string modelName { get; set; }

        public async Task<List<RouteEntity>> BuildRouteAsync(CancellationToken? token = null)
        {
            List<RouteEntity> entities = JsonSerializer.Deserialize<List<RouteEntity>>(await File.ReadAllTextAsync("database/routeData.json", System.Text.Encoding.UTF8, token ?? CancellationToken.None))!;
            Parallel.ForEach(entities, x => { x.path = x.path.Replace("{version}", x.version); });
            return entities.GroupBy(x =>new { x.path, x.method }).Any(x => x.Count() > 1) ? throw new InvalidOperationException("出现重复Path") : entities;
        }

        private List<RouteEntity> _OldEntitys = new List<RouteEntity>();
        public async Task<(List<RouteEntity>, List<RouteEntity>)> CheckUpdateAsync(CancellationToken? token = null)
        {
            List<RouteEntity> newEntitys = await BuildRouteAsync(token);

            List<RouteEntity> addEntity = newEntitys.Where(x => !_OldEntitys.Any(y => y.path == x.path && y.method == x.method)).ToList();
            List<RouteEntity> removeEntity = _OldEntitys.Where(x=> !newEntitys.Any(y => y.path == x.path && y.method == x.method)).ToList();

            //修改过其他参数项
            List<RouteEntity> modify = _OldEntitys.Where(x => !newEntitys.Any(y => JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y))).ToList();
            addEntity.AddRange(newEntitys.Where(x=> modify.Any(y => y.path == x.path && y.method == x.method)).ToList());
            removeEntity.AddRange(modify);

            _OldEntitys.Clear();
            _OldEntitys.AddRange(newEntitys);
            return (addEntity, removeEntity);
        }

        /// <summary>
        /// 通知修改
        /// </summary>
        public async Task NotificationChangeAsync(EndpointFactory endpointFactory) {
            if (ChangeDataActive != null) 
                ChangeDataActive.Invoke(await CheckUpdateAsync(), endpointFactory);
        }

        public event Action<(List<RouteEntity>, List<RouteEntity>), EndpointFactory>? ChangeDataActive;
    }
}
