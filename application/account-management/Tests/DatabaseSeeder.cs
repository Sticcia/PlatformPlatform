using PlatformPlatform.AccountManagement.Database;
using PlatformPlatform.AccountManagement.Features.Tenants.Domain;
using PlatformPlatform.AccountManagement.Features.Users.Domain;
using PlatformPlatform.SharedKernel.Domain;

namespace PlatformPlatform.AccountManagement.Tests;

public sealed class DatabaseSeeder
{
    public readonly Tenant Tenant1;
    public readonly User User1;

    public DatabaseSeeder(AccountManagementDbContext accountManagementDbContext)
    {
        var tenantId1 = TenantId.NewId();
        Tenant1 = Tenant.Create(tenantId1, "owner@tenant-1.com");
        accountManagementDbContext.Set<Tenant>().AddRange(Tenant1);
        User1 = User.Create(Tenant1.Id, "owner@tenant-1.com", UserRole.Owner, true, null);
        accountManagementDbContext.Set<User>().AddRange(User1);

        accountManagementDbContext.SaveChanges();
    }
}
