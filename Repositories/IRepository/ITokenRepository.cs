using Microsoft.AspNetCore.Identity;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITokenRepository
    {
       string CreateJWTToken(IdentityUser user, List<string> roles);

    }
}
