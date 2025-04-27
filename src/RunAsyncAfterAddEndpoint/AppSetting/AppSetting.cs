namespace RunAsyncAfterAddEndpoint.AppSetting
{
    public record AppSetting(IConfiguration _configuration)
    {
        public string AuthorizationKey => _configuration.GetValue<string>("JwtSettings:SecretKey") ?? throw new ArgumentNullException();

        public string ReadConnectionStr => _configuration.GetValue<string>("ConnectionStrings:ReadConnectionStr") ?? throw new ArgumentNullException();

        public string WriteConnectionStr => _configuration.GetValue<string>("ConnectionStrings:WriteConnectionStr") ?? throw new ArgumentNullException();
    }
}
