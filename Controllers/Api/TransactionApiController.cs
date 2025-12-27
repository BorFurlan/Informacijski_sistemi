using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinFriend.Data;
using FinFriend.Models;

namespace FinFriend.Controllers_Api
{
    [Route("api/v1/TransactionsApi")]
    [ApiController]
    public class TransactionApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/TransactionsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        // GET: api/v1/TransactionsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // PUT: api/v1/TransactionsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/v1/TransactionsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            Account? sourceAccount = null;
            Account? destinationAccount = null;

            if (transaction.Type == TransactionType.Expense || transaction.Type == TransactionType.Transfer)
            {
                if (!transaction.SourceAccountId.HasValue)
                    return BadRequest("Source account is required.");

                sourceAccount = await _context.Accounts.FindAsync(transaction.SourceAccountId.Value);
                if (sourceAccount == null)
                    return BadRequest("Invalid source account.");
            }

            if (transaction.Type == TransactionType.Income || transaction.Type == TransactionType.Transfer)
            {
                if (!transaction.DestinationAccountId.HasValue)
                    return BadRequest("Destination account is required.");

                destinationAccount = await _context.Accounts.FindAsync(transaction.DestinationAccountId.Value);
                if (destinationAccount == null)
                    return BadRequest("Invalid destination account.");
            }

            // NORMALIZACIJA (ZELO POMEMBNO)
            if (transaction.Type == TransactionType.Income)
            {
                transaction.SourceAccountId = null;
                transaction.SourceAccount = null;
            }

            if (transaction.Type == TransactionType.Expense)
            {
                transaction.DestinationAccountId = null;
                transaction.DestinationAccount = null;
            }

            transaction.SourceAccount = sourceAccount;
            transaction.DestinationAccount = destinationAccount;

            _context.Transactions.Add(transaction);

            // POSODOBITEV STANJA
            if (sourceAccount != null)
                sourceAccount.CurrentBalance -= transaction.Amount;

            if (destinationAccount != null)
                destinationAccount.CurrentBalance += transaction.Amount;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // DELETE: api/v1/TransactionsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
