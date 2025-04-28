using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
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
            string funcPara = FuncDynamicParaStr(route.parameter);

            //获取需要执行的方法
            string method = GetMethod(route.method, route.response);

            string script = $@"(Delegate)(Func<{paraValuesStr}, Task<ActionResult>>)(async ({paraKeysStr}) => await apiTemplate.{method}(new {{{funcPara}}}, ""{route.sql}""))";

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
        /// 生成默认描述
        /// </summary>
        public OpenApiOperation BuilderDescription(string description, Dictionary<string, string> para) => new OpenApiOperation() {
            Description = description,
            Parameters = BuilderParameter(para)
        };

        /// <summary>
        /// 生成参数描述
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public OpenApiParameter[] BuilderParameter(Dictionary<string, string> para) => para.Select(x => new OpenApiParameter()
        {
            Name = x.Key,
            Required = x.Value.IndexOf("?") == -1,
            In = ParameterLocation.Query
        }).ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="IsAuthorize"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public EndpointMetadataCollection endpointMetadata(RouteEntity route) => route.authorization ? new EndpointMetadataCollection(BuilderHttpMethodMetadata(route.method), BuilderDescription(route.introduction, route.parameter), BuilderAuthorize) : new EndpointMetadataCollection(BuilderHttpMethodMetadata(route.method), BuilderDescription(route.introduction, route.parameter));

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
        /// 创建方法运行时所需要的实体值，根据like判断是否支持模糊查询
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string FuncDynamicParaStr(Dictionary<string, string> parameters)
        {
            return string.Join(", ", parameters.Select(x =>
            {
                string[] para = x.Value.Split('|');
                return para.Length > 1 && para[1].ToLower() == "like" ? $@"{x.Key}=$""%{{{x.Key}}}%""" : $"{x.Key}";
            }));
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
