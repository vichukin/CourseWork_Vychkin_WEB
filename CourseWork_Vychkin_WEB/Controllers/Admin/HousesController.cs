using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork_Vychkin_WEB.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Image = CourseWork_Vychkin_WEB.Data.Image;
using Microsoft.Extensions.Azure;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CourseWork_Vychkin_WEB.Models.ViewModels.HousesViewModel;

namespace CourseWork_Vychkin_WEB.Controllers.Admin
{
    public class HousesController : Controller
    {
        private readonly HousesDBContext _context;
        BlobServiceClient blob;
        private readonly UserManager<User> UserManager;
        BlobContainerClient container;

        public HousesController(HousesDBContext context, BlobServiceClient client, UserManager<User> userManager)
        {
            _context = context;
            this.blob = client;
            this.UserManager = userManager;
            container = this.blob.GetBlobContainerClient("houseimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            if (id == null || _context.Houses == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Address)
                .Include(h => h.Images)
                .Include(h => h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (house == null)
            {
                return NotFound();
            }
            house.IsModerated= true;
            _context.Update(house);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> RentHouse()
        {
            return RedirectToAction("Index","Home");
        }
        [Authorize(Roles ="User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RentHouse(int Id, string? From, string? To, double? Price,int CountOfDay)
        {
            var house = await _context.Houses
                    .Include(h => h.Address)
                    .Include(h => h.Images)
                    .Include(h => h.Rents)
                    .Include(h => h.User)
                    .Include(_ => _.Tags)
                    .FirstOrDefaultAsync(m => m.Id == Id);
            if (ModelState.IsValid)
            {
                var username = User.Identity.IsAuthenticated ? User.Identity.Name : "";
                var user = _context.Users.Where(t => t.UserName == username).FirstOrDefault();
                Rent rent = new Rent() { From = From, To = To, HouseId = Id, Username = username, Price = Price, CountOfDay = CountOfDay, User = user };
                await _context.AddAsync(rent);
                await _context.SaveChangesAsync();
                
                
            }
            return RedirectToAction("Details", house);
        }

        // GET: Houses
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? IsModerated = 0)
        {
            IQueryable<House> housesDBContext = _context.Houses.Include(h => h.Address)
                .Include(h => h.Images)
                .Include(h => h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags);
            if (IsModerated == null)
                IsModerated = 0;
            if(IsModerated !=null && IsModerated!=0)
                if(IsModerated==1)
                   housesDBContext = housesDBContext.Where(h => h.IsModerated == true);
                else
                   housesDBContext = housesDBContext.Where(_ => _.IsModerated == false);
            ViewData["IsModerated"] = IsModerated;
            return View(await housesDBContext.ToListAsync());
        }

        // GET: Houses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Houses == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Address)
                .Include(h=>h.Images)
                .Include(h=>h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        // GET: Houses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Tags"] = new SelectList(_context.Tags, "Id", "Name");
            return View();
        }

        // POST: Houses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHouseVM vm)
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Tags"] = new SelectList(_context.Tags, "Id", "Name");
            if (ModelState.IsValid)
            {
                
                List<Image> images = new List<Image>();
                List<Tag> tags = new List<Tag>();
                Category category = _context.Categories.First(c => c.Id == vm.Category);
                _context.Add(vm.Address);
                await _context.SaveChangesAsync();
                var username = User.Identity.IsAuthenticated ? User.Identity.Name : "";
                var user = _context.Users.Where(t=> t.UserName== username).FirstOrDefault();
                House house = new House() { Address = vm.Address, AddressId=vm.Address.Id, Images = images, Description = vm.Description, Price = vm.Price, Username = username,User = user, Category = category, CategoryId=category.Id, Tags=tags, Rooms=vm.Rooms,SquareMeter=vm.SquareMeter };
                _context.Add(house);
                await _context.SaveChangesAsync();
                if (vm.Images != null)
                {
                    foreach (var item in vm.Images)
                    {
                        bool isMain = vm.Images.IndexOf(item) == vm.MainImage;
                        string filename = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";
                        BlobClient client = container.GetBlobClient(filename);
                        using (Stream stream = new MemoryStream())
                        {
                            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(item.OpenReadStream()))
                            {
                                image.Mutate(x => x.Resize(500, 500));
                                image.SaveAsPng(stream);
                            }
                            stream.Position = 0;
                            await client.UploadAsync(stream);   
                        }
                        
                        Image img = new Image() { House = house, HouseId = house.Id, IsMain = isMain, Path = client.Uri.AbsoluteUri };
                        images.Add(img);
                    }
                }
                await _context.AddRangeAsync(images);
                await _context.SaveChangesAsync();
                if (vm.Tags != null)
                {
                    foreach (var item in vm.Tags)
                    {
                        Tag tag = await _context.Tags.FirstAsync(t => t.Id == item);
                        tags.Add(tag);
                    }

                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "Id", house.AddressId);
            return View(vm);
        }

        // GET: Houses/Edit/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Houses == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Address)
                .Include(h => h.Images)
                .Include(h => h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags).FirstOrDefaultAsync(t=>t.Id==id);
            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            if(!roles.Contains("Admin")&&user.Id!=house.User.Id)
            {
                return NotFound();
            }
            if (house == null)
            {
                return NotFound();
            }
            PrepareForHouseEditVM vm = new PrepareForHouseEditVM()
            {
                Tags = new SelectList(_context.Tags, "Id", "Name", house.Tags),
                Categories = new SelectList(_context.Categories, "Id", "Name", house.Category),
                House = house
            };
            return View(vm);
        }

        // POST: Houses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditHouseVM vM)
        {
            if (id != vM.Id)
            {
                return NotFound();
            }
            var House = await _context.Houses
                .Include(h => h.Address)
                .Include(h => h.Category)
                .Include(h => h.Images)
                .Include(h => h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags).FirstOrDefaultAsync(t => t.Id == id);
            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && user.Id != House.User.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    House.Rooms = vM.Rooms;
                    House.Price = vM.Price;
                    House.Description = vM.Description;
                    House.SquareMeter=vM.SquareMeter;
                    var category = await _context.Categories.FirstAsync(c => c.Id == vM.Category);
                    House.CategoryId = vM.Category;
                    House.Category = category;
                    var tags =  _context.Tags.Where(t=>vM.Tags.Contains(t.Id)).ToList();
                    House.Tags= tags; 
                    _context.Update(House);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseExists(vM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Edit",new {Id=id });
            }
            PrepareForHouseEditVM vm = new PrepareForHouseEditVM()
            {
                Tags = new SelectList(_context.Tags, "Id", "Name", House.Tags),
                Categories = new SelectList(_context.Categories, "Id", "Name", House.Category),
                House = House
            };
            return View(vm);
        }

        // GET: Houses/Delete/5
        [Authorize(Roles ="User")]
        public async Task<IActionResult> Delete(int? id, bool? adm)
        {
            if (id == null || _context.Houses == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Address)
                .Include(h => h.Images)
                .Include(h => h.User)
                .Include(h => h.Rents)
                .Include(_ => _.Tags).FirstOrDefaultAsync(t => t.Id == id);
            var user = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && user.Id != house.User.Id)
            {
                return NotFound();
            }
            if (house == null)
            {
                return NotFound();
            }
            foreach(var img in house.Images)
            {
                var client = container.GetBlobClient(img.Path);
                await client.DeleteIfExistsAsync();

            }
            _context.Images.RemoveRange(house.Images);
            await _context.SaveChangesAsync();
            //_context.Addresses.Remove(house.Address);
            //await _context.SaveChangesAsync();
            _context.Houses.Remove(house);
            await _context.SaveChangesAsync();
            if(adm!=null&&adm==true)
               return RedirectToAction("Index", "Houses");
            else
                return RedirectToAction("Index","Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAddress(int HouseId, Address Address)
        {
            if(ModelState.IsValid)
            {
                var address = await _context.Addresses.FirstAsync(t => t.Id == Address.Id);
                address.AddressLabel = Address.AddressLabel;
                address.FormattedAddress = Address.FormattedAddress;
                address.Latitude = Address.Latitude;
                address.Longitude = Address.Longitude;
                address.City = Address.City;
                address.Country = Address.Country;
                _context.Update(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Edit", new { Id = HouseId });
        }
            private bool HouseExists(int id)
        {
          return (_context.Houses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
