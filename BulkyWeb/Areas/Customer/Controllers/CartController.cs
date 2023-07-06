using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.ComponentModel;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        private readonly IUnitofWork _unitofWork;
        public CartController(IUnitofWork unitofWork )
        {
            _unitofWork = unitofWork;

        }
     
        //get product in shoppingcart
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;               // when we push cart button we need Userid 
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()            // get shoppingcartVM Model from database
            {
                ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "Product"),
                OrderHeader = new()
            };

           IEnumerable<ProductImage> productImages = _unitofWork.ProductImage.GetAll();  // product images in shoppingcart will be populated


            // count ( Calculating the order total )    we have to repetition each one of the cart 
            foreach (var Cart in ShoppingCartVM.ShoppingCartList)
            {
                Cart.Product.productImages = productImages.Where(u => u.ProductId == Cart.Product.Id).ToList();  // product images in shoppingcart will be populated
                Cart.price = GetPriceBasedOnQuantity(Cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (Cart.price * Cart.Count);
            }

            return View(ShoppingCartVM);
        }


        // how to deal with many price for product in count
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }

                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }



        // button plus
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);   // get shoppingcat from database based on id
            cartFromDb.Count += 1;    // update the count
            _unitofWork.ShoppingCart.Update(cartFromDb);
            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }
        // button Minus
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                _unitofWork.ShoppingCart.Remove(cartFromDb);

                HttpContext.Session.SetInt32(SD.SessionCart, _unitofWork.ShoppingCart   // when we remove product from shopping cart hysam3 fe shopping cart icon
                  .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitofWork.ShoppingCart.Update(cartFromDb);
            }
            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }

             // button Remove
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitofWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitofWork.ShoppingCart.Remove(cartFromDb);

            HttpContext.Session.SetInt32(SD.SessionCart, _unitofWork.ShoppingCart  // when we remove product from shopping cart hysam3 fe shopping cart icon
          .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }


        
        // get
        public IActionResult Summary()
        { 
            var claimsIdentity = (ClaimsIdentity)User.Identity;               // when we push cart button we need Userid 
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()            // get shoppingcartVM Model from database
            {
                ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "Product"),
                OrderHeader = new()
            };
            // applicationuser that we want populate and retrieve it 
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId);
            // manually update all properties in orderheader == applicationUser lik name, street ,city, postal code
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            // count ( Calculating the order total )   we have to repetition each one of the cart  
            foreach (var Cart in ShoppingCartVM.ShoppingCartList)
            {
                Cart.price = GetPriceBasedOnQuantity(Cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (Cart.price * Cart.Count);
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;               // when we push cart button we need Userid 
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // get Populating ShoppingCartList Model from database
            ShoppingCartVM.ShoppingCartList = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;   // get|| populate orderdate to be datetime now
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;  // userid that we retreive in line 146 (from website) == applicationuserid from database

            // applicationuser that we want populate and retrieve it  
            ApplicationUser applicationUser = _unitofWork.ApplicationUser.Get(u => u.Id == userId);

            // count ( Calculating the order total )   we have to repetition each one of the cart  
            foreach (var Cart in ShoppingCartVM.ShoppingCartList)
            {
                Cart.price = GetPriceBasedOnQuantity(Cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (Cart.price * Cart.Count);
            }

            // the payment is diffent btween CompanyUser and regular User
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is a company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitofWork.OrderHeader.add(ShoppingCartVM.OrderHeader);  // we create orderHeader
            _unitofWork.Save();


            // create order Details     
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.price,
                    Count = cart.Count
                };
                _unitofWork.OrderDetail.add(orderDetail);
                _unitofWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic

                var domain = "https://localhost:44333/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderComfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),     
                    Mode = "payment",
                };
                                              //get all  product in cash page 
                foreach (var item in ShoppingCartVM.ShoppingCartList) //get all item | retrive ShoppingCartList
                {
                    // all that for one item
                    var sessionLineItem = new SessionLineItemOptions   
                    {
                        PriceData = new SessionLineItemPriceDataOptions // generating and configure PriceData 
                        {
                            UnitAmount = (long)(item.price * 100), // $20.50 => 2050   // configure UnitAmount 
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title   // we can add here images or name or description
                            }
                        },
                        Quantity = item.Count
                    };
                    // (add al items in LineItems)  if there are 10 item in shoppingcart it will retrive and add all of those 10 items
                    options.LineItems.Add(sessionLineItem);  
                }

                var service = new SessionService();
               Session session =  service.Create(options);   // we create a session here we can expect a response back from the server
                 // when we get response of session we have inside both Id and paymentintentId and  we need update them 
                _unitofWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitofWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderComfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

          // how we will know tle payment succefull or not
        public IActionResult OrderComfirmation(int id)
        {
            OrderHeader orderHeader = _unitofWork.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser"); // retreve order header based om id
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer
                var service = new SessionService();      // build class in stripe package
                Session session = service.Get(orderHeader.SessionId); //retrive a stripe session

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId); //update payment intent id
                    _unitofWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved); // update ordr status
                    _unitofWork.Save();
                }
                    HttpContext.Session.Clear(); // when the stripe succse it will make shoppingcart ViewComponent = 0 
            }
            // remove shopping cart and make it emty
            List<ShoppingCart> shoppingCarts = _unitofWork.ShoppingCart
              .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitofWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitofWork.Save();

            return View(id);
        }















    }
     }
