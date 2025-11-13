using System.Transactions;

namespace FinFriend.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public TransactionType Type { get; set; }

    //podatki o uporabniku
    public int? UserId { get; set; } //ce je null, potem je kategorija globalna
    public User? User { get; set; }

    //razmisli če želiš dodati barvo - loh kot string ali kot tip Color
}