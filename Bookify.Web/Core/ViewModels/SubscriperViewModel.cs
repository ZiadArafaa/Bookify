﻿namespace Bookify.Web.Core.ViewModels
{
    public class SubscriperViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime BirthOfDate { get; set; }
        public string Address { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public bool HasWhatsApp { get; set; }
        public string Governorate { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string ImageThumbnailUrl { get; set; } = null!;
        public bool IsBlackListed { get; set; }
    }
}
