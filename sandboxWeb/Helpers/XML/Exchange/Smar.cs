using sandboxWeb.EF;
using sandboxWeb.Helpers.DataManipulation;
using sandboxWeb.Helpers.Maintenance;
using sandboxWeb.Misc;
using sandboxWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sandboxWeb.Helpers.XML.Exchange
{
    public class Smar : Company
    {
        public Smar(List<sandboxWeb.EF.Team> teams, List<sandboxWeb.EF.TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        {
        }
        public void ReadSmarUKFootball()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                byte[] data = client.DownloadData("http://odds.smarkets.com/oddsfeed.xml.gz ");
                byte[] decompress = FileManipulation.Decompress(data);

                // doc.Load("http://odds.smarkets.com/oddsfeed.xml");
                XmlDocument doc = new XmlDocument();
                MemoryStream ms = new MemoryStream(decompress);
                doc.Load(ms);
                AllLogic(doc);
            }
        }

        public void AllLogic(XmlDocument doc)
        {
            foreach(XmlNode node in doc.SelectSingleNode("odds").ChildNodes)
            {
                if(node.Attributes["type"].Value.ToString().ToUpper() == "FOOTBALL MATCH")
                {
                    var competitionName = node.Attributes["parent"].Value.ToString();
                    CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                    Models.Competition comp = new Models.Competition(competitionName, CurrentComps);
                    
                    foreach (XmlNode matchNode in node.ChildNodes)
                    {
                        
                        if (matchNode.Attributes["slug"].Value.ToString() == "winner")
                        {
                            List<Models.Team> teamsForMatchFields = new List<Models.Team>();
                            foreach (XmlNode contractNode in matchNode.ChildNodes)
                            {
                                var teamname = contractNode.Attributes["name"].Value.ToString();
                                if (teamname.ToLower() != "draw")
                                {
                                    TeamMaintenance.IsTeamNameRecorded(teamname, NewTeams, CurrentTeams);
                                    var team = new Models.Team(teamname, CurrentTeams);
                                    teamsForMatchFields.Add(team);
                                }
                            }
                            foreach (XmlNode contractNode in matchNode.ChildNodes)
                            {
                                XmlNode bidsNode = contractNode.SelectSingleNode("bids").FirstChild;
                                if (bidsNode != null && teamsForMatchFields.Count() > 1)
                                {
                                    var match = new Models.Match()
                                    {
                                        Id = Convert.ToInt32(node.Attributes["id"].Value),
                                        Name = node.Attributes["name"].Value.ToString(),
                                        Bookmaker = BookmakersConstants.SmarketsName,
                                        BookmakerId = BookmakersConstants.SmarketsId,
                                        Competition = comp,
                                        LastUpdated = DateTime.Now,
                                        Team1 = teamsForMatchFields.First(),
                                        Team2 = teamsForMatchFields.Last(),
                                        Date = Convert.ToDateTime(node.Attributes["date"].Value),
                                        Time = node.Attributes["time"].Value.ToString(),
                                        Url = node.Attributes["url"].Value.ToString(),
                                        Bet = contractNode.Attributes["name"].Value.ToString(),
                                        Odds = Convert.ToDecimal(bidsNode.Attributes["decimal"].Value),
                                        MoneyInMarket = Convert.ToDecimal(bidsNode.Attributes["backers_stake"].Value)
                                    };
                                    Matches.Add(match);
                                }
                                    
                            }
                        }
                    }
                }
                if (node.Attributes["type"].Value.ToString().ToUpper() == "HORSE RACING RACE")
                {
                    var competitionName = node.Attributes["parent"].Value.ToString();
                    CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                    Models.Competition comp = new Models.Competition(competitionName, CurrentComps);
                    foreach (XmlNode marketNode in node.ChildNodes)
                    {
                        if (marketNode.Attributes["slug"].Value.ToString() == "to-win")
                        {
                            foreach (XmlNode contractNode in marketNode.ChildNodes)
                            {
                                XmlNode bidsNode = contractNode.SelectSingleNode("bids").FirstChild;
                                if (bidsNode != null)
                                {
                                    var race = new Models.Race()
                                    {
                                        Id = Convert.ToInt32(node.Attributes["id"].Value),
                                        Name = node.Attributes["name"].Value.ToString() + " " + comp.Name,
                                        Bookmaker = BookmakersConstants.SmarketsName,
                                        BookmakerId = BookmakersConstants.SmarketsId,
                                        Horse = contractNode.Attributes["name"].Value.ToString(),
                                        Meeting = comp,
                                        LastUpdated = DateTime.Now,
                                        Date = Convert.ToDateTime(node.Attributes["date"].Value),
                                        Time = node.Attributes["time"].Value.ToString(),
                                        Url = node.Attributes["url"].Value.ToString(),
                                        Odds = Convert.ToDecimal(bidsNode.Attributes["decimal"].Value),
                                        MoneyInMarket = Convert.ToDecimal(bidsNode.Attributes["backers_stake"].Value)
                                    };
                                    Races.Add(race);
                                }

                            }
                        }
                    }
                }
            }
        }







    }
}
