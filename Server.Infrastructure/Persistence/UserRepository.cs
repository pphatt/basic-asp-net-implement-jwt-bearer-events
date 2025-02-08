using Server.Application.Common.Interfaces.Persistence;
using Server.Domain.Entity.Identity;

namespace Server.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private static readonly List<AppUser> _users = new() 
    {
        new AppUser()
        {
            Email = "test@gmail.com",
            Password = "admin@123",
            FirstName = "Admin",
            LastName = "Test",
        }
    };

    public AppUser? GetUserByEmail(string Email)
    {
        return _users.SingleOrDefault(u => u.Email == Email);
    }
}
