using backend_3_module.Data.DTO.Comment;

namespace backend_3_module.Services.IServices;

public interface ICommentService
{
    public Task<List<CommentInfoDTO>> GetAllNestedComments(Guid commentId, Guid userId);
    public Task AddComment(Guid postId, Guid userId, CommentDTO comment);
    public Task EditComment(Guid commentId, Guid userId, CommentContentDTO comment);
    public Task DeleteComment(Guid commentId, Guid userId);
}