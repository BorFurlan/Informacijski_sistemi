using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinFriend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace FinFriend.Controllers;
using FinFriend.Data;
using Microsoft.EntityFrameworkCore;
using FinFriend.ViewModels;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        IQueryable<Account> accountsQuery = _context.Accounts.Where(a => a.UserId == userId);

        var accounts = await accountsQuery.ToListAsync();
        var vm = new DashboardViewModel
        {
            TotalBalance = accounts.Where(a => a.IsIncludedInTotal).Sum(a => a.CurrentBalance),
            Accounts = accounts.Select(a => new DashboardViewModel.AccountInfo
            {
                AccountId = a.AccountId,
                Name = a.Name,
                Type = a.Type,
                InitialBalance = a.InitialBalance,
                CurrentBalance = a.CurrentBalance,
                IsIncludedInTotal = a.IsIncludedInTotal
            }).ToList()
        };
        // Prikaže samo račune prijavljenega uporabnika, ki so označeni kot "included"
    

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
