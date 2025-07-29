using FluentValidation;
using JetBrains.Annotations;
using PlatformPlatform.AccountManagement.Features.Tenants.Domain;
using PlatformPlatform.AccountManagement.Features.Users.Domain;
using PlatformPlatform.SharedKernel.Cqrs;
using PlatformPlatform.SharedKernel.ExecutionContext;
using PlatformPlatform.SharedKernel.Telemetry;

namespace PlatformPlatform.AccountManagement.Features.Tenants.Commands;

[PublicAPI]
public sealed record UpdateCurrentTenantCommand : ICommand, IRequest<Result>
{
    public required string Name { get; init; }
}

public sealed class UpdateCurrentTenantValidator : AbstractValidator<UpdateCurrentTenantCommand>
{
    public UpdateCurrentTenantValidator()
    {
        RuleFor(x => x.Name).Length(1, 30).WithMessage("Name must be between 1 and 30 characters.");
    }
}

public sealed class UpdateTenantHandler(
    ITenantRepository tenantRepository,
    IExecutionContext executionContext,
    ITelemetryEventsCollector events
) : IRequestHandler<UpdateCurrentTenantCommand, Result>
{
    public async Task<Result> Handle(UpdateCurrentTenantCommand command, CancellationToken cancellationToken)
    {
        if (executionContext.UserInfo.Role != UserRole.Owner.ToString())
        {
            return Result.Forbidden("Only owners are allowed to update tenant information.");
        }

        var tenant = await tenantRepository.GetCurrentTenantAsync(cancellationToken);

        tenant.Update(command.Name);
        tenantRepository.Update(tenant);

        events.CollectEvent(new TenantUpdated());

        return Result.Success();
    }
}
