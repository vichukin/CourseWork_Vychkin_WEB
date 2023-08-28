using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_Vychkin_WEB.ViewComponents
{
    public class ShowAddressViewComponent : ViewComponent
    {
        private readonly HousesDBContext context;
        public ShowAddressViewComponent(HousesDBContext context)
        {
            this.context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync( int Id)
        {
            var house = context.Houses.Include(t=>t.Address).First(x => x.Id == Id);
            return View(house);
        }
    }
}
