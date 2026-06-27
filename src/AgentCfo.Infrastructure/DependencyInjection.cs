using AgentCfo.Core.Interfaces;
using AgentCfo.Infrastructure.Data;
using AgentCfo.Infrastructure.Data.Repositories;
using AgentCfo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentCfo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IAgentDecisionRepository, AgentDecisionRepository>();
        services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();

        // Services
        services.AddScoped<StripeWebhookService>();
        services.AddScoped<IRevenueMetricsService, RevenueMetricsService>();
        services.AddScoped<IForecastService, ForecastService>();

        return services;
    }
}
