using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Cinema.DataAccess.Data;
using Cinema.DataAccess.Repository.IRepository;
using Cinema.Models;
using Cinema.Utility;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    //[Authorize(Roles = SD.Role_Admin)] 
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(IUnitOfWork unitOfWork,
                               UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public async Task<IActionResult> Index()
        {

            var users = await _unitOfWork.ApplicationUser.GetAllAsync();
            foreach (var user in users)
            {
                user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Guest";
                // dung lo em default neu ma luc 
                // create user ko set role thi no se la guest -- quan 
            }

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser updatedUser)
        {
            if (!ModelState.IsValid) return View(updatedUser);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = updatedUser.FullName;
            user.PhoneNumber = updatedUser.PhoneNumber;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _unitOfWork.ApplicationUser.GetAllAsync();
            var usersList = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.LockoutEnd
            }).ToList();

            return Json(new { data = usersList });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser user, string role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_unitOfWork.ApplicationUser.Get(u => u.Email == user.Email) != null)
                    {
                        return Json(new { success = false, message = "Email already exists." });
                    }
                    if (_unitOfWork.ApplicationUser.Get(u => u.PhoneNumber == user.PhoneNumber) != null)
                    {
                        return Json(new { success = false, message = "Phone number already exists." });
                    }
                    if (!IsValidPhoneNumber(user.PhoneNumber ?? string.Empty))
                    {
                        return Json(new { success = false, message = "Invalid phone number format." });
                    }

                    user.UserName = user.Email; // Ensure UserName is set to Email
                    string password = GenerateRandomPassword(); // xem lai nhe .net lam gium r ko can phai lam v dau -- quan
                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        return Json(new { success = true, message = "User created successfully.", password });
                    }
                    else
                    {
                        return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error creating user: {ex.Message}" });
                }
            }
            return Json(new { success = false, message = "Invalid user data." });
        }
        private string GenerateRandomPassword()
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            var password = new StringBuilder();
            for (int i = 0; i < 12; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }
            return password.ToString();
        }
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Define a regular expression for validating phone numbers
            var phoneRegex = new Regex(@"^\d{10}$"); // Example: 10-digit phone number
            return phoneRegex.IsMatch(phoneNumber);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserField(string id, string field, string value)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            try
            {
                switch (field)
                {
                    case "FullName":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Full Name cannot be empty." });
                        }
                        user.FullName = value;
                        break;
                    case "Email":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Email cannot be empty." });
                        }
                        if (_unitOfWork.ApplicationUser.Get(u => u.Email == value && u.Id != id) != null)
                        {
                            return Json(new { success = false, message = "Email already exists." });
                        }
                        user.Email = value;
                        break;
                    case "PhoneNumber":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Phone Number cannot be empty." });
                        }
                        if (_unitOfWork.ApplicationUser.Get(u => u.PhoneNumber == value && u.Id != id) != null)
                        {
                            return Json(new { success = false, message = "Phone number already exists." });
                        }
                        if (!IsValidPhoneNumber(user.PhoneNumber ?? string.Empty))
                        {
                            return Json(new { success = false, message = "Invalid phone number format." });
                        }
                        user.PhoneNumber = value;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid field." });
                }

                _ = _unitOfWork.SaveAsync();
                return Json(new { success = true, message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating user: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var currentUserId = _userManager.GetUserId(User); // Lấy ID của người dùng hiện tại
            if (id == currentUserId)
            {
                return Json(new { success = false, message = "You cannot lock your own account." });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin) ||
                await _userManager.IsInRoleAsync(user, SD.Role_Staff))
            {
                return Json(new { success = false, message = "Cannot modify Admin or Staff accounts" });
            }

            try
            {
                // Khóa user bằng cách set LockoutEnd đến tương lai xa
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "User has been locked successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to lock user" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error locking user: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult Unlock(string id)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            try
            {
                user.LockoutEnd = null;
                _unitOfWork.SaveAsync();
                return Json(new { success = true, message = "User unlocked successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error unlocking user: {ex.Message}" });
            }
        }
        //Search admin for Cinemas
        public ApplicationUser SearchUserById(string id)
        {
            return _unitOfWork.ApplicationUser.Get(u => u.Id == id);
        }

        internal static async Task<IEnumerable<ApplicationUser>> GetUsersByRole(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string role_Admin)
        {
            var role = await roleManager.FindByNameAsync(role_Admin);
            if (role == null)
            {
                throw new ArgumentException("Role not found.");
            }

            var usersInRole = await userManager.GetUsersInRoleAsync(role_Admin);
            var applicationUsers = usersInRole.Select(user => new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.UserName, // Assuming FullName is stored in UserName
                Role = role_Admin
            });

            return applicationUsers;
        }

        public bool CurrentUser(string idEditing)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Current user id: " + userId);
            return userId == idEditing;
            //var user = await _userManager.FindByIdAsync(id);
            //return View(user);
        }
    }
}