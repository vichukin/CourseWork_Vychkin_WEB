using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CourseWork_Vychkin_WEB.Data;
using CourseWork_Vychkin_WEB.Models.ViewModels.UsersViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_Vychkin_WEB.Controllers.Admin
{
    public class UsersController : Controller
    {
        // GET: UsersController
        HousesDBContext context;
        public UserManager<User> UserManager { get; set; }
        public RoleManager<IdentityRole> RoleManager { get; set; }
        public SignInManager<User> SignInManager { get; set; }
        BlobServiceClient blob;
        BlobContainerClient container;
        public UsersController(HousesDBContext context, UserManager<User> userManager, SignInManager<User> signInManager, BlobServiceClient client, RoleManager<IdentityRole> RoleManager)
        {
            this.context = context;
            UserManager = userManager;
            SignInManager = signInManager;
            this.blob = client;
            container = this.blob.GetBlobContainerClient("userimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
            this.RoleManager = RoleManager;
        }
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> Index()
        {
            var users = UserManager.Users.ToList();
            List<UserWithRolesVM> list = new List<UserWithRolesVM>();
            foreach(var user in users)
            {
                var roles = await UserManager.GetRolesAsync(user);
                list.Add(new UserWithRolesVM() { User = user, Roles = roles.ToList() }) ;
            }
            return View(list);
        }

        // GET: UsersController/Details/5
        public async Task<ActionResult> Details(string Id, int menu = 1,int isModerated=0)
        {
            var user = await UserManager.FindByIdAsync(Id);
            List<House> houses = new List<House>();
            List<Rent> rents = new List<Rent>();
            if(menu==1)
            {
                houses = context.Houses.Include(t=>t.User)
                    .Include(h => h.Address)
                    .Include(h => h.Category)
                .Include(h => h.Images)
                .Include(h => h.Rents)
                .Include(_ => _.Tags)
                    .Where(t=>t.User.Id==user.Id).ToList();
                if(isModerated==1)
                    houses = houses.Where(t=>t.IsModerated==true).ToList();
                else if(isModerated==2)
                    houses = houses.Where(t=>t.IsModerated!=true).ToList();
            }
            if (menu == 2)
                rents = context.Rents.Include(t=>t.House).Include(t=>t.House.Address).Include(t=>t.User).Include(t=>t.House.Images).Include(t=>t.House.Category).Include(t=>t.House.Tags).Where(t => t.User == user).ToList();
            //ViewData["IsModerated"] = isModerated;
            var vm = new UserVM() { Id= user.Id, DisplayName=user.DisplayName, ImagePath=user.ImagePath,Menu = menu, Houses=houses,Rents=rents ,IsModerated=isModerated};
            return View(vm);
        }

        // GET: UsersController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: UsersController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: UsersController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: UsersController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: UsersController/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string Id, int menu = 1, int isModerated = 0)
        {
            var user = await UserManager.FindByIdAsync(Id);
            List<House> houses = new List<House>();
            List<Rent> rents = new List<Rent>();
            if (menu == 1)
            {
                houses = context.Houses.Include(t => t.User)
                    .Include(h => h.Address)
                    .Include(h => h.Category)
                .Include(h => h.Images)
                .Include(h => h.Rents)
                .Include(_ => _.Tags)
                    .Where(t => t.User.Id == user.Id).ToList();
                if (isModerated == 1)
                    houses = houses.Where(t => t.IsModerated == true).ToList();
                else if (isModerated == 2)
                    houses = houses.Where(t => t.IsModerated != true).ToList();
            }
            if (menu == 2)
                rents = context.Rents.Include(t => t.House).Include(t => t.House.Address).Include(t => t.User).Include(t => t.House.Images).Include(t => t.House.Category).Include(t => t.House.Tags).Where(t => t.User == user).ToList();
            ViewData["IsModerated"] = isModerated;
            var vm = new UserVM() { Id = user.Id, DisplayName = user.DisplayName, ImagePath = user.ImagePath, Menu = menu, Houses = houses, Rents = rents };
            return View(vm);
        }

        // POST: UsersController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string Id)
        {
            if (ModelState.IsValid)
            {
                if (Id == null)
                    return NotFound();
                var user = await UserManager.FindByIdAsync(Id);
                if (user == null)
                    return NotFound();
                var images = context.Images.Include(t => t.House).Include(t => t.House.User).Where(t => t.House.User.Id == user.Id);
                foreach(var image in images)
                {
                    var client = container.GetBlobClient(image.Path);
                    await client.DeleteIfExistsAsync();
                    context.Remove(image);
                }
                await context.SaveChangesAsync();
                var Houses = context.Houses.Include(t=>t.User).Where(t=>t.User.Id== user.Id);
                foreach(var house in Houses)
                {
                    context.Remove(house);
                }    
                await context.SaveChangesAsync();
                var rents = context.Rents.Include(t=>t.User).Where(t=>t.User.Id==user.Id);
                foreach(var rent in rents)
                {
                    context.Remove(rent);
                }    
                await context.SaveChangesAsync();
                var result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Users");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(string Id)
        {
            //await RoleManager.CreateAsync(new IdentityRole("Admin"));
            //await RoleManager.CreateAsync(new IdentityRole("User"));
            var user = await UserManager.FindByIdAsync(Id);
            await UserManager.AddToRoleAsync(user, "Admin");
            await UserManager.AddToRoleAsync(user, "User");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangeDisplayName(string Id, string CurrentId,string DisplayName)
        {
            if(ModelState.IsValid)
            {
                if (Id == CurrentId)
                {
                    var user = await UserManager.FindByIdAsync(Id);
                    user.DisplayName= DisplayName;
                    await UserManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("Details", new {Id=Id});
        }
    }
}
