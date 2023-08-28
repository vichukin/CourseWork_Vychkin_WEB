using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Image = CourseWork_Vychkin_WEB.Data.Image;

namespace CourseWork_Vychkin_WEB.Controllers.Admin
{
    public class ImagesController : Controller
    {
        private readonly HousesDBContext _context;
        BlobServiceClient blob;
        private readonly UserManager<User> UserManager;
        BlobContainerClient container;

        public ImagesController(HousesDBContext context, BlobServiceClient client, UserManager<User> userManager)
        {
            _context = context;
            this.blob = client;
            this.UserManager = userManager;
            container = this.blob.GetBlobContainerClient("houseimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Id, List<int> Images)
        {
            if(ModelState.IsValid)
            {
                bool mainDeleted=false;
                var images = _context.Images.Where(t => Images.Contains(t.Id));
                int countOfImg = _context.Images.Count(t => t.HouseId == Id);
                if(countOfImg == images.Count()) 
                    images = images.Skip(1);
                foreach (var image in images)
                {
                    if(!mainDeleted)
                        mainDeleted = image.IsMain;
                    var client = container.GetBlobClient(image.Path);
                    await client.DeleteIfExistsAsync();
                }
                _context.RemoveRange(images);
                await _context.SaveChangesAsync();
                if (mainDeleted)
                {
                    var img = _context.Images.First(t => t.HouseId == Id);
                    img.IsMain = true;
                    _context.Update(img);
                }
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("Edit","Houses",new {Id=Id});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMainImage(int Id, int MainImage)
        {
            if(ModelState.IsValid)
            {

                var Images = _context.Images.Where(t => t.HouseId == Id);
                var Main = Images.FirstOrDefault(t => t.IsMain == true);
                if(Main!=null)
                {
                    Main.IsMain = false;
                    _context.Update(Main);
                }    
                var newMain= Images.First(t => t.Id == MainImage);
                newMain.IsMain = true;
                _context.Update( newMain);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Edit", "Houses", new { Id = Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int Id, List<IFormFile> Images)
        {
            if(ModelState.IsValid)
            {
                var house = _context.Houses.First(t => t.Id == Id);
                foreach(var item in Images)
                {
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

                    Image img = new Image() { House = house, HouseId = Id, IsMain = false, Path = client.Uri.AbsoluteUri };
                    _context.Add(img);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Edit", "Houses", new { Id = Id });
        }
    }
}
