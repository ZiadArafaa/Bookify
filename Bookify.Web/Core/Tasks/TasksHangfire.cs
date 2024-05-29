using AutoMapper;
using Bookify.Web.Helpers.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Core.Tasks
{
    public class TasksHangfire
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public TasksHangfire(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public async Task NotifayRenewAsync()
        {
            if (!_context.Set<Subscriber>().Any() || !_context.Set<Subscribtion>().Any())
                return;

            var subscribers = await _context.Set<Subscriber>().Include(s => s.Subscribtions)
                .Where(s => !s.IsBlackListed && !s.IsDeleted
                && s.Subscribtions.OrderByDescending(s => s.Id).First().EndDate.AddDays(-5) == DateTime.Today)
                .ToListAsync();

            foreach (var subscriber in subscribers)
                await _emailSender.SendEmailAsync(subscriber.Email, "Bookify Renewal", $"Subscribtion will expired at {DateTime.Today.AddDays(5)} 😟");
        }
    }
}
