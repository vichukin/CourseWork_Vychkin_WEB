using CourseWork_Vychkin_WEB.Data;
using Image = CourseWork_Vychkin_WEB.Data.Image;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.HousesViewModel
{
    public class EditHouseVM
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Category { get; set; }
        public string? MainImage { get; set; }
        public int SquareMeter { get; set; }
        public int Rooms { get; set; }
        public List<int>? Tags { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
