namespace InstituteWebAPI.Services.StudentMonthlyResults
{
    public interface IStudentMonthlyResultService
    {
        Task RecalculateAsync(Guid termId, Guid currentClassId, Guid termMonthId, IEnumerable<Guid> studentIds);
    }
}
