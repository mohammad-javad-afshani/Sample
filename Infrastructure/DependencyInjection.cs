using Application.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Customers;
using Persistence.Repositories;
using Domain.Addresses;
using Application.Notifications;
using Application.Payments;
using Domain.Orders;
using Domain.Payments;
using Domain.Products;
using Domain.Promotions;
using Persistence.ExternalServices;

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
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();

            services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["PaymentGateway:BaseUrl"] ?? "https://payments.example.local/");
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("PaymentGateway:TimeoutSeconds", 30));
            });

            services.AddHttpClient<IWebhookNotificationClient, WebhookNotificationClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Webhooks:BaseUrl"] ?? "https://hooks.example.local/");
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("Webhooks:TimeoutSeconds", 10));
            });

            return services;
        }
    }
}
