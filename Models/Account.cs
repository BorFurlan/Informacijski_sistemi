using System.ComponentModel.DataAnnotations;

namespace FinFriend.Models;

public enum AccountType
{
    Cash,
    Bank,
    CreditCard
}


public class Account
{
    public int AccountId { get; set; }
    public AccountType Type { get; set; }
    public string Name { get; set; }
    
    [Display(Name = "Initial Balance")]
    public decimal InitialBalance { get; set; }

    //razmisli če želiš dodati valuto
    //public string Currency { get; set; }

    [Display(Name = "Current Balance")]
    public decimal CurrentBalance { get; set; }

    //podatki o uporabniku
    public string? UserId { get; set; } 
    public User? User { get; set; }


    //morebiti za prikaz imas se boolean vkljucenost v skupnem sestevku
    [Display(Name = "Is Included in Total")]
    public bool IsIncludedInTotal { get; set; }
    //seznami transakcij povezanih z racunom (kot vir in kot cilj)
    public ICollection<Transaction> SourceTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> DestinationTransactions { get; set; } = new List<Transaction>();

//izracunaj trenutno stanje racuna -> tipi morajo biti upostevani v primeru, da razlikujemo, če ne lahko samo sestejemo
    public void CalculateCurrentBalance()
    {
        CurrentBalance = InitialBalance;

        //odstejes vse transakcije iz source transactions (preveri se po tipu)
        foreach (var t in SourceTransactions)
        {
            CurrentBalance -= t.Amount;   
        }

        // sestejes vse transakcije iz destination transactions (preveri se po tipu)
        foreach (var t in DestinationTransactions)
        {
           CurrentBalance += t.Amount;
            
        }
    }
}

// ce bo tezava in ne bo nasel oz. pravilno racunal trenutnega stanja, uporabimo tole funkcijo z kontekstom in potrebno je spremenit kontekst
// public decimal CalculateCurrentBalance(ApplicationDbContext context)
//     {
//         decimal balance = InitialBalance;

//         var relatedTransactions = context.Transactions
//             .Where(t => t.SourceAccountId == AccountId || t.DestinationAccountId == AccountId)
//             .ToList();

//         foreach (var t in relatedTransactions)
//         {
//             if (t.Type == TransactionType.Income && t.DestinationAccountId == AccountId)
//                 balance += t.Amount;

//             else if (t.Type == TransactionType.Expense && t.SourceAccountId == AccountId)
//                 balance -= t.Amount;

//             else if (t.Type == TransactionType.Transfer)
//             {
//                 if (t.SourceAccountId == AccountId)
//                     balance -= t.Amount;
//                 else if (t.DestinationAccountId == AccountId)
//                     balance += t.Amount;
//             }
//         }

//         CurrentBalance = balance;
//         return balance;
//     }