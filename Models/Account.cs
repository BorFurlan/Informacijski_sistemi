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
    public decimal InitialBalance { get; set; }

    //razmisli če želiš dodati valuto
    //public string Currency { get; set; }

    public decimal CurrentBalance { get; set; }

    //podatki o uporabniku
    public int? UserId { get; set; } //ce je null, potem je racun globalen
    public User? User { get; set; }


    //morebiti za prikaz imas se boolean vkljucenost v skupnem sestevku
    public bool IsIncludedInTotal { get; set; }
    //seznam transakcij povezanih z racunom
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

//izracunaj trenutno stanje racuna
    public void CalculateCurrentBalance()
    {
        CurrentBalance = InitialBalance;
        foreach (var transaction in Transactions)
        {
            switch (transaction.Type)
            {
                case TransactionType.Income:
                    CurrentBalance += transaction.Amount;
                    break;

                case TransactionType.Expense:
                    CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    if (transaction.SourceAccountId == AccountId)
                    {
                        //denar odtece iz tega racuna
                        CurrentBalance -= transaction.Amount;
                    }
                    else if (transaction.DestinationAccountId == AccountId)
                    {
                        //denar prispe na ta racun
                        CurrentBalance += transaction.Amount;
                    }
                    break;


            }
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