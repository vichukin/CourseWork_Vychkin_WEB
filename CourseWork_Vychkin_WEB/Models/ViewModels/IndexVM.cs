using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseWork_Vychkin_WEB.Models.ViewModels
{
    public class IndexVM
    {
        public List<House> Houses { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
       
    }
}
