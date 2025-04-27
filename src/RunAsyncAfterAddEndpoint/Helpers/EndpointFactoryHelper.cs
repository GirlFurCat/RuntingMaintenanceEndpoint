using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using RunAsyncAfterAddEndpoint.Route;

namespace RunAsyncAfterAddEndpoint.Helpers
{
    public class EndpointFactoryHelper
    {
        /// <summary>
        /// 生成代码脚本
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public string BuilderScript(RouteEntity route)
        {
            //将Parameter转为String
            string paraKeysStr = ParameterKeyStr(route.parameter);
            string paraValuesStr = ParameterValueStr(route.parameter);

            //获取需要执行的方法
            string method = GetMethod(route.method, route.response);

            string script = $@"
        (Delegate)(Func<{paraValuesStr}, Task<ActionResult>>)(async ({paraKeysStr}) => await apiTemplate.{method}(new {{{paraKeysStr}}}, ""{route.sql}""))";

            return script;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Method"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public HttpMethodMetadata BuilderHttpMethodMetadata(string Method)
        {
            switch (Method.ToUpper())
            {
                case "GET":
                    return new HttpMethodMetadata(new[] { HttpMethods.Get });

                case "POST":
                    return new HttpMethodMetadata(new[] { HttpMethods.Post });

                case "DELETE":
                    return new HttpMethodMetadata(new[] { HttpMethods.Delete });

                case "PUT":
                    return new HttpMethodMetadata(new[] { HttpMethods.Put });
            }
            throw new InvalidOperationException(nameof(EndpointFactory));
        }

        /// <summary>
        /// 生成默认授权策略
        /// </summary>
        public AuthorizeAttribute BuilderAuthorize => new AuthorizeAttribute();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="IsAuthorize"></param>
        /// <returns></returns>
        public EndpointMetadataCollection endpointMetadata(string method, bool IsAuthorize) => IsAuthorize ? new EndpointMetadataCollection(BuilderHttpMethodMetadata(method), BuilderAuthorize) : new EndpointMetadataCollection(BuilderHttpMethodMetadata(method));

        /// <summary>
        /// 将Values转化为字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ParameterValueStr(Dictionary<string, string> parameters)
        {
            return string.Join(", ", parameters.Values.Select(x => x.Split('|')[0]));
        }

        /// <summary>
        /// 将Keys转化为字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ParameterKeyStr(Dictionary<string, string> parameters)
        {
            return string.Join(", ", parameters.Keys);
        }

        /// <summary>
        /// 获取需要执行的方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string GetMethod(string method, string response)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    return response == "list" ? "GetPagesAsync" : "GetAsync";

                case "POST":
                    return "PostAsync";

                case "DELETE":
                    return "DeleteAsync";

                case "PUT":
                    return "PutAsync";
            }
            throw new InvalidOperationException(nameof(EndpointFactory));
        }
    }
}
