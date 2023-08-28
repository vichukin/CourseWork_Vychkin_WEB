using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Image = CourseWork_Vychkin_WEB.Data.Image;
using Microsoft.AspNetCore.Authorization;
using CourseWork_Vychkin_WEB.Models.ViewModels.HousesViewModel;

namespace CourseWork_Vychkin_WEB.Controllers
{
    [Authorize(Roles ="User")]
    public class UploadController : Controller
    {
        private readonly HousesDBContext _context;
        BlobServiceClient blob;
        BlobContainerClient container;

        public UploadController(HousesDBContext context, BlobServiceClient client)
        {
            _context = context;
            this.blob = client;
            container = this.blob.GetBlobContainerClient("houseimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }
        [HttpGet]
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CreateHouseVM? vm)
        {
            var Tags = _context.Tags.ToList();
            if(ModelState.IsValid)
            {
                List<Image> images = new List<Image>();
                List<Tag> tags = new List<Tag>();
                Category category = _context.Categories.First(c => c.Id == vm.Category);
                _context.Add(vm.Address);
                await _context.SaveChangesAsync();
                var username = User.Identity.IsAuthenticated ? User.Identity.Name : "";
                var user = _context.Users.Where(t => t.UserName == username).FirstOrDefault();
                House house = new House() { Address = vm.Address, AddressId = vm.Address.Id, Images = images, Description = vm.Description, Price = vm.Price,SquareMeter=vm.SquareMeter,Rooms=vm.Rooms, Username = username, User = user, Category = category, CategoryId = category.Id, Tags = tags,IsModerated=false };
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
                //ViewData["toast"] = true;
                return RedirectToAction("Index","Home", new {toast = true});
                //return View("modal");
            }
            //foreach(var val in ModelState.Values)
            //{
            //    Console.WriteLine(val);
            //}
            //if (ModelState.TryGetValue("Images", out var imgs) && imgs.ValidationState == ModelValidationState.Valid)
            //{
            //    foreach(var img in vm.Images)
            //    {
            //        Console.WriteLine(img);
            //    }
            //    return View("Images", vm);
            //}
            if (ModelState.TryGetValue("Price",out var price) && price.ValidationState==ModelValidationState.Valid)
            {
                if (vm.Price == 0 || vm.Rooms == 0 || vm.SquareMeter == 0)
                    return View("Other", vm);
                return View("Images", vm);
            }
            if (ModelState.TryGetValue("Description", out var desc) && desc.ValidationState == ModelValidationState.Valid)
            {

                return View("Other", vm);
            }
            if (ModelState.TryGetValue("Address.City", out var adr) && adr.ValidationState == ModelValidationState.Valid)
            {
                bool isValid = false;
                for(int i =4;i<=9;i++)
                {
                    isValid = ModelState.Values.ElementAt(i).ValidationState == ModelValidationState.Valid;
                }
                if(isValid)
                    return View("Description", vm);
            }
            if (ModelState.TryGetValue("Tags", out var tgs)&& tgs.ValidationState==ModelValidationState.Valid)
            {
                return View("Address", vm);
            }
            if (ModelState.TryGetValue("Category",out var cat) && cat.ValidationState == ModelValidationState.Valid)
            {
                
                (CreateHouseVM, List<Tag>) tuple = (vm, Tags);
                return View("Tags",tuple);
            }
            
            //if (ModelState.Values.First(t => t. == "Category") != null)
            var categories = _context.Categories.ToList();
            return View("Category",categories);
        }
    }
}
