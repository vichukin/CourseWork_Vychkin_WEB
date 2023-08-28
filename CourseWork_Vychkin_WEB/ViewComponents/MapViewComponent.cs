using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork_Vychkin_WEB.ViewComponents
{
    [ViewComponent]
    public class MapViewComponent : ViewComponent
    {
        private readonly HousesDBContext context;
        public MapViewComponent(HousesDBContext context)
        {
            this.context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(int Width, int Height)
        {
            (int,int) attr = (Width,Height);
            return View(attr);
        }
    }
}
