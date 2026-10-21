using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantCRM.BusinessLogic.Abstractions;
using RestaurantCRM.DataAccess.Data;
using RestaurantCRM.DataAccess.Repositories;

namespace RestaurantCRM.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RestaurantCrmContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IRepository, EfRepository>();
        return services;
    }
}
