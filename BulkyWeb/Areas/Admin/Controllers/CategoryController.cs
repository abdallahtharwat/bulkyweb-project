
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
          // authorize for route 
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _UnitofWork;
        public CategoryController(IUnitofWork UnitofWork)
        {

            _UnitofWork = UnitofWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategorylist = _UnitofWork.Category.GetAll().ToList();
         
            return View(objCategorylist);
        }

        [Authorize(Roles = SD.Role_Admin)]
        // get create
        public IActionResult Create()
        {

            return View();
        }
        // post create
        [HttpPost]
        public IActionResult Create(Category obj)
        {

            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("DisplayOrder", "the Display order can not exactly match the name");
            }

            //Validation for sliver side
            if (ModelState.IsValid)
            {
                _UnitofWork.Category.add(obj);
                _UnitofWork.Save();
                TempData["success"] = "category  Created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [Authorize(Roles = SD.Role_Admin)]
        // get Edit
        public IActionResult Edit(int? id)
        {

            // how to create edit btton
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryfromDb = _UnitofWork.Category.Get(u => u.Id == id);
            //Category categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==Id); ;
            if (categoryfromDb == null)
            {
                return NotFound();
            }

            return View(categoryfromDb);
        }

        // post Edit
        [HttpPost]
        public IActionResult Edit(Category obj)
        {

            //Validation for sliver side
            if (ModelState.IsValid)
            {
                _UnitofWork.Category.Update(obj);
                _UnitofWork.Save();
                TempData["success"] = "category  update successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        [Authorize(Roles = SD.Role_Admin)]
        // get Delete
        public IActionResult Delete(int? id)
        {

            // how to create edit btton
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryfromDb = _UnitofWork.Category.Get(u => u.Id == id);
            //Category categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==Id); ;
            if (categoryfromDb == null)
            {
                return NotFound();
            }

            return View(categoryfromDb);
        }

        // post Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult Deletepost(int? id)
        {
            Category? obj = _UnitofWork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _UnitofWork.Category.Remove(obj);
            _UnitofWork.Save();
            TempData["success"] = "category  delete successfully";
            return RedirectToAction("Index");


        }

    }
}
