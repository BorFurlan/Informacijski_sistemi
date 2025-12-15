using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinFriend.Data;
using FinFriend.Models;
using Microsoft.AspNetCore.Identity;


[ApiController]
[Route("api/v1/Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<User> userManager
    )
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/v1/Dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = _userManager.GetUserId(User);

        IQueryable<Account> accountsQuery = _context.Accounts.Where(a => a.UserId == userId);
    
        var accounts = await accountsQuery
            .Where(a => a.IsIncludedInTotal)
            .ToListAsync();

        var totalBalance = accounts.Sum(a => a.CurrentBalance);

        var summary = new
        {
            TotalBalance = totalBalance,
            AccountCount = accounts.Count,
            Accounts = accounts.Select(a => new
            {
                a.AccountId,
                a.Name,
                a.CurrentBalance
            })
        };

        return Ok(summary);
    }
}
