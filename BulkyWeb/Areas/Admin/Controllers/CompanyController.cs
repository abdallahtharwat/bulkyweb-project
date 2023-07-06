using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
      // authorize for route 
    public class CompanyController : Controller
    {
        private readonly IUnitofWork _UnitofWork;
      
        public CompanyController(IUnitofWork UnitofWork)
        {

            _UnitofWork = UnitofWork;
           
        }

        public IActionResult Index()
        {
            List<Company> objCompanylist = _UnitofWork.Company.GetAll().ToList();
            
            return View(objCompanylist);
        }

        [Authorize(Roles = SD.Role_Admin)]
        // get create
        public IActionResult Upsert(int? id)
        {
                                                                                                                                       //ViewBag.Categorylist = Categorylist;    //ViewData["Categorylist"] = Categorylist; 
          
           if( id == null || id == 0)
            {
                // Create
                return View(new Company());
            }
            else
            {
                // Update
                Company Companyobj = _UnitofWork.Company.Get(u => u.Id == id);
                return View(Companyobj);
            }

        }

        // post create
        [HttpPost]
        public IActionResult Upsert(Company Companyobj)
        {
            //Validation for sliver side
            if (ModelState.IsValid)
            {
                     // create and update
                if (Companyobj.Id == 0)
                {
                    _UnitofWork.Company.add(Companyobj);
                    TempData["success"] = "  Created successfully";
                }
                else
                {
                    _UnitofWork.Company.Update(Companyobj);
                    TempData["success"] = "  update successfully";
                }
                _UnitofWork.Save();
                return RedirectToAction("Index");
            }

            else
            {
               
                return View(Companyobj);
            }
          
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanylist = _UnitofWork.Company.GetAll().ToList();
            return Json(new { data = objCompanylist });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var Companytobedelete = _UnitofWork.Company.Get(u => u.Id == id);
            if (Companytobedelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }


            _UnitofWork.Company.Remove(Companytobedelete);
            _UnitofWork.Save();

            return Json(new { success = false, message = "Delete Successful" });
        }


        #endregion

    }
}

