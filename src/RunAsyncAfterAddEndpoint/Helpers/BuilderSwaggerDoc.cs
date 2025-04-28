
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace RunAsyncAfterAddEndpoint.Helpers
{
    public class BuilderSwaggerDoc(EndpointDataSource endpointData)
    {
        public async Task InvokeAsync()
        {
            //检查Json文件
            string filePath = Path.Combine("wwwroot", "Swagger", "swagger.json");
            if (File.Exists(filePath))
                File.Delete(filePath);

            // 创建 OpenAPI 文档对象
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Auto Generated API",
                    Version = "v1"
                },
                Paths = new OpenApiPaths()
            };

            // 获取终结点
            var endpoints = endpointData.Endpoints;
            foreach (RouteEndpoint routeEndpoint in endpoints)
            {
                string routePattern = routeEndpoint.RoutePattern.RawText ?? "/unknown";
                OpenApiOperation openApiOperation = routeEndpoint.Metadata.GetMetadata<OpenApiOperation>()!;

                // 提取参数
                var parameters = new List<OpenApiParameter>();
                foreach (var param in openApiOperation.Parameters)
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = param.Name,
                        In = ParameterLocation.Path,
                        Required = param.Required,
                    });
                }


                // 添加到文档中
                if (!document.Paths.ContainsKey(routePattern))
                {
                    document.Paths.Add(routePattern, new OpenApiPathItem
                    {
                        Operations =
                        {
                            [GetOperationType(routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.FirstOrDefault()??string.Empty)] = new OpenApiOperation {
                                Summary = openApiOperation.Description??"无描述",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "Success"
                                    }
                                },
                                Parameters = parameters
                            }
                        }
                    });
                }
                else
                {
                    // 如果路径已经存在，更新现有的操作或做其他处理
                    var existingPathItem = document.Paths[routePattern];
                    existingPathItem.Operations[GetOperationType(routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.FirstOrDefault() ?? string.Empty)] = new OpenApiOperation
                    {
                        Summary = openApiOperation.Description ?? "无描述",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Success"
                            }
                        },
                        Parameters = parameters
                    };
                }
            }

            // 序列化 OpenAPI 文档到 JSON
            // 使用 using 语句确保所有资源正确释放
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    var writer = new OpenApiJsonWriter(streamWriter);
                    document.SerializeAsV3(writer);

                    await stream.FlushAsync();
                    writer.Flush();
                }
            }
        }

        private OperationType GetOperationType(string Method)
        {
            switch (Method)
            {
                case "GET": return OperationType.Get;
                case "POST": return OperationType.Post;
                case "PUT": return OperationType.Put;
                case "DELETE": return OperationType.Delete;
                default: return OperationType.Get;
            }
        }
    }
}
