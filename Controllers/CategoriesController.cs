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
using FinFriend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FinFriend.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Category> query = _context.Categories
                .Include(c => c.User);

            // Če ni admin → vidi samo svoje kategorije (UserId == njegov ID)
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(c => c.UserId == userId || c.UserId == null);
            }

            // Admin vidi vse kategorije, tudi tiste s UserId == null
            var categories = await query.ToListAsync();

            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // access check
            if (!User.IsInRole("Admin") &&
               category.UserId != null &&
               category.UserId != userId)
            {
                return Forbid();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Type,UserId")] Category category)
        {
            if (ModelState.IsValid)
            {
                var user = await UserHelper.GetCurrentUserAsync(HttpContext, _context);
                category.User = user;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", category.UserId);
            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // access check
            if (!User.IsInRole("Admin") &&
               category.UserId != null &&
               category.UserId != userId)
            {
                return Forbid();
            }
            //da uporabnik ne more urejati globalne kategorije
            if (!User.IsInRole("Admin") && category.UserId == null)
            {
                TempData["Error"] = "Nimate dovoljenja za urejanje te kategorije.";
                return RedirectToAction("Index");
            }
            ViewData["TransactionTypeId"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>().Select(t => new { Value = t, Text = t.ToString() }), "Value", "Text");
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Type,UserId")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.User = await UserHelper.GetCurrentUserAsync(HttpContext, _context);
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // access check
            if (!User.IsInRole("Admin") &&
               category.UserId != null &&
               category.UserId != userId)
            {
                return Forbid();
            }

            //da uporabnik ne more zbrisati globalne kategorije
            if (!User.IsInRole("Admin") && category.UserId == null)
            {
                TempData["Error"] = "Nimate dovoljenja za brisanje te kategorije.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
