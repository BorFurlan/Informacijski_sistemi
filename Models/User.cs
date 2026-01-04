using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FinFriend.Models;

public class User : IdentityUser
{
    //je ze podedovano iz IdentityUser
    //public int UserId { get; set; }
    // dodatni podatki o uporabniku
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    // seznami racunov in kategorij (tudi investicij)
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();

    // Provide a display name for the inherited UserName property
    [Display(Name = "Username")]
    public new string UserName
    {
        get => base.UserName;
        set => base.UserName = value;
    }

}