using Microsoft.AspNetCore.Identity;

namespace arpg.Models
{
    public class ApplicationUser: IdentityUser
    {
        public IEnumerable<Game>? Rankings { get; set; }
    }
}
