using Microsoft.AspNetCore.Identity;

namespace EventManagement.Models
{
    public class EventSignup
    {
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public string UserId { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
