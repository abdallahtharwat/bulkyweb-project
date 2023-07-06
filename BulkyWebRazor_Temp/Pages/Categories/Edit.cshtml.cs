using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]

    public class EditModel : PageModel
    {
        private readonly ApplicationDbcontext _db;

        public Category Category { get; set; }
        public EditModel(ApplicationDbcontext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            if (id  != null || id != 0)
            {
                Category = _db.Categories.Find(id);
            }

        }

        public IActionResult OnPost(int? id)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(Category);
                _db.SaveChanges();
                TempData["success"] = "category  update successfully";
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
