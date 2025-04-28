using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RunAsyncAfterAddEndpoint.Configuration;
using RunAsyncAfterAddEndpoint.Route;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace RunAsyncAfterAddEndpoint.EFCore
{
    public class RouteDBContext(Configuration.AppSetting appSetting) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(appSetting.WriteConnectionStr);
            base.OnConfiguring(optionsBuilder);
        }

        public required DbSet<RouteEntity> RouteEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //配置Json参数
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            modelBuilder.Entity<RouteEntity>(entity =>
            {
                entity.ToTable("RouteEntity");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.path).HasMaxLength(255).IsRequired();
                entity.Property(e => e.method).HasMaxLength(10).IsRequired();
                entity.Property(e => e.sql).HasColumnType("varchar(max)").IsRequired();
                entity.Property(e => e.parameter).HasColumnType("varchar(max)").IsRequired().HasConversion(
                    v => JsonSerializer.Serialize(v, options), 
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, options)!,
                    new ValueComparer<Dictionary<string, string>>(
                        (c1, c2) => JsonSerializer.Serialize(c1, options) == JsonSerializer.Serialize(c2, options),
                        c => c == null ? 0 : JsonSerializer.Serialize(c, options).GetHashCode(),
                        c => c
                    )
                );
                entity.Property(x => x.response).HasMaxLength(10).IsRequired();
                entity.Property(x => x.authorization).IsRequired();
                entity.Property(x => x.version).HasMaxLength(5).IsRequired();
                entity.Property(x => x.introduction).HasMaxLength(255).IsRequired();
                entity.Property(x => x.createdBy).HasMaxLength(10).IsRequired();
                entity.Property(x => x.createdAt).IsRequired();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
