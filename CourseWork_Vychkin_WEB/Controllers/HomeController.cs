using CourseWork_Vychkin_WEB.Data;
using CourseWork_Vychkin_WEB.Models;
using CourseWork_Vychkin_WEB.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace CourseWork_Vychkin_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HousesDBContext context;
        public HomeController(ILogger<HomeController> logger, HousesDBContext context)
        {
            _logger = logger;
            this.context = context;
        }
        public async Task<IActionResult> Index(List<int>? Categories,List<int>? Tags,int? Min,int? Max, List<string>? Cities, bool? toast, int Page = 1)
        {
            //Category category = context.Categories.Include(t=>t.Houses).First();
            //House house1 = context.Houses.Include(t=>t.Categories).First();
            //house1.Categories.Add(category);
            //context.SaveChanges();
            //IQueryable<House> hotels = context.Houses.Include(t => t.Address);
            //if (selectedCountry != null)
            //    hotels =  hotels.Where(t => t.Address.country == selectedCountry);
            //var countries = await context.Addresses.Select(t => new { t.Id, Country = t.country }).Distinct().ToListAsync();
            //IndexVM indexVM = new IndexVM
            //{
            //    Countries = new SelectList(countries, "Id", "Country"),
            //};
            var houses = context.Houses.Include(t => t.Address).Include(t=>t.Tags).Include(t => t.Images).Include(t => t.Category)?.Where(t=>t.IsModerated==true).ToList();
            double maxprice = 0;
            if (houses.Count() > 0)
                maxprice = houses.Max(t => t.Price);
            if(Cities!=null&&Cities.Count>0)
            {
                houses = houses.Where(t => Cities.Contains(t.Address.City)).ToList(); 
            }
            if (Categories!=null&&Categories.Count>0)
            {
                houses = houses?.Where(t=> Categories.Contains(t.CategoryId) )?.ToList();
            }
            if(Tags!=null&&Tags.Count()>0)
            {
                houses = houses?.Where(t =>
                {
                    foreach (var item in Tags)
                    {
                        if (t.Tags != null && t.Tags.Any(tag => tag.Id == item))
                        {
                            return true;
                        }
                    }
                    return false;
                })?.ToList();
            }
            if (Min != null)
                houses = houses.Where(t => t.Price >= Min).ToList();
            if (Max != null)
                houses = houses.Where(t => t.Price <= Max).ToList();
            int itemsPerPage = 16;
            int pagecount = (int)(Math.Ceiling((decimal)houses.Count() / itemsPerPage));
            houses = houses.Skip((Page - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            if (toast!=null)
            {
                ViewData["toast"] = true;
            }
            ViewData["Cities"] = context.Addresses.Include(t=>t.House).Where(t=>t.House.IsModerated).Select(t => new CityAndCountry() { City = t.City, Country = t.Country }).GroupBy(t => t.City).Select(group => group.First()).ToList();
            ViewData["Countries"] = context.Addresses.Include(t => t.House).Where(t => t.House.IsModerated).Select(t => t.Country).Distinct().OrderBy(t=>t).ToList();
            ViewData["PriceSearch"] = new PriceSearchViewModel { Max=(int)maxprice,SelectedMax=Max,SelectedMin=Min };
            ViewData["Categories"] = context.Categories.ToList();
            ViewData["SelectedCategories"] = Categories;
            ViewData["SelectedTags"] = Tags;
            ViewData["SelectedCities"] = Cities;
            ViewData["Tags"] = context.Tags.ToList();
            IndexVM vm = new IndexVM()
            {
                Houses = houses,
                Page = Page,
                PageCount = pagecount
            };
            
            return View(vm);
        }

        public IActionResult Privacy()
        {
            House house = context.Houses.First();
            
            return View(house);
        }
		public IActionResult Test()
		{
			return View();
		}

		//[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		//public IActionResult Error()
		//{
		//    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		//}
	}
}