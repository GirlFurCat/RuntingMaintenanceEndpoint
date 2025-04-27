namespace RunAsyncAfterAddEndpoint.Models
{
    public class JwtSettingsModel
    {
        public required string SecretKey { get; set; } // 用于加密签名的密钥
        public required string Issuer { get; set; }     // JWT 的颁发者
        public required string Audience { get; set; }   // JWT 的受众
        public int AccessTokenExpiration { get; set; } // 访问令牌过期时间（分钟）
        public int RefreshTokenExpiration { get; set; } // 刷新令牌过期时间（分钟）
    }
}
