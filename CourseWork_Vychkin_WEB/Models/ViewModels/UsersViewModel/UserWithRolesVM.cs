using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Identity;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.UsersViewModel
{
	public class UserWithRolesVM
	{
		public User User { get; set; }
		public List<string>? Roles { get; set; }
	}
}
