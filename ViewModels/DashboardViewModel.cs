using FinFriend.Models;
namespace FinFriend.ViewModels;
public class DashboardViewModel
{
    public decimal TotalBalance { get; set; }

    public List<AccountInfo> Accounts { get; set; } = new();

    public class AccountInfo
    {
        public int AccountId { get; set; }
        public AccountType Type { get; set; }
        public string Name { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsIncludedInTotal { get; set; }
    }
}