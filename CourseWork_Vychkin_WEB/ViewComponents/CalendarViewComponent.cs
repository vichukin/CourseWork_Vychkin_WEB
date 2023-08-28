using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_Vychkin_WEB.ViewComponents
{
    [ViewComponent]
    public class CalendarViewComponent : ViewComponent
    {
        HousesDBContext context;
        public CalendarViewComponent(HousesDBContext context)
        {
            this.context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(int Id)
        {
            House house = await context.Houses.Include(t=>t.Rents).FirstAsync(context=> context.Id == Id);

            return View(house);
        }
    }
}
