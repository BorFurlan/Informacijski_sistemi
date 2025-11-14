using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinFriend.Models;
namespace FinFriend.Data;

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
        
        // Configure relationships for transactions where an account can be source or destination
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
    }
}
