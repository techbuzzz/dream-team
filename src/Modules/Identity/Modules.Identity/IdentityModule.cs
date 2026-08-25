using Asp.Versioning;
using DreamTeam.Framework.Core.Context;
using DreamTeam.Framework.Eventing;
using DreamTeam.Framework.Persistence;
using DreamTeam.Framework.Quota;
using DreamTeam.Framework.Storage;
using DreamTeam.Framework.Storage.Local;
using DreamTeam.Framework.Storage.Services;
using DreamTeam.Framework.Web.Modules;
using DreamTeam.Modules.Identity.Authorization;
using DreamTeam.Modules.Identity.Authorization.Jwt;
using DreamTeam.Modules.Identity.Contracts.Services;
using DreamTeam.Modules.Identity.Data;
using DreamTeam.Modules.Identity.Domain;
using DreamTeam.Modules.Identity.Features.v1.Groups.AddUsersToGroup;
using DreamTeam.Modules.Identity.Features.v1.Groups.CreateGroup;
using DreamTeam.Modules.Identity.Features.v1.Groups.DeleteGroup;
using DreamTeam.Modules.Identity.Features.v1.Groups.GetGroupById;
using DreamTeam.Modules.Identity.Features.v1.Groups.GetGroupMembers;
using DreamTeam.Modules.Identity.Features.v1.Groups.GetGroups;
using DreamTeam.Modules.Identity.Features.v1.Groups.RemoveUserFromGroup;
using DreamTeam.Modules.Identity.Features.v1.Groups.UpdateGroup;
using DreamTeam.Modules.Identity.Features.v1.Impersonation.EndImpersonation;
using DreamTeam.Modules.Identity.Features.v1.Impersonation.GetImpersonationGrants;
using DreamTeam.Modules.Identity.Features.v1.Impersonation.RevokeImpersonationGrant;
using DreamTeam.Modules.Identity.Features.v1.Impersonation.StartImpersonation;
using DreamTeam.Modules.Identity.Features.v1.Permissions.GetPermissionCatalog;
using DreamTeam.Modules.Identity.Features.v1.Roles;
using DreamTeam.Modules.Identity.Features.v1.Roles.DeleteRole;
using DreamTeam.Modules.Identity.Features.v1.Roles.GetRoleById;
using DreamTeam.Modules.Identity.Features.v1.Roles.GetRoles;
using DreamTeam.Modules.Identity.Features.v1.Roles.GetRoleWithPermissions;
using DreamTeam.Modules.Identity.Features.v1.Roles.UpdateRolePermissions;
using DreamTeam.Modules.Identity.Features.v1.Roles.UpsertRole;
using DreamTeam.Modules.Identity.Features.v1.Sessions.AdminRevokeAllSessions;
using DreamTeam.Modules.Identity.Features.v1.Sessions.AdminRevokeSession;
using DreamTeam.Modules.Identity.Features.v1.Sessions.GetMySessions;
using DreamTeam.Modules.Identity.Features.v1.Sessions.GetTenantSessions;
using DreamTeam.Modules.Identity.Features.v1.Sessions.GetUserSessions;
using DreamTeam.Modules.Identity.Features.v1.Sessions.RevokeAllSessions;
using DreamTeam.Modules.Identity.Features.v1.Sessions.RevokeSession;
using DreamTeam.Modules.Identity.Features.v1.Tokens.RefreshToken;
using DreamTeam.Modules.Identity.Features.v1.Tokens.TokenGeneration;
using DreamTeam.Modules.Identity.Features.v1.TwoFactor.Disable;
using DreamTeam.Modules.Identity.Features.v1.TwoFactor.Enroll;
using DreamTeam.Modules.Identity.Features.v1.TwoFactor.VerifyEnroll;
using DreamTeam.Modules.Identity.Features.v1.Users.AssignUserRoles;
using DreamTeam.Modules.Identity.Features.v1.Users.ChangePassword;
using DreamTeam.Modules.Identity.Features.v1.Users.AdminConfirmEmail;
using DreamTeam.Modules.Identity.Features.v1.Users.ConfirmEmail;
using DreamTeam.Modules.Identity.Features.v1.Users.ResendConfirmationEmail;
using DreamTeam.Modules.Identity.Features.v1.Users.DeleteUser;
using DreamTeam.Modules.Identity.Features.v1.Users.ForgotPassword;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUserById;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUserGroups;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUserPermissions;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUserProfile;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUserRoles;
using DreamTeam.Modules.Identity.Features.v1.Users.GetUsers;
using DreamTeam.Modules.Identity.Features.v1.Users.RegisterUser;
using DreamTeam.Modules.Identity.Features.v1.Users.ResetPassword;
using DreamTeam.Modules.Identity.Features.v1.Users.SearchUsers;
using DreamTeam.Modules.Identity.Features.v1.Users.SelfRegistration;
using DreamTeam.Modules.Identity.Features.v1.Users.SetProfileImage;
using DreamTeam.Modules.Identity.Features.v1.Users.ToggleUserStatus;
using DreamTeam.Modules.Identity.Features.v1.Users.UpdateUser;
using DreamTeam.Modules.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace DreamTeam.Modules.Identity;

public class IdentityModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        DreamTeam.Framework.Shared.Constants.PermissionConstants.Register(
            DreamTeam.Modules.Identity.Contracts.Authorization.IdentityPermissions.All);

        var services = builder.Services;
        services.AddScoped<RolePermissionSyncer>();
        services.AddHostedService<RolePermissionSyncHostedService>();

        // E1.2 slice 2 — seed the 4 FDS role names (TeamLead/PM/DeliveryManager/Member)
        // on every host startup. Idempotent: no-op if already present.
        services.AddHostedService<FdsRoleSeedService>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PathAwareAuthorizationHandler>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<ICurrentUserInitializer>(sp => sp.GetRequiredService<ICurrentUserService>());
        services.AddScoped<IRequestContextService, RequestContextService>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<IRequestContextService>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IImpersonationGrantService, ImpersonationGrantService>();

        // User services - focused single-responsibility services
        services.AddTransient<IUserRegistrationService, UserRegistrationService>();
        services.AddTransient<IUserProfileService, UserProfileService>();
        services.AddTransient<IUserStatusService, UserStatusService>();
        services.AddTransient<IUserRoleService, UserRoleService>();
        services.AddTransient<IUserPasswordService, UserPasswordService>();
        services.AddTransient<IUserPermissionService, UserPermissionService>();

        // Facade for backward compatibility
        services.AddTransient<IUserService, UserService>();

        services.AddTransient<IRoleService, RoleService>();
        services.AddHeroStorage(builder.Configuration);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddHeroDbContext<IdentityDbContext>();
        // Eventing itself is bootstrapped by the host (AddEventingCore) — the outbox is framework
        // infrastructure, not Identity's. Handler registration stays per module.
        services.AddIntegrationEventHandlers(typeof(IdentityModule).Assembly);
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<IdentityDbContext>(
                name: "db:identity",
                failureStatus: HealthStatus.Unhealthy);
        services.AddScoped<IDbInitializer, IdentityDbInitializer>();

        // Configure password policy options
        services.Configure<PasswordPolicyOptions>(builder.Configuration.GetSection("PasswordPolicy"));

        // Tenant subscription grace period (shared "Billing" section) — used by the login expiry check.
        services.Configure<TenantGraceOptions>(builder.Configuration.GetSection(TenantGraceOptions.SectionName));

        // Register password history service
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();

        // Register password expiry service
        services.AddScoped<IPasswordExpiryService, PasswordExpiryService>();

        // Register session service and background cleanup
        services.AddScoped<ISessionService, SessionService>();
        services.AddHostedService<SessionCleanupHostedService>();

        // Register group role service for group-derived permissions
        services.AddScoped<IGroupRoleService, GroupRoleService>();

        // Quota gauge: reports live user count per tenant for the Users quota.
        services.AddScoped<IQuotaGaugeProvider, UserCountQuotaGaugeProvider>();

        services.AddIdentity<DreamTeamUser, DreamTeamRole>(options =>
        {
            options.Password.RequiredLength = IdentityModuleConstants.PasswordLength;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;

            // Account lockout: 5 consecutive failed logins → 15-minute lockout (applies to new users by default).
            // IdentityService's login flow drives AccessFailedAsync / IsLockedOutAsync.
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
           .AddEntityFrameworkStores<IdentityDbContext>()
           .AddDefaultTokenProviders();

        //metrics
        services.AddSingleton<IdentityMetrics>();

        services.ConfigureJwtAuth();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/identity")
            .WithTags("Identity")
            .WithApiVersionSet(apiVersionSet);

        // tokens
        group.MapGenerateTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");
        group.MapRefreshTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");

        // The outbox is dispatched by the framework's OutboxDispatcherHostedService (on by default), which now claims
        // rows with FOR UPDATE SKIP LOCKED so several instances can drain safely. This module still registers no
        // dispatcher of its own: the outbox is framework infrastructure, not Identity's.

        // roles
        group.MapGetRolesEndpoint();
        group.MapGetRoleByIdEndpoint();
        group.MapDeleteRoleEndpoint();
        group.MapGetRolePermissionsEndpoint();
        group.MapUpdateRolePermissionsEndpoint();
        group.MapCreateOrUpdateRoleEndpoint();

        // permission catalog — every permission registered with the host,
        // filtered to the caller's tenant context (root vs admin set)
        group.MapGetPermissionCatalogEndpoint();

        // users
        group.MapAssignUserRolesEndpoint();
        group.MapChangePasswordEndpoint();
        group.MapAdminConfirmEmailEndpoint();
        group.MapResendConfirmationEmailEndpoint().RequireRateLimiting("auth");
        group.MapConfirmEmailEndpoint().RequireRateLimiting("auth");
        group.MapDeleteUserEndpoint();
        group.MapGetUserByIdEndpoint();
        group.MapGetCurrentUserPermissionsEndpoint();
        group.MapGetMeEndpoint();
        group.MapGetUserRolesEndpoint();
        group.MapGetUsersListEndpoint();
        group.MapSearchUsersEndpoint();
        group.MapRegisterUserEndpoint();
        group.MapForgotPasswordEndpoint().RequireRateLimiting("auth");
        group.MapResetPasswordEndpoint().RequireRateLimiting("auth");
        group.MapSelfRegisterUserEndpoint().RequireRateLimiting("auth");
        group.MapToggleUserStatusEndpoint();
        group.MapUpdateUserEndpoint();
        group.MapSetProfileImageEndpoint();

        // sessions - user endpoints
        group.MapGetMySessionsEndpoint();
        group.MapRevokeSessionEndpoint();
        group.MapRevokeAllSessionsEndpoint();

        // sessions - admin endpoints
        group.MapGetTenantSessionsEndpoint();
        group.MapGetUserSessionsEndpoint();
        group.MapAdminRevokeSessionEndpoint();
        group.MapAdminRevokeAllSessionsEndpoint();

        // groups
        group.MapGetGroupsEndpoint();
        group.MapGetGroupByIdEndpoint();
        group.MapCreateGroupEndpoint();
        group.MapUpdateGroupEndpoint();
        group.MapDeleteGroupEndpoint();
        group.MapGetGroupMembersEndpoint();
        group.MapAddUsersToGroupEndpoint();
        group.MapRemoveUserFromGroupEndpoint();

        // user groups
        group.MapGetUserGroupsEndpoint();

        // impersonation
        group.MapStartImpersonationEndpoint();
        group.MapEndImpersonationEndpoint();
        group.MapGetImpersonationGrantsEndpoint();
        group.MapRevokeImpersonationGrantEndpoint();

        // two-factor authentication (TOTP)
        group.MapEnrollTwoFactorEndpoint();
        group.MapVerifyEnrollTwoFactorEndpoint();
        group.MapDisableTwoFactorEndpoint();
    }
}