using WebAPIWithAuthenticationToken.Models;

namespace WebAPIWithAuthenticationToken.Repositories
{
    public static class UserRepository
    {
        public static User Get(string userName, string password)
        {
            var users = new List<User>
            {
                new User{ Id = 1, UserName = "batman", Password = "batman", Role = "manager" },
                new User{ Id = 1, UserName = "robin", Password = "robin", Role = "employee" }
            };

            return users.Single(u => u.UserName.ToLower().Equals(userName) && u.Password.ToLower().Equals(password));
        }
    }
}
