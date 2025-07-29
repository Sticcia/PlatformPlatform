using PlatformPlatform.AccountManagement.Features.Tenants.Domain;
using PlatformPlatform.AccountManagement.Features.Users.Domain;
using PlatformPlatform.SharedKernel.Authentication;

namespace PlatformPlatform.AccountManagement.Features.Users.Shared;

/// <summary>
///     Factory for creating UserInfo instances with tenant information.
///     Centralizes the logic for creating UserInfo to follow SRP and avoid duplication.
/// </summary>
public sealed class UserInfoFactory(ITenantRepository tenantRepository)
{
    /// <summary>
    ///     Creates a UserInfo instance from a User entity, including tenant name.
    /// </summary>
    /// <param name="user">The user entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>UserInfo with all required properties including tenant name</returns>
    public async Task<UserInfo> CreateUserInfoAsync(User user, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);

        return new UserInfo
        {
            IsAuthenticated = true,
            Id = user.Id,
            TenantId = user.TenantId,
            Role = user.Role.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            AvatarUrl = user.Avatar.Url,
            TenantName = tenant?.Name,
            Locale = user.Locale,
            IsInternalUser = user.IsInternalUser
        };
    }
}
