using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class EditUserRolesViewModel
{
    public string UserId { get; set; }
    public IEnumerable<IdentityRole> AvailableRoles { get; set; }
    public IList<string> UserRoles { get; set; }
    public IEnumerable<string> SelectedRoles { get; set; }
}
