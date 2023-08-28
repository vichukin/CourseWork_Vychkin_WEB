using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseWork_Vychkin_WEB.Models.ViewModels.HousesViewModel
{
    public class PrepareForHouseEditVM
    {
        public SelectList Categories { get; set; }
        public SelectList Tags { get; set; }
        public House House { get; set; }
    }
}
