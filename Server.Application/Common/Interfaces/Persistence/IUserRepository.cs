using Server.Domain.Entity.Identity;

namespace Server.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    AppUser? GetUserByEmail(string Email);
}
