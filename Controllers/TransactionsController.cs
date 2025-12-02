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

namespace FinFriend.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category).Include(t => t.DestinationAccount).Include(t => t.SourceAccount);
            return View(await applicationDbContext.ToListAsync());
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

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
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

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
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
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
