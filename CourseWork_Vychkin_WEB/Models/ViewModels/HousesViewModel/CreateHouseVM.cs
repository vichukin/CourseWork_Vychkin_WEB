using CourseWork_Vychkin_WEB.Data;
using System.ComponentModel.DataAnnotations;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.HousesViewModel
{
    public class CreateHouseVM
    {
        public string Description { get; set; } = default!;
        [Required]
        public double Price { get; set; }
        [Required]
        public int Category { get; set; }
        [Required]
        public int MainImage { get; set; }
        public int SquareMeter { get; set; }
        public int Rooms { get; set; }
        public Address Address { get; set; } = default!;

        public List<int> Tags { get; set; }
        public List<IFormFile> Images { get; set; } = default!;

    }
}
