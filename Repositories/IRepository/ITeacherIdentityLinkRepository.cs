namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITeacherIdentityLinkRepository
    {
        Task<Guid?> GetTeacherIdForUserIdAsync(string userId);
        Task LinkTeacherToUserIdAsync(Guid teacherId, string userId);
    }
}
