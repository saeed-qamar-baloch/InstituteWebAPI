using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IResultApprovalRepository
    {
        Task<List<ResultApproval>> GetAllAsync(Guid? termId = null, Guid? currentClassId = null);

        Task<ResultApproval?> GetByIdAsync(Guid id);

        /// <summary>Returns the approval record for a specific class in a term, or null if none exists.</summary>
        Task<ResultApproval?> GetByClassAndTermAsync(Guid termId, Guid currentClassId);

        /// <summary>Returns true if the result for this class/term has been approved (locked).</summary>
        Task<bool> IsApprovedAsync(Guid termId, Guid currentClassId);

        /// <summary>Creates or updates the approval record for a class/term.</summary>
        Task<ResultApproval> UpsertAsync(ResultApproval approval, string approvedByUserId);
    }
}
