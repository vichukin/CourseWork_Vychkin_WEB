using System.ComponentModel.DataAnnotations;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.AccountViewModels
{
    public class RegisterViewModel 
    {
        [Required]
        [Display(Name = "Displayed name for other users")]
        public string DisplayName { get; set; } = default!;
        [Required]
        [Display(Name ="Login")]
        public string Login { get; set; } = default!;
        [Required]
        [Display(Name ="Email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = default!;
        [Required]
        [Display(Name = "Choose your avatar")]
        public IFormFile Avatar { get; set; }
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),ErrorMessage="The passwords must match!")]
        public string ConfirmPassword { get; set; } = default!;

        public string? ReturnUrl { get; set; }
    }
}
