using FinFriend.Models; 
using Microsoft.AspNetCore.Mvc.Rendering;
namespace FinFriend.ViewModels;

public class TransactionFilterViewModel
{
    public int? AccountId { get; set; }
    public TransactionType? Type { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Seznami za dropdown-e
    public List<SelectListItem> Accounts { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Types { get; set; } = new List<SelectListItem>();
}