using CourseWork_Vychkin_WEB.Data;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.UsersViewModel
{
    public class UserVM
    {
        public string Id { get; set; } = default!;
        public string ImagePath { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public int Menu { get; set; } = default!;
        public int IsModerated { get; set; } = default!;
        public List<House>? Houses { get; set; }
        public List<Rent>? Rents { get; set; }
    }
}
