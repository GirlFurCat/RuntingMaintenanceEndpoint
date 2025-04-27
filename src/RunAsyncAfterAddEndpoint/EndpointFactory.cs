using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RunAsyncAfterAddEndpoint.apis;
using RunAsyncAfterAddEndpoint.Helpers;
using RunAsyncAfterAddEndpoint.Models;
using RunAsyncAfterAddEndpoint.Route;
using System.Linq.Expressions;
using System.Reflection;

namespace RunAsyncAfterAddEndpoint
{
    public class EndpointFactory(ApiTemplate apiTemplate, EndpointFactoryHelper helper, EndpointDataSource endpointDataSource)
    {
        public async Task<Endpoint> CreateAsync(RouteEntity route)
        {
            //创建地址
            var pattern = RoutePatternFactory.Parse(route.path);

            //生成脚本
            string scriptCode = helper.BuilderScript(route);

            // 执行脚本，返回委托对象
            var func = await CSharpScript.EvaluateAsync<Delegate>(scriptCode, ScriptOptions.Default
                .WithReferences(typeof(object).Assembly, typeof(Func<>).Assembly, Assembly.GetExecutingAssembly())
                .WithImports("System", 
                    "Microsoft.AspNetCore.Http", 
                    "RunAsyncAfterAddEndpoint.apis", 
                    "System.Threading.Tasks",
                    "Microsoft.AspNetCore.Mvc"),
                globals: new RoslynGlobalsModel() { apiTemplate = apiTemplate }
                );

            //创建委托
            var requestDelegate = RequestDelegateFactory.Create(func, new RequestDelegateFactoryOptions()).RequestDelegate;

            //构建终结端点
            var endpoint = new RouteEndpoint(
                requestDelegate,
                pattern,
                order: endpointDataSource.Endpoints.Count + 1,
                new EndpointMetadataCollection(helper.BuilderHttpMethodMetadata(route.method)),
                displayName: $"{route.method}-{route.path}"
            );

            return endpoint;
        }

        
    }
}
