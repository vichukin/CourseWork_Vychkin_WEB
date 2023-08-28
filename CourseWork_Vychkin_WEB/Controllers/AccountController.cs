using Microsoft.AspNetCore.Mvc;
using CourseWork_Vychkin_WEB.Models.ViewModels.AccountViewModels;
using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CourseWork_Vychkin_WEB.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<User> UserManager { get; set; }
        public SignInManager<User> SignInManager { get; set; }
        BlobServiceClient blob;
        BlobContainerClient container;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, BlobServiceClient client)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            this.blob = client;
            container = this.blob.GetBlobContainerClient("userimages");
            container.CreateIfNotExists();
            container.SetAccessPolicy(PublicAccessType.BlobContainer);

        }
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public async Task< IActionResult> GoogleAuth()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user != null)
            {
                return RedirectToAction("Index", "Home");
            }
            string redirecturl = Url.Action("GoogleRedirect", "Account");
            var properties = SignInManager.ConfigureExternalAuthenticationProperties("Google", redirecturl);
            var chall = new ChallengeResult("Google", properties);
            return chall;

        }
        public async Task<IActionResult> GoogleRedirect()
        {
            ExternalLoginInfo loginInfo = await SignInManager.GetExternalLoginInfoAsync();
            var pr = loginInfo.Principal;
            if (loginInfo == null) return RedirectToAction("Login");

            var res = await SignInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, false);

            
            string[] userinfo =
            {
                loginInfo.Principal.FindFirst(ClaimTypes.Name)?.Value,
                loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value,
                loginInfo.Principal.FindFirstValue("picture")
            };
            if (res.Succeeded)
            {
                var us = await UserManager.FindByEmailAsync(userinfo[1]);
                var claims = new List<Claim>()
                    {
                        new Claim("DisplayName", us.DisplayName),
                        new Claim("ImagePath", us.ImagePath),
                        new Claim("Id", us.Id)
                    };
                await UserManager.AddClaimsAsync(us, claims);

                return RedirectToAction("Index", "Home");

            }
            WebClient client = new WebClient();
            byte[] imageData = client.DownloadData(userinfo[2]);
            string filename = $"{Guid.NewGuid()}.png";
            BlobClient cl = container.GetBlobClient(filename);
            using (Stream stream = new MemoryStream())
            {
                using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(imageData))
                {
                    image.Mutate(x => x.Resize(500, 500));
                    image.SaveAsPng(stream);
                }
                stream.Position = 0;
                await cl.UploadAsync(stream);
            }

            User user = new User { DisplayName = userinfo[0], UserName = userinfo[1].ToLower().Split('@')[0], Email = userinfo[1], ImagePath = cl.Uri.AbsoluteUri };
           
            try
            {
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    var us = await UserManager.FindByEmailAsync(userinfo[1]);
                    var claims = new List<Claim>()
                    {
                        new Claim("DisplayName", user.DisplayName),
                        new Claim("ImagePath", user.ImagePath),
                        new Claim("Id", us.Id)
                    };
                    await UserManager.AddToRoleAsync(us, "User");
                    await UserManager.AddClaimsAsync(us, claims);
                    result = await UserManager.AddLoginAsync(user, loginInfo);
                    if (result.Succeeded)
                    {

                        await SignInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    User? findedUser =
                    await UserManager.Users.FirstOrDefaultAsync(t => t.NormalizedEmail == user.Email!.ToUpper());
                    if (findedUser != null)
                    {
                        await UserManager.AddLoginAsync(findedUser!, loginInfo);
                        var us = await UserManager.FindByEmailAsync(userinfo[1]);
                        var claims = new List<Claim>()
                    {
                        new Claim("DisplayName", findedUser.DisplayName),
                        new Claim("ImagePath", findedUser.ImagePath),
                        new Claim("Id", us.Id)
                    };
                        await UserManager.AddClaimsAsync(findedUser, claims);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch
            {
                cl.DeleteIfExists();
                return RedirectToAction("Register", new { error = "yes" });
            }
            cl.DeleteIfExists();
            return RedirectToAction("SignIn");
        }
        public async Task<IActionResult> Register(string? ReturnUrl,string? error)
        {
            var user = await UserManager.GetUserAsync(User);
            if (user != null)
            {
               return RedirectToAction("Index", "Home");
            }
            if (error!= null)
                ModelState.AddModelError(string.Empty, "This email already registered!");
            RegisterViewModel vm = new RegisterViewModel() { ReturnUrl = ReturnUrl };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            var bufus = await UserManager.GetUserAsync(User);
            if (bufus != null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                string url= "https://coursework.blob.core.windows.net/userimages/uknown.jpg";
                if (vm.Avatar != null)
                {
                    string filename = $"{Guid.NewGuid()}{Path.GetExtension(vm.Avatar.FileName)}";
                    BlobClient client = container.GetBlobClient(filename);
                    await client.UploadAsync(vm.Avatar.OpenReadStream());
                    url = client.Uri.AbsoluteUri;
                }
                User user = new User()
                {
                    Email = vm.Email,
                    UserName = vm.Login,
                    ImagePath = url,
                    DisplayName = vm.DisplayName
                };
                try{
                    var result = await UserManager.CreateAsync(user, vm.Password);
                    if (result.Succeeded)
                    {
                        var us = await UserManager.FindByNameAsync(vm.Login);
                        var claims = new List<Claim>()
                    {
                        new Claim("DisplayName", us.DisplayName),
                        new Claim("ImagePath", us.ImagePath),
                        new Claim("Id", us.Id)
                    };
                        await UserManager.AddToRoleAsync(us, "User");
                        await UserManager.AddClaimsAsync(us, claims);
                        await SignInManager.SignInAsync(us, false);
                        return RedirectToAction("Index", "Home");
                    }
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, item.Description);
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "This email already registered!");
                }
                
            }
            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> SignIn(string? ReturnUrl)
        {
            var bufus = await UserManager.GetUserAsync(User);
            if (bufus != null)
            {
               return RedirectToAction("Index", "Home");
            }
            LoginViewModel vm = new LoginViewModel() { ReturnUrl=ReturnUrl};
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel vm)
        {
            var bufus = await UserManager.GetUserAsync(User);
            if (bufus != null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.IsPersistent, false);
                if(result.Succeeded)
                {
                    var user = await UserManager.FindByNameAsync(vm.Login);
                    var claims = new List<Claim>()
                    {
                        new Claim("DisplayName", user.DisplayName),
                        new Claim("ImagePath", user.ImagePath),
                        new Claim("Id", user.Id)
                    };
                    await UserManager.AddClaimsAsync(user, claims);

                    if (!string.IsNullOrEmpty(vm.ReturnUrl)&&Url.IsLocalUrl(vm.ReturnUrl))
                    {

                        return Redirect(vm.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index","Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Password or login are wrong!");
            }
            return View(vm);
        }
        public async Task<IActionResult> LogOut(string? ReturnUrl)
        {
            await SignInManager.SignOutAsync();
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
