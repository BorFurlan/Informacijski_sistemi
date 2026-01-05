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
using Microsoft.AspNetCore.Identity;
using FinFriend.ViewModels;

namespace FinFriend.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<bool> UserCanAccessTransaction(Transaction transaction)
        {
            if (User.IsInRole("Admin"))
                return true;

            var userId = _userManager.GetUserId(User);

            return
                (transaction.SourceAccount?.UserId == userId) ||
                (transaction.DestinationAccount?.UserId == userId);
        }
        // GET: Transactions
        public async Task<IActionResult> Index(TransactionFilterViewModel filter)
        {
            var userId = _userManager.GetUserId(User);
            //osnovna poizvedba
            IQueryable<Transaction> query = _context.Transactions
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .Include(t => t.Category);

            // navadni uporabnik: vidi samo svoje
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t =>
                    (t.SourceAccount != null && t.SourceAccount.UserId == userId) ||
                    (t.DestinationAccount != null && t.DestinationAccount.UserId == userId)
                );
            }

            //filtri
            //filter racuna
            if (filter.AccountId.HasValue)
            {
                query = query.Where(t =>
                    t.SourceAccountId == filter.AccountId.Value ||
                    t.DestinationAccountId == filter.AccountId.Value
                );
            }
            //filter tipa transakcije
            if (filter.Type.HasValue)
            {
                query = query.Where(t => t.Type == filter.Type.Value);
            }
            //filter kategorije
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
            }

            //fitler zneska
            if (filter.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
            }

            //filter datuma
            if (filter.StartDate.HasValue)
            {
                query = query.Where(t => t.Date >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(t => t.Date <= filter.EndDate.Value);
            }

            // priprava seznama za dropdowne
            filter.Accounts = await _context.Accounts
                .Where(a => User.IsInRole("Admin") || a.UserId == userId)
                .Select(a => new SelectListItem { Value = a.AccountId.ToString(), Text = a.Name })
                .ToListAsync();

            filter.Categories = await _context.Categories
                .Where(c => User.IsInRole("Admin") || c.UserId == userId || c.UserId == null)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            filter.Types = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() })
                .ToList();
                
            var transactions = await query.ToListAsync();
            ViewBag.Transactions = transactions;
            return View(filter);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.DestinationAccount)
                .Include(t => t.SourceAccount)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            if (!await UserCanAccessTransaction(transaction))
                return Forbid();

            return View(transaction);
        }

        // GET: Transactions/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // Admin vidi vse račune
            IQueryable<Account> accountsQuery = _context.Accounts;

            if (!User.IsInRole("Admin"))
            {
                accountsQuery = accountsQuery.Where(a => a.UserId == userId);
            }

            var accounts = await accountsQuery
                .Select(a => new { a.AccountId, Display = a.Name })
                .ToListAsync();


            // Admin vidi vse kategorije; navadni samo svoje + shared (UserId == null)
            IQueryable<Category> categoriesQuery = _context.Categories;

            if (!User.IsInRole("Admin"))
            {
                categoriesQuery = categoriesQuery.Where(c =>
                    c.UserId == userId || c.UserId == null);
            }

            var categories = await categoriesQuery
                .Select(c => new { c.CategoryId, c.Name, c.Type })
                .ToListAsync();

            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");

            ViewData["SourceAccountId"] = new SelectList(accounts, "AccountId", "Display");
            ViewData["DestinationAccountId"] = new SelectList(accounts, "AccountId", "Display");
            // pass categories with type info for client-side filtering
            ViewBag.Categories = categories;
            ViewData["CategoryId"] = new SelectList(categories.Select(c => new { c.CategoryId, Display = c.Name }), "CategoryId", "Display");

            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,Amount,Date,Note,Type,SourceAccountId,DestinationAccountId,CategoryId")] Transaction transaction)
        {
            // Nastavimo kategorijo iz baze glede na izbran CategoryId
            transaction.Category = await _context.Categories.FindAsync(transaction.CategoryId);
            var userId = _userManager.GetUserId(User);

            // Server-side: ensure required accounts are provided based on transaction type
            if (transaction.Type == TransactionType.Income)
            {
                if (!transaction.DestinationAccountId.HasValue)
                {
                    ModelState.AddModelError("DestinationAccountId", "Destination account is required for Income transactions.");
                }
            }
            else if (transaction.Type == TransactionType.Expense)
            {
                if (!transaction.SourceAccountId.HasValue)
                {
                    ModelState.AddModelError("SourceAccountId", "Source account is required for Expense transactions.");
                }
            }
            else if (transaction.Type == TransactionType.Transfer)
            {
                if (!transaction.SourceAccountId.HasValue)
                {
                    ModelState.AddModelError("SourceAccountId", "Source account is required for Transfer transactions.");
                }
                if (!transaction.DestinationAccountId.HasValue)
                {
                    ModelState.AddModelError("DestinationAccountId", "Destination account is required for Transfer transactions.");
                }
            }
            if (ModelState.IsValid)
            {
                // load source/destination accounts (if chosen)
                Account? sourceAccount = null;
                Account? destinationAccount = null;

                if (transaction.SourceAccountId.HasValue)
                {
                    sourceAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountId == transaction.SourceAccountId.Value);
                }

                if (transaction.DestinationAccountId.HasValue)
                {
                    destinationAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountId == transaction.DestinationAccountId.Value);
                }

                if (transaction.Type == TransactionType.Income)
                {
                    // Pri dohodku ni source account
                    transaction.SourceAccountId = null;
                    transaction.SourceAccount = null;

                }
                else if (transaction.Type == TransactionType.Expense)
                {
                    // Pri strošku ni destination account
                    transaction.DestinationAccountId = null;
                    transaction.DestinationAccount = null;

                
                }
                

                // Ensure FKs / navigation properties are set explicitly
                if (sourceAccount != null)
                {
                    transaction.SourceAccountId = sourceAccount.AccountId;
                    transaction.SourceAccount = sourceAccount;
                }

                if (destinationAccount != null)
                {
                    transaction.DestinationAccountId = destinationAccount.AccountId;
                    transaction.DestinationAccount = destinationAccount;
                }

                // If an account id wasn't provided, ensure the navigation property is null
                if (!transaction.SourceAccountId.HasValue)
                {
                    transaction.SourceAccount = null;
                }

                if (!transaction.DestinationAccountId.HasValue)
                {
                    transaction.DestinationAccount = null;
                }

                // add the transaction
                _context.Transactions.Add(transaction);

                // Update account CurrentBalance directly so changes persist correctly
                // and reflect the newly added transaction immediately.
                // For Income: increase destination account (source is null)
                // For Expense: decrease source account (destination is null)
                // For Transfer: decrease source and increase destination
                if (sourceAccount != null)
                {
                    // sourceAccount is tracked; apply delta and mark modified
                    sourceAccount.CurrentBalance -= transaction.Amount;
                    _context.Accounts.Update(sourceAccount);
                }

                if (destinationAccount != null)
                {
                    destinationAccount.CurrentBalance += transaction.Amount;
                    _context.Accounts.Update(destinationAccount);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            
            // Repopulate accounts, filtered for non-admin users (so they don't see others' accounts)
            IQueryable<Account> accountsQuery = _context.Accounts;
            if (!User.IsInRole("Admin"))
            {
                accountsQuery = accountsQuery.Where(a => a.UserId == userId);
            }
            var accountsList = await accountsQuery.Select(a => new { a.AccountId, Display = a.Name }).ToListAsync();
            ViewData["SourceAccountId"] = new SelectList(accountsList, "AccountId", "Display", transaction.SourceAccountId);
            ViewData["DestinationAccountId"] = new SelectList(accountsList, "AccountId", "Display", transaction.DestinationAccountId);

            // ensure categories are available for the view (include Type for client filtering)
            // reuse the earlier `userId` declared above in this method
            IQueryable<Category> categoriesQuery = _context.Categories;
            if (!User.IsInRole("Admin"))
            {
                categoriesQuery = categoriesQuery.Where(c => c.UserId == userId || c.UserId == null);
            }
            var categories = await categoriesQuery.Select(c => new { c.CategoryId, c.Name, c.Type }).ToListAsync();
            ViewBag.Categories = categories;
              ViewData["CategoryId"] = new SelectList(categories.Select(c => new { c.CategoryId, Display = c.Name }), "CategoryId", "Display");

              return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }
            if (!await UserCanAccessTransaction(transaction))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            // Admin vidi vse račune
            IQueryable<Account> accountsQuery = _context.Accounts;

            if (!User.IsInRole("Admin"))
            {
                accountsQuery = accountsQuery.Where(a => a.UserId == userId);
            }

            var accounts = await accountsQuery
                .Select(a => new { a.AccountId, Display = a.Name })
                .ToListAsync();


            // Admin vidi vse kategorije; navadni samo svoje + shared (UserId == null)
            IQueryable<Category> categoriesQuery = _context.Categories;

            if (!User.IsInRole("Admin"))
            {
                categoriesQuery = categoriesQuery.Where(c =>
                    c.UserId == userId || c.UserId == null);
            }

            var categories = await categoriesQuery
                .Select(c => new { c.CategoryId, Display = c.Name })
                .ToListAsync();

            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");

            ViewData["SourceAccountId"] = new SelectList(accounts, "AccountId", "Display", transaction.SourceAccountId);
            ViewData["DestinationAccountId"] = new SelectList(accounts, "AccountId", "Display", transaction.DestinationAccountId);
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Display", transaction.CategoryId);

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,Amount,Date,Note,Type,SourceAccountId,DestinationAccountId,CategoryId")] Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            // Server-side: ensure required accounts are provided based on transaction type
            if (transaction.Type == TransactionType.Income)
            {
                if (!transaction.DestinationAccountId.HasValue)
                {
                    ModelState.AddModelError("DestinationAccountId", "Destination account is required for Income transactions.");
                }
            }
            else if (transaction.Type == TransactionType.Expense)
            {
                if (!transaction.SourceAccountId.HasValue)
                {
                    ModelState.AddModelError("SourceAccountId", "Source account is required for Expense transactions.");
                }
            }
            else if (transaction.Type == TransactionType.Transfer)
            {
                if (!transaction.SourceAccountId.HasValue)
                {
                    ModelState.AddModelError("SourceAccountId", "Source account is required for Transfer transactions.");
                }
                if (!transaction.DestinationAccountId.HasValue)
                {
                    ModelState.AddModelError("DestinationAccountId", "Destination account is required for Transfer transactions.");
                }
            }

            if (ModelState.IsValid)
            {
                // Use a DB transaction to ensure undo+redo is atomic
                using var dbTx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // load original transaction (no tracking so we can compare)
                    var original = await _context.Transactions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

                    if (original != null)
                    {
                        // Undo original effect on accounts
                        if (original.Type == TransactionType.Transfer)
                        {
                            if (original.SourceAccountId.HasValue)
                            {
                                var origSrc = await _context.Accounts.FindAsync(original.SourceAccountId.Value);
                                if (origSrc != null)
                                {
                                    origSrc.CurrentBalance += original.Amount;
                                    _context.Accounts.Update(origSrc);
                                }
                            }
                            if (original.DestinationAccountId.HasValue)
                            {
                                var origDst = await _context.Accounts.FindAsync(original.DestinationAccountId.Value);
                                if (origDst != null)
                                {
                                    origDst.CurrentBalance -= original.Amount;
                                    _context.Accounts.Update(origDst);
                                }
                            }
                        }
                        else if (original.Type == TransactionType.Income)
                        {
                            if (original.DestinationAccountId.HasValue)
                            {
                                var origDst = await _context.Accounts.FindAsync(original.DestinationAccountId.Value);
                                if (origDst != null)
                                {
                                    origDst.CurrentBalance -= original.Amount;
                                    _context.Accounts.Update(origDst);
                                }
                            }
                        }
                        else if (original.Type == TransactionType.Expense)
                        {
                            if (original.SourceAccountId.HasValue)
                            {
                                var origSrc = await _context.Accounts.FindAsync(original.SourceAccountId.Value);
                                if (origSrc != null)
                                {
                                    origSrc.CurrentBalance += original.Amount;
                                    _context.Accounts.Update(origSrc);
                                }
                            }
                        }
                    }

                    // Now apply the new transaction effect (similar to Create)
                    // Ensure we have up-to-date tracked accounts
                    Account? newSource = null;
                    Account? newDestination = null;

                    if (transaction.SourceAccountId.HasValue)
                    {
                        newSource = await _context.Accounts.FindAsync(transaction.SourceAccountId.Value);
                    }
                    if (transaction.DestinationAccountId.HasValue)
                    {
                        newDestination = await _context.Accounts.FindAsync(transaction.DestinationAccountId.Value);
                    }

                    // Adjust for transaction type rules (Income: no source; Expense: no destination)
                    if (transaction.Type == TransactionType.Income)
                    {
                        transaction.SourceAccountId = null;
                        transaction.SourceAccount = null;
                    }
                    else if (transaction.Type == TransactionType.Expense)
                    {
                        transaction.DestinationAccountId = null;
                        transaction.DestinationAccount = null;
                    }

                    if (newSource != null)
                    {
                        newSource.CurrentBalance -= transaction.Amount;
                        _context.Accounts.Update(newSource);
                    }

                    if (newDestination != null)
                    {
                        newDestination.CurrentBalance += transaction.Amount;
                        _context.Accounts.Update(newDestination);
                    }

                    // Update the transaction record itself
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            
            ViewData["SourceAccountId"] = new SelectList(
                _context.Accounts.Select(a => new { a.AccountId, Display = $"{a.Name}" }),
                "AccountId",
                "Display"
            );
            
            ViewData["DestinationAccountId"] = new SelectList(
                 _context.Accounts.Select(a => new { a.AccountId, Display = $"{a.Name}" }),
                "AccountId",
                "Display"
             );

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Select(c => new { c.CategoryId, Display = $"{c.Name}" }),
                "CategoryId",
                "Display"
            );

            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.DestinationAccount)
                .Include(t => t.SourceAccount)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }
            if (!await UserCanAccessTransaction(transaction))
                return Forbid();

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                // Undo transaction effect on accounts before deleting
                if (transaction.Type == TransactionType.Transfer)
                {
                    if (transaction.SourceAccountId.HasValue)
                    {
                        var src = await _context.Accounts.FindAsync(transaction.SourceAccountId.Value);
                        if (src != null)
                        {
                            src.CurrentBalance += transaction.Amount;
                            _context.Accounts.Update(src);
                        }
                    }
                    if (transaction.DestinationAccountId.HasValue)
                    {
                        var dst = await _context.Accounts.FindAsync(transaction.DestinationAccountId.Value);
                        if (dst != null)
                        {
                            dst.CurrentBalance -= transaction.Amount;
                            _context.Accounts.Update(dst);
                        }
                    }
                }
                else if (transaction.Type == TransactionType.Income)
                {
                    if (transaction.DestinationAccountId.HasValue)
                    {
                        var dst = await _context.Accounts.FindAsync(transaction.DestinationAccountId.Value);
                        if (dst != null)
                        {
                            dst.CurrentBalance -= transaction.Amount;
                            _context.Accounts.Update(dst);
                        }
                    }
                }
                else if (transaction.Type == TransactionType.Expense)
                {
                    if (transaction.SourceAccountId.HasValue)
                    {
                        var src = await _context.Accounts.FindAsync(transaction.SourceAccountId.Value);
                        if (src != null)
                        {
                            src.CurrentBalance += transaction.Amount;
                            _context.Accounts.Update(src);
                        }
                    }
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
