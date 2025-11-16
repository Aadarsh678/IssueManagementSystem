using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Enums;
using IssueManagementSystem.Domain.Interface;
using System.Security.Cryptography;
using System.Text;

namespace IssueManagementSystem.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUnitofWork _unitOfWork;

        public UserService(IUserRepository userRepo, IUnitofWork unitOfWork)
        {
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            var existing = await _userRepo.GetByEmailAsync(email);
            if (existing != null) throw new Exception("Email already exists");

            var user = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = UserRole.USER
            };

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return user;
        }

        public async Task<User> PromoteToAdminAsync(int userId, int superAdminId)
        {
            // Check if the caller is SuperAdmin
            var superAdmin = await _userRepo.GetByIdAsync(superAdminId);
            if (superAdmin == null || superAdmin.Role != UserRole.SUPERADMIN)
                throw new Exception("Only SuperAdmin can promote users");

            // Get the user to be promoted
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            // Promote
            user.Role = UserRole.ADMIN;
            await _unitOfWork.SaveAsync();

            return user;
        }


        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return null;
            return VerifyPassword(password, user.PasswordHash) ? user : null;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;
    }
}
