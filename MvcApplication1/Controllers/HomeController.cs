using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.ViewModels;

namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            PaypalViewModel model = new PaypalViewModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(PaypalViewModel model)
        {
            TempData["paypalviewmodel"] = model;
            if (model.IsPayPalPayment == true)
                return RedirectToAction("PaypalPayment", "Paypal");
            else
                return RedirectToAction("CardPayment", "Paypal");
                
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
