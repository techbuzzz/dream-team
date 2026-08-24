using DreamTeam.Modules.Forms.Contracts.Dtos;
using Mediator;

namespace DreamTeam.Modules.Forms.Contracts.v1.ProcessInstances.GetProcessInstanceById;

/// <summary>
/// MVP-1 E1.1 — Fetch a single ProcessInstance by its Id. The base DbContext's
/// default-on tenant-isolation query filter narrows the lookup to the
/// caller's tenant; an Id that exists in another tenant surfaces as 404,
/// which is the right behavior (no cross-tenant existence leaks).
///
/// Read-only (AsNoTracking). The instance is a runtime occurrence — its
/// FormVersion pointer never moves (snapshot-on-publish invariant).
/// </summary>
public sealed record GetProcessInstanceByIdQuery(Guid Id) : IQuery<ProcessInstanceDto>;
