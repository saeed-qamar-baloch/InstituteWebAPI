using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Services.TermContext;

public interface ITermContext
{
    Task<Term> GetActiveTermAsync(CancellationToken cancellationToken = default);
}
