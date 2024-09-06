using JetBrains.Annotations;
using PlatformPlatform.AccountManagement.Tenants.Domain;
using PlatformPlatform.SharedKernel.Cqrs;

namespace PlatformPlatform.AccountManagement.Signups.Queries;

[PublicAPI]
public sealed record IsSubdomainFreeQuery(string Subdomain) : IRequest<Result<bool>>;

public sealed class IsSubdomainFreeHandler(ITenantRepository tenantRepository)
    : IRequestHandler<IsSubdomainFreeQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(IsSubdomainFreeQuery request, CancellationToken cancellationToken)
    {
        return await tenantRepository.IsSubdomainFreeAsync(request.Subdomain, cancellationToken);
    }
}
