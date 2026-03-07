namespace ProjectManagementSaaS.Application.Tasks;

public sealed record TaskCommentResponse(Guid CommentId, Guid AuthorUserId, string Content, DateTime CreatedUtc);
