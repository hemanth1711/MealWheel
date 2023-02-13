﻿using MealWheel.Areas.Identity.Data;
using MealWheel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Diagnostics;

namespace MealWheel.Controllers
{
    public class HomeController : Controller
    {
        public const string raz_key = "rzp_test_cfElqRcKNLh6aA";
        public const string raz_secret = "3QaoZiwflNJbQftFonT55elT";
        private readonly ILogger<HomeController> _logger;
        public MealDbContext _meal;
        public UserManager<ApplicationUser> UserManager;
        public SignInManager<ApplicationUser> SignInManager;

        public HomeController(ILogger<HomeController> logger, MealDbContext meal)
        {
            _logger = logger;
            _meal = meal;
        }

        public IActionResult Index(int? i,string search)
        {
            
            bool val = HttpContext.User.Identity.IsAuthenticated;
           
            if (search!=null)
            {
                List<Food_Products> d = _meal.Food_Products.Where(e => e.Name.Contains(search)).ToList();
                if (val)
                {
                    string us = HttpContext.User.Identity.Name.ToString();
                    List<int> ids = _meal.favorites.Where(e => e.uname == us).Select(e => e.pid).ToList();
                    string co = _meal.carts.Where(e => e.uname == us).Count().ToString();
                    List<Food_Products> x = _meal.Food_Products.Where(e => e.Name.Contains(search)).ToList();
                    ViewData["hello"] = co;
                    //List<Food_Products> p1 = _meal.Food_Products.Where(e => e.Name.Contains(search)).Where(e=>id.Any())).ToList();
                    foreach(Food_Products food in x)
                    {
                        food.fav = false;
                        foreach(int a in ids)
                        {
                            if(food.Id==a)
                            {
                                food.fav = true;
                                break;
                            }
                        }
                    }
                    
                    return View(x);
                }
                foreach(Food_Products food in d)
                {
                    food.fav=false;
                }

                return View(d);
            }
            if (val)
            {
                string us = HttpContext.User.Identity.Name.ToString();
                string co = _meal.carts.Where(e=>e.uname==us).Count().ToString();
                List<int> ids = _meal.favorites.Where(e => e.uname == us).Select(e => e.pid).ToList();
                List<Food_Products> r = _meal.Food_Products.ToList();
                foreach (Food_Products food in r)
                {
                    food.fav = false;
                    foreach (int a in ids)
                    {
                        if (food.Id == a)
                        {
                            food.fav = true;
                            break;
                        }
                    }
                }
                ViewData["hello"] = co;
                return View(r);
            }
            List<Food_Products> p = _meal.Food_Products.ToList();
            foreach (Food_Products food in p)
            {
                food.fav = false;
            }
            return View(p);
        }

        [Authorize]
        public IActionResult add_fav(int id,bool fav)
        {
            Food_Products f = _meal.Food_Products.ToList().FirstOrDefault(e => e.Id == id);
            if(fav==true)
            {
                favorite fa = _meal.favorites.ToList().FirstOrDefault(e => e.pid == f.Id);
                _meal.favorites.Remove(fa);
                _meal.SaveChanges();
                f.fav = false;
            }
            else
            {
                favorite fa = new favorite();
                fa.uname = HttpContext.User.Identity.Name.ToString();
                fa.product = f;
                _meal.favorites.Add(fa);
                _meal.SaveChanges();
                f.fav = true;
            }
            _meal.Food_Products.Update(f);
            _meal.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            return View(_meal.Food_Products.ToList().FirstOrDefault(e=>e.Id==id));
        }
        [HttpPost]
        public IActionResult Details(Food_Products p)
        {
            Cart cart = new Cart();
            cart.pid = p.Id;
            cart.uname = HttpContext.User.Identity.Name.ToString();
            cart.totalPrice = p.quantity * p.Price;
            _meal.carts.Add(cart);
            _meal.SaveChanges();
            return RedirectToAction(nameof(cart));
        }
        [Authorize]
        public IActionResult cart()
        {
            string us = HttpContext.User.Identity.Name.ToString();
            var cati = _meal.carts.Include(c => c.product).Where(p => p.uname == us).ToList();
            ViewData["hello"] = _meal.carts.Where(p => p.uname == us).Sum(c => c.totalPrice).ToString();
            return View(cati);
        }
        [Authorize]
        public IActionResult Remove_cart(int? id)
        {
            var cartss = _meal.carts.FirstOrDefault(c => c.id == id);
            _meal.Remove(cartss);
            _meal.SaveChanges();
            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        public IActionResult Fav()
        {
            string us = HttpContext.User.Identity.Name.ToString();
            var favr = _meal.favorites.Include(c => c.product).Where(p => p.uname == us).ToList();
            return View(favr);
        }
        public IActionResult Remove_Fav(int id)
        {
            favorite f = _meal.favorites.Where(e => e.id == id).FirstOrDefault();
            _meal.favorites.Remove(f);
            _meal.SaveChanges();
            return RedirectToAction(nameof(Fav));
        }
        [Authorize]
        public IActionResult payment(int? id)
        {
            var products = _meal.Food_Products.Include(c => c.category).FirstOrDefault(p => p.Id == id);
            var oid = Createorder(products);
            payOptions pay = new payOptions()
            {
                key = raz_key,
                Amountinsub = products.Price * 100,
                currency = "INR",
                name = "Global Logic MeanWheel",
                Description = "Good dishes for Global Logic Employes",
                ImageUrl = "",
                Orderid = oid,
                productid = products.Id

            };
            return View(pay);

        }

        //public IActionResult gotoaddress(int id)
        //{
        //    var products = _meal.Food_Products.Include(c => c.category).FirstOrDefault(p => p.Id == id);
        //    var unam = HttpContext.User.Identity.Name;
        //    Address user = _meal.addresses.FirstOrDefault(e => e.uname == unam);
        //    if (user == null)
        //    {
        //        return RedirectToAction(nameof(createAddress));
        //    }
        //    if (user != null)
        //    {
                
        //    }
        //    //ViewData["hello"] = products.Id;
        //    return View(user);
        //}
        //public IActionResult createAddress()
        //{
        //    return View();
        //}
        public IActionResult selecttype(int id,int qty)
        {
            var products = _meal.Food_Products.Include(c => c.category).FirstOrDefault(p => p.Id == id);
            products.Price = qty * products.Price;
            return View(products);
        }

        public IActionResult Failure()
        {
            return View();
        }
        public IActionResult Success(payOptions pay)
        {
            Billing s = new Billing();
            s.Orderplaced = true;
            var products = _meal.Food_Products.FirstOrDefault(p => p.Id == pay.productid);
            s.quantity = products.quantity;
            s.pid = pay.productid;
            s.dateOrdered = DateTime.Now;
            _meal.billings.Add(s);
            _meal.SaveChanges();
            var bill = _meal.billings.FirstOrDefault(c => c.dateOrdered == s.dateOrdered);
            MyOrder o = new MyOrder();
            o.bid = bill.id;
            o.uname = HttpContext.User.Identity.Name;
            _meal.myOrders.Add(o);
            _meal.SaveChanges();
            return View();
        }

        [Authorize]
        public IActionResult Orders()
        {
            var username = HttpContext.User.Identity.Name;
            var orders = _meal.myOrders.Include(e=>e.bill.product).Include(e=>e.bill).Where(e=>e.uname==username);
            return View(orders);
        }
        public String Createorder(Food_Products products)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(raz_key, raz_secret);
                Dictionary<string, object> input = new Dictionary<string, object>();
                input.Add("amount", products.Price * 100);
                input.Add("currency", "INR");
                input.Add("receipt", "12121");

                Order ord_Res = client.Order.Create(input);
                var oid = ord_Res.Attributes["id"].ToString();
                return oid;

            }
            catch
            {
                return null;
            }
        }

        public IActionResult feedback(int id)
        {
            Billing b = _meal.billings.Include(e=>e.product).FirstOrDefault(e => e.id == id);
            string unam = HttpContext.User.Identity.Name;
            feedback f = _meal.feedbacks.Where(e => e.uname == unam).Where(e => e.pid == b.pid).Include(e=>e.product).FirstOrDefault();
            if(f==null)
            {
                feedback f1 = new feedback();
                f1.uname = unam;
                f1.pid = b.pid;
                f1.rating = 0;
                f1.review = "";
                _meal.feedbacks.Add(f1);
                _meal.SaveChanges();
                return View(f1);
            }
            return View(f);
        }

        [HttpPost]
        public IActionResult feedback(feedback feedback)
        {
            feedback f1 = new feedback();
            f1.id = feedback.id;
            f1.uname = feedback.uname;
            f1.pid = feedback.pid;
            f1.review=feedback.review;
            f1.rating = feedback.rating;
            _meal.feedbacks.Update(f1);
            _meal.SaveChanges();
            int total_rat = _meal.feedbacks.Where(e => e.pid == f1.pid).Sum(e => e.rating);
            int total_user = _meal.feedbacks.Where(e => e.pid == f1.pid).Count();
            int total_rating = (int)Math.Round((float)(total_rat / total_user));
            Food_Products f2 = _meal.Food_Products.FirstOrDefault(e => e.Id == f1.pid);
            f2.rating = total_rating;
            _meal.Food_Products.Update(f2);
            _meal.SaveChanges();
            return View(f1);

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