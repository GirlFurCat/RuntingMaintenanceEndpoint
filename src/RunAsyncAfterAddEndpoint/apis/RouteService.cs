using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunAsyncAfterAddEndpoint.EFCore;
using RunAsyncAfterAddEndpoint.Route;

namespace RunAsyncAfterAddEndpoint.apis;

public class RouteService(RouteDBContext routeDbContext)
{
    public async Task<List<RouteEntity>> GetRoutesAsync() => await routeDbContext.RouteEntities.ToListAsync();
    
    public async Task<bool> AddRouteAsync(RouteEntity routeEntity)
    {
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

    public async Task<bool> UpdateRouteAsync(RouteEntity routeEntity)
    {
        routeDbContext.Update(routeEntity);
        return await routeDbContext.SaveChangesAsync() == 1;
    }
}