using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<DataContext>(options =>
                options
                      .UseLazyLoadingProxies()
                      .UseSqlServer(connectionString));

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
