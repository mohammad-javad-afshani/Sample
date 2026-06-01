using Application.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Customers;
using Persistence.Repositories;
using Domain.Addresses;
using Domain.Products;

namespace Persistence

{
    public static class DependencyInjection
    {         
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
           
            services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
           
            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IUnitOfWork>(sp =>
               sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            services.AddScoped<IAddressRepository, AddressRepository>();

            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
