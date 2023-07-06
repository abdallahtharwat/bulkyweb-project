using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbcontext _db;
      
        public Category Category { get; set; }
        public CreateModel(ApplicationDbcontext db)
        {
            _db = db;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            _db.Categories.Add(Category);
             _db.SaveChanges();
            TempData["success"] = "category  Created successfully";
            return RedirectToPage("Index");

        }
    }
}
