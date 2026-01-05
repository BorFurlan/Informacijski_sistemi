using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FinFriend.Models;
using FinFriend.Data;

namespace FinFriend.Helpers
{
    public static class UserHelper
    {
        public static async Task<User?> GetCurrentUserAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            var userId = httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            return await context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }
    }
}