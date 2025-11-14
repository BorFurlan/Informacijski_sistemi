namespace FinFriend.Models;

using System.ComponentModel.DataAnnotations.Schema;

public enum TransactionType
{
    Income,
    Expense,
    Transfer
}

public class Transaction
{
    //podatki o transakciji
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now; //privzeto vzame trenutni datum in cas
    public string? Note { get; set; }
    public TransactionType Type { get; set; }

    //racuni
    public int? SourceAccountId { get; set; }
   
    public Account? SourceAccount { get; set; }

    public int? DestinationAccountId { get; set; }

    public Account? DestinationAccount { get; set; }
    

    //kategorija
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    //ce zelis se sliko racuna kot prilogo
    // public string? ReceiptImagePath { get; set;  } - ce je shranjena na oblaku
    //public byte[]? ReceiptImage { get; set; } - ce je shranjena v bazi (za majhne ne bi smelo biti problema)
}