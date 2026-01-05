using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinFriend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FinFriend.Data;
using Microsoft.EntityFrameworkCore;
using FinFriend.ViewModels;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinFriend.Controllers
{
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

            ViewData["Accounts"] = new SelectList(accounts, "AccountId", "Name", accounts.FirstOrDefault()?.AccountId);

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

        [HttpGet]
        public async Task<IActionResult> GetAccountHistory(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return NotFound();

            var transactions = await _context.Transactions
                .Where(t => (t.SourceAccountId.HasValue && t.SourceAccountId == accountId)
                        || (t.DestinationAccountId.HasValue && t.DestinationAccountId == accountId))
                .OrderBy(t => t.Date)
                .ToListAsync();

            decimal balance = account.InitialBalance;
            var labels = new List<string>();
            var balances = new List<decimal>();

            // initial point
            labels.Add("Initial");
            balances.Add(balance);

            foreach (var t in transactions)
            {
                if (t.DestinationAccountId == accountId) balance += t.Amount;
                if (t.SourceAccountId == accountId) balance -= t.Amount;

                labels.Add(t.Date.ToString("dd-mm-yyyy hh:mm"));
                balances.Add(balance);
            }

            return Json(new { labels, balances });
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionsByCategory(int? accountId)
        {
            var q = _context.Transactions.AsQueryable();
            if (accountId.HasValue && accountId.Value != 0)
                q = q.Where(t => t.SourceAccountId == accountId.Value || t.DestinationAccountId == accountId.Value);

            var data = await q
                .GroupBy(t => t.Category)                 // adjust if Category is a navigation prop
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
                        
            return Json(new
            {
                labels = data.Select(d => d.Category.Name),
                counts = data.Select(d => d.Count)
            });
        }
    }
}
