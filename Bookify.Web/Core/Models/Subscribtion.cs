﻿namespace Bookify.Web.Core.Models
{
    public class Subscribtion
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber? Subscriber { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 
        public string CreatedById { get; set; } = null!;
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } 
    }
}
