using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinFriend.Data;
using FinFriend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FinFriend.Filters;

[ApiController]
[Route("api/v1/Dashboard")]
[ApiKeyAuth]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    
    }

    // GET: api/v1/Dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
    

        IQueryable<Account> accountsQuery = _context.Accounts;
    
       //vse racune povlece
        var accounts = await accountsQuery.ToListAsync();

        //sesteje samo tistih, ki so vključeni v total
        var totalBalance = accounts.Where(a => a.IsIncludedInTotal).Sum(a => a.CurrentBalance);

        //transakcije
        var transactions = await _context.Transactions.ToListAsync();

        var summary = new
        {
            TotalBalance = totalBalance,
            AccountCount = accounts.Count,
            IncludedAccountCount = accounts.Count(a => a.IsIncludedInTotal),
            TransactionCount = transactions.Count
        };

        return Ok(summary);
    }
}
