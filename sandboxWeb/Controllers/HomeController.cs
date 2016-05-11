using sandboxConsole.Helpers.XML.Exchange;
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace sandboxWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Betfair betfair = new Betfair();
            betfair.Login();
            betfair.GetCompetitions();
            betfair.GetEvents();
            betfair.GetMarkets();
            betfair.GetMarketBooks();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}