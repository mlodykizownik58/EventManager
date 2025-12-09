using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EventManagement.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }

        // OrganizerId jest przypisywane w kontrolerze, nie musisz tego oznaczać jako Required
        public string OrganizerId { get; set; }

        [ValidateNever]
        public virtual IdentityUser Organizer { get; set; }

        public virtual ICollection<EventSignup> EventSignups { get; set; } = new List<EventSignup>();

        // Dodana właściwość ImageUrl do przechowywania ścieżki obrazu
        public string? ImageUrl { get; set; }
    }
}
