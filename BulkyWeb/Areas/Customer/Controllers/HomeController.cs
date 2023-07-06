
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _UnitofWork;
        public HomeController(ILogger<HomeController> logger , IUnitofWork UnitofWork)
        {
            _logger = logger;
            _UnitofWork = UnitofWork;
        }
        
        public IActionResult Index()
        {
            IEnumerable<Product> Productlist = _UnitofWork.Product.GetAll(includeproperties: "Category,productImages");
            return View(Productlist);
        }

        // get 
        public IActionResult Details(int productid)
        {
             //get product and productid and count from database to pass to view
            ShoppingCart cart = new()
            {
                Product = _UnitofWork.Product.Get(u => u.Id == productid, includeproperties: "Category,productImages"),
                Count = 1,
                ProductId = productid
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]  // when we add a item in cart we must be login first
        public IActionResult Details(ShoppingCart shoppingCart)
        {
                 // when we push cart button we need Userid and productid
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            // we must do that because we donot need to duplicate product when we add  a new item and if user has already have a product on shoppingcart
            // get shopping cart from database and make sure the userid == ApplicationUserId and produc id == productid
            ShoppingCart cartFromDb = _UnitofWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
           u.ProductId == shoppingCart.ProductId);
            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _UnitofWork.ShoppingCart.Update(cartFromDb);
                _UnitofWork.Save();
            }
            else
            {
                //add cart record
                _UnitofWork.ShoppingCart.add(shoppingCart);
                _UnitofWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,    // how to makr shoppingcart icon to count
                    _UnitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());

            }

            TempData["success"] = "Cart updated successfully";
          
            return RedirectToAction(nameof(Index));
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}