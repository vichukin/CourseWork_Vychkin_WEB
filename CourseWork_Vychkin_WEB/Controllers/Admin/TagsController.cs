using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseWork_Vychkin_WEB.Data;
using CourseWork_Vychkin_WEB.Models.ViewModels.TagsViewModels;
using Azure.Storage.Blobs.Models;
using System.ComponentModel;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;

namespace CourseWork_Vychkin_WEB.Controllers.Admin
{
    [Authorize(Roles ="Admin")]
    public class TagsController : Controller
    {
        private readonly HousesDBContext _context;
        BlobServiceClient blob;
        BlobContainerClient container;

        public TagsController(HousesDBContext context, BlobServiceClient client)
        {
            _context = context;
            this.blob = client;
            container = this.blob.GetBlobContainerClient("tagsimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
              return _context.Tags != null ? 
                          View(await _context.Tags.ToListAsync()) :
                          Problem("Entity set 'HousesDBContext.Tags'  is null.");
        }

        // GET: Tags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tags == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // GET: Tags/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTagVM vm)
        {
            if (ModelState.IsValid)
            {
                string filename = $"{Guid.NewGuid()}{Path.GetExtension(vm.Image.FileName)}";
                BlobClient client = container.GetBlobClient(filename);
                await client.UploadAsync(vm.Image.OpenReadStream());
                var tag = new Tag() { Name= vm.Name, ImagePath=client.Uri.AbsoluteUri };
                _context.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Tags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tags == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        // POST: Tags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,IFormFile? File, Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var prevTag = _context.Tags.First(t => t.Id == tag.Id);
                    if (File != null)
                    {
                        
                        string blobName = Path.GetFileName(prevTag.ImagePath);
                        BlobClient blobClient = container.GetBlobClient(blobName);
                        await blobClient.DeleteIfExistsAsync();
                        string filename = $"{Guid.NewGuid()}{Path.GetExtension(File.FileName)}";
                        BlobClient client = container.GetBlobClient(filename);
                        await client.UploadAsync(File.OpenReadStream());
                        prevTag.ImagePath = client.Uri.AbsoluteUri;
                    }
                    prevTag.Name = tag.Name;
                    _context.Update(prevTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tags == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tags == null)
            {
                return Problem("Entity set 'HousesDBContext.Tags'  is null.");
            }
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
          return (_context.Tags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
