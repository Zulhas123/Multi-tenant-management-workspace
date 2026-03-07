namespace ProjectManagementSaaS.Application.Organizations;

public sealed record ActivityLogResponse(
    Guid ActivityLogId,
    string EntityType,
    string EntityId,
    string Action,
    string Description,
    Guid? ActorUserId,
    DateTime CreatedUtc);
