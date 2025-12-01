using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinFriend.Data;
using FinFriend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace FinFriend.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Accounts.Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            ViewData["UserName"] = new SelectList(_context.Users, "UserName", "UserName", account.User.UserName);
            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            ViewData["AccountType"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,Type,Name,InitialBalance,UserId,IsIncludedInTotal")] Account account)
        {
            if (!User?.Identity?.IsAuthenticated ?? true) 
                return Challenge();

            // set owner from ClaimsPrincipal (server-side)
            account.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (account.User == null)
            {                
                account.User = await GetCurrentUserAsync();
            }

            if (ModelState.IsValid)
            {
                account.CurrentBalance = account.InitialBalance;
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", account.UserId);
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["AccountType"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,Type,Name,InitialBalance,CurrentBalance,UserId,IsIncludedInTotal")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    account.User = await GetCurrentUserAsync();
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AccountType"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

           if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }    
    }
}