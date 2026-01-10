namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITeacherIdentityLinkRepository
    {
        Task<Guid?> GetTeacherIdForUserIdAsync(string userId);
    }
}
