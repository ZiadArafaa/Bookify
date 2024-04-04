using AutoMapper;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var models = _mapper.Map<IEnumerable<ApplicationUserViewModel>>(users);

            foreach (var userViewModel in models)
            {
                userViewModel.CreatedBy = await _userManager.Users.Where(u => u.Id == userViewModel.CreatedBy)
               .Select(u => u.UserName).SingleOrDefaultAsync();

                if (userViewModel.UpdatedBy is not null)
                {
                    userViewModel.UpdatedBy = await _userManager.Users.Where(u => u.Id == userViewModel.UpdatedBy)
                   .Select(u => u.UserName).SingleOrDefaultAsync();
                }
            }

            return View(models);
        }
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            return PartialView("_Form", await PopulateUserFormAsync());
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByNameAsync(model.UserName) ?? await _userManager.FindByEmailAsync(model.Email);

            if (user is not null)
                return BadRequest();

            ApplicationUser applicationUser = new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreateOn = DateTime.Now,
            };

            var result = await _userManager.CreateAsync(applicationUser, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(applicationUser, model.SelectedRoles);

                var viewModel = _mapper.Map<ApplicationUserViewModel>(applicationUser);

                viewModel.CreatedBy = await _userManager.Users
                    .Where(u => u.Id == viewModel.CreatedBy)
                    .Select(u => u.UserName).SingleOrDefaultAsync();

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                       "/Account/ConfirmEmail",
                       pageHandler: null,
                       values: new { area = "Identity", userId = applicationUser.Id, code = code },
                       protocol: Request.Scheme);

                _logger.LogInformation(callbackUrl);
                await _emailSender.SendEmailAsync(applicationUser.Email,
                    "Confirm Your Email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description).ToList()));
        }
        [AjaxOnly]
        public async Task<IActionResult> Edit(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            var model = new ApplicationUserFormViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SelectedRoles = await _userManager.GetRolesAsync(user),
            };
            return PartialView("_Form", await PopulateUserFormAsync(model));
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            //Todo:
            //add mapper
            user.UserName = model.UserName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.LastUpdatedOn = DateTime.Now;
            user.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(',', result.Errors.Select(e => e.Description).ToList()));

            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.SequenceEqual(model.SelectedRoles))
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }

            var viewModel = _mapper.Map<ApplicationUserViewModel>(user);

            //Todo:
            //add single responsablility
            viewModel.UpdatedBy = await _userManager.Users.Where(u => u.Id == user.UpdatedById).Select(u => u.UserName).SingleOrDefaultAsync();
            viewModel.CreatedBy = await _userManager.Users.Where(u => u.Id == user.CreatedById).Select(u => u.UserName).SingleOrDefaultAsync();

            return PartialView("_UserRow", viewModel);
        }
        [AjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedOn = DateTime.Now;
            user.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)).ToList());
            //ToDo:
            //add username claim 
            return Ok(new
            {
                date = user.LastUpdatedOn.ToString()
                ,
                user = await _userManager.Users.Where(i => i.Id == user.UpdatedById).Select(u => u.UserName).SingleOrDefaultAsync()
            });
        }
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user is null)
                return NotFound();
            return PartialView("_Reset", new ResetFormViewModel { Id = user.Id });
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var hashPassword = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (!result.Succeeded)
            {
                user.PasswordHash = hashPassword;
                await _userManager.UpdateAsync(user);
                return BadRequest(string.Join(',', result.Errors.Select(u => u.Description).ToList()));
            }

            return Ok();
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnLockUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user is null)
                return NotFound();

            if (await _userManager.IsLockedOutAsync(user))
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, null);

                if (!result.Succeeded)
                    return BadRequest(string.Join(',', result.Errors.Select(e => e.Description).ToList()));
            }

            return Ok();
        }

        public async Task<JsonResult> AllowUsername(ApplicationUserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isValid = user is null || model.Id == user.Id;
            return Json(isValid);
        }
        public async Task<JsonResult> AllowEmail(ApplicationUserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var isValid = user is null || model.Id == user.Id;
            return Json(isValid);
        }
        private async Task<ApplicationUserFormViewModel> PopulateUserFormAsync(ApplicationUserFormViewModel? model = null)
        {
            model ??= new ApplicationUserFormViewModel();

            model.Roles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();

            return model;
        }
    }
}
