using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IMarkEditRequestRepository
    {
        Task<List<MarkEditRequest>> GetAllAsync(Guid? teacherId = null, int? status = null);
        Task<MarkEditRequest?> GetByIdAsync(Guid id);

        /// <summary>Teacher submits a new request. TeacherID is resolved from JWT inside the repo.</summary>
        Task<MarkEditRequest> AddAsync(MarkEditRequest request);

        /// <summary>
        /// Admin approves or rejects the request.
        /// If approved, the underlying StudentMark.ObtainedMarks is updated and monthly result is recalculated.
        /// </summary>
        Task<MarkEditRequest?> ReviewAsync(Guid requestId, MarkEditRequestStatus status, string reviewedByUserId, string? reviewRemarks);
    }
}
