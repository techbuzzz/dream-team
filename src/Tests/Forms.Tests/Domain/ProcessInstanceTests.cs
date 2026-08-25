using DreamTeam.Modules.Forms.Domain;
using Shouldly;
using Xunit;

namespace Forms.Tests.Domain;

/// <summary>
/// Domain tests for the ProcessInstance entity. Pins the state-machine
/// invariants of the Schedule factory and confirms that a freshly-scheduled
/// instance starts in <c>Planned</c> with <c>CompletedAt = null</c>.
/// </summary>
public sealed class ProcessInstanceTests
{
    [Fact]
    public void Schedule_Should_StartIn_PlannedState_WithNullCompletedAt()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var tenantId = "tenant-1";
        var scheduledAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var pairUserId = "user-42";

        // Act
        var instance = ProcessInstance.Schedule(
            formVersionId: formVersionId,
            tenantId: tenantId,
            scheduledAt: scheduledAt,
            pairUserId: pairUserId);

        // Assert
        instance.Id.ShouldNotBe(Guid.Empty);
        instance.FormVersionId.ShouldBe(formVersionId);
        instance.TenantId.ShouldBe(tenantId);
        instance.ScheduledAt.ShouldBe(scheduledAt);
        instance.PairUserId.ShouldBe(pairUserId);
        instance.Status.ShouldBe(ProcessStatus.Planned);
        instance.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Schedule_Should_AcceptNullPairUser_ForWholeTeamRituals()
    {
        // Arrange — Daily Sync, Sprint Retro, etc. have no PairUserId.
        var instance = ProcessInstance.Schedule(
            formVersionId: Guid.NewGuid(),
            tenantId: "tenant-1",
            scheduledAt: DateTime.UtcNow.AddDays(1),
            pairUserId: null);

        // Assert
        instance.PairUserId.ShouldBeNull();
        instance.Status.ShouldBe(ProcessStatus.Planned);
    }

    [Fact]
    public void Schedule_Should_Throw_When_TenantIdIsEmpty()
    {
        // Act + Assert
        Should.Throw<ArgumentException>(() =>
            ProcessInstance.Schedule(
                formVersionId: Guid.NewGuid(),
                tenantId: "",
                scheduledAt: DateTime.UtcNow.AddDays(1),
                pairUserId: null));
    }
}
