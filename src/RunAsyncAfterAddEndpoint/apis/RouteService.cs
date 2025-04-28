using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunAsyncAfterAddEndpoint.EFCore;
using RunAsyncAfterAddEndpoint.Models.dto;
using RunAsyncAfterAddEndpoint.Route;
using System.Reflection;

namespace RunAsyncAfterAddEndpoint.apis;

public class RouteService(RouteDBContext routeDbContext)
{
    public async Task<List<RouteEntity>> GetRoutesAsync() => await routeDbContext.RouteEntities.ToListAsync();
    
    public async Task<bool> AddRouteAsync(RouteEntityDto routeEntityDto)
    {
        RouteEntity routeEntity = CheckChange(routeEntityDto);
        await routeDbContext.RouteEntities.AddAsync(routeEntity);
        return await routeDbContext.SaveChangesAsync() == 1;
    }

    public async Task<bool> DeleteRouteAsync(int id)
    {
        var routeEntity = await routeDbContext.RouteEntities.FindAsync(id);
        if (routeEntity == null) return false;
        routeDbContext.RouteEntities.Remove(routeEntity);
        return await routeDbContext.SaveChangesAsync() == 1;
    }

    public async Task<bool> UpdateRouteAsync(RouteEntityDto routeEntityDto)
    {
        var entity = await routeDbContext.RouteEntities.FindAsync(routeEntityDto.id);
        if (entity == null) return false;
        entity = CheckChange(routeEntityDto, entity);

        routeDbContext.Update(entity);
        return await routeDbContext.SaveChangesAsync() == 1;
    }

    /// <summary>
    /// dto修改过的参数映射到实体
    /// </summary>
    /// <param name="routeEntityDto"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    private RouteEntity CheckChange(RouteEntityDto routeEntityDto, RouteEntity? entity = null)
    {
        if (entity == null)
            entity = new RouteEntity() { id = 0, authorization = false, createdAt = DateTime.Now, createdBy = string.Empty, introduction = string.Empty, parameter = new Dictionary<string, string>(), method = string.Empty, path = string.Empty, response = string.Empty, sql = string.Empty, version = string.Empty };

        PropertyInfo[] dtoPropertys = typeof(RouteEntityDto).GetProperties();
        PropertyInfo[] entityPropertys = typeof(RouteEntity).GetProperties();
        foreach (var dtoProperty in dtoPropertys)
        {
            var entityProperty = entityPropertys.FirstOrDefault(x => x.Name == dtoProperty.Name);
            if (entityProperty == null) continue;

            var dtoValue = dtoProperty.GetValue(routeEntityDto);
            var entityValue = entityProperty.GetValue(entity);
            if (dtoValue != null && !dtoValue.Equals(entityValue))
            {
                entityProperty.SetValue(entity, dtoValue);
            }
        }
        return entity;
    }
}