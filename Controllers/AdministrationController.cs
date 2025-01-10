using IdentityDemo.Models;
using IdentityDemo.Models.Claims;
using IdentityDemo.Models.RoleVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace IdentityDemo.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdministrationController : Controller
    {

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdministrationController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreateRolePolicy")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                //check if role already exists
                bool roleExists = await _roleManager.RoleExistsAsync(roleModel.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Role already exists");
                }
                else
                {
                    //tạo 1 đối tượng role
                    ApplicationRole identityRole = new ApplicationRole
                    {
                        Name = roleModel?.RoleName,
                        Description = roleModel?.Description
                    };

                    //identityResult : class cho biết thao tác có thành công hay không 
                    IdentityResult result = await _roleManager.CreateAsync(identityRole);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(roleModel);
        }

        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return View("Error");
            }
            //tạo đối tươngk EidtRoleViewModel để hiện dữ liệu
            var model = new EditRoleViewModel
            {
                Id = roleId,
                RoleName = role.Name,
                Description = role.Description,
                Users = new List<string>()
            };

            //check list user có user nào thuộc role hiện tại 
            foreach (var user in _userManager.Users.ToList())
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    //nếu đúng thì add vào list
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    // Handle the scenario when the role is not found
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                    return View("Error");
                }
                else
                {
                    role.Name = model.RoleName;
                    role.Description = model.Description;
                    //update
                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles"); // Redirect to the roles list
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy ="DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = "Cannot be found this role id";
                return BadRequest();
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("ListRoles", await _roleManager.Roles.ToListAsync());
        }


        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.RoleId = roleId;
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            ViewBag.RoleName = role.Name;
            //tạo list để chứa UserRoleViewModel hiện thị lên view
            var model = new List<UserRoleViewModel>();

            //
            foreach (var user in _userManager.Users.ToList())
            {
                //tạo đối tượng để thêm vào list bên trên
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                //check user hiện tại đã nằm trong role hay chưa
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);

                IdentityResult? result;
                //nếu đã đc chọn và user không thuộc role hiện tại
                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                //nếu không được chọn và user đang thuộc role hiện tại
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    //nếu trạng thái user ko có j thay đổi, tiếp tục loop
                    continue;
                }

                //check đã thêm hoặc xoá thành công (ko cần thiết lắm)
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { roleId = roleId });
                }
            }
            //vì EditRole() có tham số nên cần tham số thứ 2
            return RedirectToAction("EditRole", new { roleId = roleId });
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            //Check if User Exists in the Database
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return NotFound();
            }

            //lấy list claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            //lấy list roles
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                //vì userClaims là kiểu claims lên phải chuyển từng phần tử sang string
                Claims = userClaims.Select(c => c.Value).ToList(),
                //roles không cần vì userRoles đã là kiểu string
                Roles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                //update 
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    //In case any error, stay in the same view and show the model validation error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            ViewBag.UserName = user.UserName;

            //
            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            //
            var existingUserClaims = await _userManager.GetClaimsAsync(user);
            //
            foreach (var claim in ClaimsStore.GetAllClaims())
            {
                //tạo obj UserClaim
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                //check claim của user có giống với claim trong list được cung cấp hay không 
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    //nếu giống, đặt trạng thái cho checkbox là true
                    userClaim.IsSelected = true;
                }
                // thêm obj userClaim vừa được tạo vào danh sách claim để hiện thị trên view
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            //lấy claims của user và xoá 
            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            //lấy ra các claims đã chọn trên ui
            var allSelectClaims = model.Claims.Where(c => c.IsSelected) //lọc ra các claims được chọn
                                .Select(c => new Claim(c.ClaimType, c.ClaimType)) //tạo obj Claim từ các claim được chọn
                                .ToList();

            //check nếu có bất kì claims đượp chọn 
            if (allSelectClaims.Any())
            {
                //thì thêm vào 
                result = await _userManager.AddClaimsAsync(user, allSelectClaims);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected claims to user");
                    return View(model);
                }
            }
            return RedirectToAction("EditUser", new { userId = model.UserId });
        }

    }
}
