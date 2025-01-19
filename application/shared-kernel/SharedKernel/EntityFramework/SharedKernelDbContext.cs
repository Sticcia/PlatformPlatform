using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PlatformPlatform.SharedKernel.Domain;
using PlatformPlatform.SharedKernel.ExecutionContext;

namespace PlatformPlatform.SharedKernel.EntityFramework;

/// <summary>
///     The SharedKernelDbContext class represents the Entity Framework Core DbContext for managing data access to the
///     database, like creation, querying, and updating of <see cref="IAggregateRoot" /> entities.
/// </summary>
public abstract class SharedKernelDbContext<TContext>(DbContextOptions<TContext> options, IExecutionContext executionContext)
    : DbContext(options) where TContext : DbContext
{
    protected TenantId? TenantId => executionContext.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.AddInterceptors(new UpdateAuditableEntitiesInterceptor());

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TContext).Assembly);

        // Ensures that all enum properties are stored as strings in the database.
        modelBuilder.UseStringForEnums();

        ApplyGlobalTenantFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    ///     Applies global tenant filters to all entities implementing <see cref="ITenantScopedEntity" /> interface.
    ///     This ensures that only data belonging to the current tenant is queried.
    /// </summary>
    private void ApplyGlobalTenantFilters(ModelBuilder modelBuilder)
    {
        var tenantScopedEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ITenantScopedEntity).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType);

        foreach (var entityType in tenantScopedEntityTypes)
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var tenantIdProperty = Expression.Property(parameter, nameof(ITenantScopedEntity.TenantId));
            var tenantIdValue = Expression.Property(Expression.Constant(this), nameof(TenantId));

            var condition = Expression.AndAlso(
                Expression.NotEqual(tenantIdValue, Expression.Constant(null, typeof(TenantId))),
                Expression.Equal(tenantIdProperty, tenantIdValue)
            );

            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType).HasQueryFilter(lambda);
        }
    }
}
