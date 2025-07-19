using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MarinaRegSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // علاقات اختيارية في حال أردت تتبع الكميات التي أضافها المستخدم
        public virtual ICollection<StockEntry> StockEntries { get; set; }

        public string Uid { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
