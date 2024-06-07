using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

public class RoleController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RoleController> _logger;

    public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RoleController> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var roles =  _roleManager.Roles.ToListAsync();
        return View(roles);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            ModelState.AddModelError("", "Role name is required.");
            return View();
        }

        var roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} created successfully.");
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                _logger.LogError($"Error creating role: {roleName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            ModelState.AddModelError("", "Role already exists.");
        }

        return View();
    }

    public IActionResult ManageUserRoles()
    {
        var users = _userManager.Users;
        return View(users);
    }

    public async Task<IActionResult> EditUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var roles = _roleManager.Roles.ToList(); // Synchronous call

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            AvailableRoles = roles,
            UserRoles = userRoles
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUserRoles(EditUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.SelectedRoles ?? new string[] { };

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded)
        {
            return View(model);
        }

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            return View(model);
        }

        return RedirectToAction("ManageUserRoles");
    }
}
