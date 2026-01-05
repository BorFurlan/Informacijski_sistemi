using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinFriend.Models;
using System.ComponentModel;
namespace FinFriend.Data;
using FinFriend.Models;
using static FinFriend.Models.TransactionType;
public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //nastejemo tabele
    //user ze podedujemo iz IdentityDbContext
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>().ToTable("Accounts");
        modelBuilder.Entity<Category>().ToTable("Categories");
        modelBuilder.Entity<Transaction>().ToTable("Transactions");
        
        // odnosi med racuni in transakcijami, da se baza ne zmede
        modelBuilder.Entity<Account>()
            .HasMany(a => a.SourceTransactions)
            .WithOne(t => t.SourceAccount)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.DestinationTransactions)
            .WithOne(t => t.DestinationAccount)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>().HasData(
            //defaultne kategorije stroskov
            new Category { CategoryId = 1, Name = "Rent", Type = Expense, UserId = null },
            new Category { CategoryId = 2, Name = "Bills", Type = Expense, UserId = null },
            new Category { CategoryId = 3, Name = "Taxes", Type = Expense, UserId = null },
            new Category { CategoryId = 4, Name = "Essentials", Type = Expense, UserId = null },
            new Category { CategoryId = 5, Name = "Health", Type = Expense, UserId = null },
            new Category { CategoryId = 6, Name = "Transport", Type = Expense, UserId = null },
            new Category { CategoryId = 7, Name = "Eating Out", Type = Expense, UserId = null },
            new Category { CategoryId = 8, Name = "Entertainment", Type = Expense, UserId = null },
            new Category { CategoryId = 9, Name = "Other Expense", Type = Expense, UserId = null },

            //defaultne kategorije prihodkov
            new Category { CategoryId = 10, Name = "Salary", Type = Income, UserId = null },
            new Category { CategoryId = 11, Name = "Other Income", Type = Income, UserId = null },



            //Defaultne kategorije transferjev
            new Category { CategoryId = 12, Name = "Withdrawal", Type = Transfer, UserId = null },
            new Category { CategoryId = 13, Name = "Deposit", Type = Transfer, UserId = null },
            new Category { CategoryId = 14, Name = "Bank Transfer", Type = Transfer, UserId = null }
        );
       
    }
}
