using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
  
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitofWork _unitOfWork;
        public UserController(UserManager<IdentityUser> userManager, IUnitofWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            return View();
        }

        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult RoleManagment(string userId)
        {

       //   string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId; // retrieve the role id 

            RoleManagmentVM RoleVM = new RoleManagmentVM() 
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeproperties: "Company"),

                RoleList = _roleManager.Roles.Select(i => new SelectListItem   // Populate the dropdown for row list
              {
                  Text = i.Name,
                  Value = i.Name
              }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem // Populate the dropdown for row list
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId))
                    .GetAwaiter().GetResult().FirstOrDefault();  // that will retrieve role of the logged in user -- that will give us a roll of the user id 
            return View(RoleVM);
         }



        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            // that way we will retrieve old rule of the user
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id))
                     .GetAwaiter().GetResult().FirstOrDefault(); 

            // retrieve the appliaction user
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);


            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                //a role was updated
              //  ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.ApplicationUser.Id);
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;  //remove
                }

                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
            else
            {
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }




        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserlist = _unitOfWork.ApplicationUser.GetAll(includeproperties: "Company").ToList();

         //   var userRoles = _db.UserRoles.ToList(); // (display roles in manage user ) that way we will have the mapping table
          //  var roles = _db.Roles.ToList(); // get roles table from database

            foreach (var user in objUserlist)
            {
                //  var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; // based on user id we can find the role id from the userrole table


                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            
            
            
            return Json(new { data = objUserlist });
        }


        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {

            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }


            objFromDb.LockoutEnabled  = !objFromDb.LockoutEnabled;

            if (!objFromDb.LockoutEnabled)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                // if the user is alrady unlocked we want to lock them
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            //if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            //{
            //    //user is currently locked and we need to unlock them
            //    objFromDb.LockoutEnd = DateTime.Now;
            //}
            //else
            //{
            //    // if the user is alrady unlocked we want to lock them
            //    objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            //}
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion

    }
}

