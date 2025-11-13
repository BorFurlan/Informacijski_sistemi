namespace FinFriend.Models;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public int UserId { get; set; }
    // dodatni podatki o uporabniku
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    // seznami racunov in kategorij (tudi investicij)
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();


}