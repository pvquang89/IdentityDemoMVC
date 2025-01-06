using IdentityDemo.Models;
using IdentityDemo.Models.LoginVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //IdentityUser : class đại diện cho user 
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    //không nên gán password ở đây vì ko được hash
                };
                // CreateAsync: tạo user mới và hash password, trả về kiểu IdentityResult
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //SignInAsync : đăng nhập ngay khi tạo acc
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }

                //Nếu có lỗi sẽ trả về list error của result (xem Identity class để rõ) 
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); //string.Empty : không liên kết với trường cụ thể nào
                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            ViewData["ReturnUrl"]= ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //ReturnUrl được nhận từ form truyền vào đây
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                //PasswordSignInAsync : kiểm tra thông tin đăng nhập
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
                                                            model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //IsLocalUrl() : kiểm tra xem một URL có phải là một URL nội bộ hay không
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }

                if (result.RequiresTwoFactor)
                {
                    // Handle two factor authentication case
                }

                if (result.IsLockedOut)
                {
                    // Handle lockout scenario
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //SignOutAsync : xoá thông tin user
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }


        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")] //chấp nhận cả 2 phương thức
        public async Task<IActionResult> IsEmailAvailable(string Email)
        {
          var user = await userManager.FindByEmailAsync(Email);
            if (user == null)
                return Json(true);
            else
                return Json($"Email {Email} is already in use.");
        }


    }
}
