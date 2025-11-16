using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IssueManagementSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        // Apply migrations first
        context.Database.Migrate();

        // Check if SuperAdmin exists — only seed if missing
        if (!context.Users.Any(u => u.Role == UserRole.SUPERADMIN))
        {
            var superAdmin = new User
            {
                UserName = "superadmin",
                Email = "superadmin@example.com",
                PasswordHash = HashPassword("SuperAdmin123!"),
                Role = UserRole.SUPERADMIN
            };

            context.Users.Add(superAdmin);
            context.SaveChanges();

            Console.WriteLine("SuperAdmin created successfully.");
        }
        else
        {
            Console.WriteLine("SuperAdmin already exists. Skipping seeding.");
        }
    }

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }
}
