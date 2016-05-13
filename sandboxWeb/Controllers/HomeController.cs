using sandboxWeb.Helpers.XML.Exchange;
using sandboxWeb.Models;
using sandboxWeb.EF;
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
            omproEntities db = new omproEntities();

            List<EF.Team> teams = db.Teams.ToList();
            List<TeamsNotFound> newTeams = db.TeamsNotFounds.ToList();

            List<EF.Competition> comps = db.Competitions.ToList();
            List<CompetitionsNotFound> newComps = db.CompetitionsNotFounds.ToList();

            Betfair betfair = new Betfair(teams, newTeams, comps, newComps);
            betfair.Login();
            
            betfair.ReadBetfairFootball("GBR");


            betfair.RefreshDB();
            betfair.NotFoundToDB();

            return View();
        }

       
    }
}