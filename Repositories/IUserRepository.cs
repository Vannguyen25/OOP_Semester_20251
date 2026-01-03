using OOP_Semester.Models;

namespace OOP_Semester.Repositories
{
    public interface IUserRepository
    {
        User? Login(string username, string password);
        bool Register(User user);
        bool ChangePassword(int userId, string oldPassword, string newPassword);
    }
}