using Microsoft.EntityFrameworkCore;
using MoviesDashboard.Persistence.Context;
using MoviesDashboard.Repositories;
using MoviesDashboard.Repositories.IRepositories;
namespace MoviesDashboard
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDIBusiness(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
