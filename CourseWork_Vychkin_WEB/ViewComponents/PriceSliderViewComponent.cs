using CourseWork_Vychkin_WEB.Models.ViewModels;
using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork_Vychkin_WEB.ViewComponents
{
	[ViewComponent]
	public class PriceSliderViewComponent : ViewComponent
	{
		private readonly HousesDBContext context;
		public PriceSliderViewComponent(HousesDBContext context)
		{
			this.context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync(PriceSearchViewModel Prices)
		{
			return View(Prices);
		}
	}
}
