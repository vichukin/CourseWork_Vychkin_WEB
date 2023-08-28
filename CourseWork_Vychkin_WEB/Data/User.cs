using Microsoft.AspNetCore.Identity;

namespace CourseWork_Vychkin_WEB.Data
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; } = default!;
        public List<House>? Houses { get; set; }
        public List<Rent>? Rents { get; set; }
        public string? ImagePath { get; set; } = default!;
        public override string Email { get; set; }
    }
}
