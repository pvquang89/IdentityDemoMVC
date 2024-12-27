using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
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
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
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
    }
}
