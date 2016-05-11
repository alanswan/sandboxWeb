using sandboxWeb.EF;
using sandboxConsole.Helpers.Maintenance;
using sandboxConsole.Misc;
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sandboxConsole.Helpers.XML
{
    public class Coral : Company
    {
        public Coral(List<sandboxWeb.EF.Team> teams, List<TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        {}

        public void ReadCoralFootball()
        {
            // Read UK Football
            ReadFootball("http://xmlfeeds.coral.co.uk/oxi/pub?template=getCouponDetails&coupon=17");

            // Read Europe Football
            ReadFootball("http://xmlfeeds.coral.co.uk/oxi/pub?template=getCouponDetails&coupon=202");
        }

        public void ReadFootball(string feedUrl)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(feedUrl);
            var test = doc;

            XmlNodeList xnList = doc.SelectNodes("/oxip/response/coupon/event");

            foreach (XmlNode eventNode in xnList)
            {
               var eventName = eventNode.Attributes["name"].Value.ToString();
               var url = eventNode.Attributes["url"].Value.ToString();
               var competitionName = eventNode.Attributes["typeName"].Value.ToString();
               var date = Convert.ToDateTime(eventNode.Attributes["date"].Value);
               var time = eventNode.Attributes["time"].Value.ToString().Substring(0,5);

               CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
               Models.Competition comp = new Models.Competition(competitionName, CurrentComps);

                XmlNodeList outcomeList = eventNode.SelectNodes("market/outcome");

                List<Models.Team> teamsForMatchFields = new List<Models.Team>();
                foreach (XmlNode outcomeNode in outcomeList)
                {
                   var teamname = outcomeNode.Attributes["name"].Value.ToString();
                    if (teamname.ToLower() != "draw")
                    {
                        TeamMaintenance.IsTeamNameRecorded(teamname, NewTeams, CurrentTeams);
                        var team = new Models.Team(teamname, CurrentTeams);
                        teamsForMatchFields.Add(team);
                    }
                }
                foreach (XmlNode outcomeNode in outcomeList)
                {
                    var match = new Models.Match()
                    {
                        Id = Convert.ToInt32(outcomeNode.Attributes["id"].Value),
                        Name = eventName,
                        Bookmaker = BookmakersConstants.CoralName,
                        BookmakerId = BookmakersConstants.CoralId,
                        Competition = comp,
                        LastUpdated = Convert.ToDateTime(outcomeNode.Attributes["lastUpdateDate"].Value),
                        Team1 = teamsForMatchFields.First(),
                        Team2 = teamsForMatchFields.Last(),
                        Date = date,
                        Time = time,
                        Bet = outcomeNode.Attributes["name"].Value.ToString(),
                        Odds = Convert.ToDecimal(outcomeNode.Attributes["oddsDecimal"].Value),
                        Url = url
                    };
                    Matches.Add(match);
                }
            }
        }

        public void ReadHorseRacing()
        {
            //return events files
            XmlDocument doc = new XmlDocument();
            doc.Load("http://xmlfeeds.coral.co.uk/oxi/pub?template=getEventsByClass&class=223");
            XmlNodeList xnList = doc.SelectNodes("/oxip/response/class/type/event");
            foreach (XmlNode eventNode in xnList)
            {
                var id = eventNode.Attributes["id"].Value.ToString();
                var url = ("http://xmlfeeds.coral.co.uk/oxi/pub?template=getEventDetails&event=" + id);

                var competitionName = eventNode.ParentNode.Attributes["name"].Value.ToString();
                CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                Models.Competition comp = new Models.Competition(competitionName, CurrentComps);
                ReadHorseRacingEvent(url, comp);
            }
        }

        public void ReadHorseRacingEvent(string url, Models.Competition comp)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(url);
            XmlNodeList xnList = doc.SelectNodes("/oxip/response/event/market[@name = 'Win or Each Way']/outcome");

            foreach (XmlNode outcomeNode in xnList)
            {
                if (outcomeNode.Attributes["oddsDecimal"].Value.ToString() != "SP")
                {
                    var test = outcomeNode;
                    Models.Race race = new Models.Race()
                    {
                        Id = Convert.ToInt32(outcomeNode.Attributes["id"].Value),
                        Name = outcomeNode.ParentNode.ParentNode.Attributes["name"].Value.ToString(),
                        BookmakerId = BookmakersConstants.CoralId,
                        Bookmaker = BookmakersConstants.CoralName,
                        Meeting = comp,
                        Horse = outcomeNode.Attributes["name"].Value.ToString().Trim(),
                        Odds = Convert.ToDecimal(outcomeNode.Attributes["oddsDecimal"].Value),
                        Date = Convert.ToDateTime(outcomeNode.ParentNode.ParentNode.Attributes["date"].Value),
                        Time = outcomeNode.ParentNode.ParentNode.Attributes["time"].Value.ToString().Substring(0, 5),
                        LastUpdated = DateTime.Now,
                        Url = outcomeNode.ParentNode.ParentNode.Attributes["url"].Value.ToString()
                    };
                    Races.Add(race);
                }
            }
        }

    }
}
