using sandboxWeb.EF;
using sandboxWeb.Helpers.Maintenance;
using sandboxWeb.Misc;
using sandboxWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sandboxWeb.Helpers.XML.Exchange
{
    public class Betdaq : Company
    {
        public List<string> BetdaqMatchPhrases;
        public Betdaq(List<sandboxWeb.EF.Team> teams, List<sandboxWeb.EF.TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        {
            // todo - need to get all relevant leagues/internationals and put in leagues for the next few years.
            this.BetdaqMatchPhrases = new List<string>() { "Serie A 2015/2016", "La Liga 2015/16", "Serie A 2016/2017", "La Liga 2016/17", "Champions League Matches", "Europa League Matches", "The Championship", "Premier League", "English League One", "English League Two", "Scottish Premiership 2015/16", "Scottish Premiership 2016/17" };
        }
        public void ReadBetdaqFootball()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://xml.betdaq.com/soccer");
            XmlNode node = doc.SelectSingleNode("root").SelectSingleNode("SPORT");
            foreach(XmlNode compNode in node.ChildNodes)
            {
                //store competition ids and names
                var competitionName = compNode.Attributes["NAME"].Value.ToString();
                CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                Models.Competition comp = new Models.Competition(competitionName, CurrentComps);

                foreach (XmlNode groupNode in compNode.ChildNodes)
                {
                    var groupName = groupNode.Attributes["NAME"].Value.ToString().Trim();
                    if (BetdaqMatchPhrases.Contains(groupName))
                    {
                        foreach(XmlNode matchNode in groupNode.ChildNodes)
                        {
                            foreach(XmlNode oddsNode in matchNode)
                            {
                                if(oddsNode.Attributes["NAME"].Value.ToString() == "Match Odds")
                                {
                                    var url = "";
                                    var moburl = "";
                                    List<Models.Team> teams = new List<Models.Team>();

                                    foreach (XmlNode oddsChildNode in oddsNode)
                                    {
                                        if (oddsChildNode.Name.ToString() == "LINK")
                                        {
                                            url = oddsChildNode.Attributes["URL"].Value.ToString();
                                            moburl = oddsChildNode.Attributes["MOBILE_URL"].Value.ToString();
                                        }
                                        else {
                                            var name = oddsChildNode.Attributes["NAME"].Value.ToString();
                                            if (name.ToUpper() != "DRAW")
                                            {
                                                TeamMaintenance.IsTeamNameRecorded(name, NewTeams, CurrentTeams);
                                                teams.Add(new Models.Team(name, CurrentTeams));
                                            }
                                        }
                                    }

                                    foreach (XmlNode oddsChildNode in oddsNode)
                                    {
                                        if (oddsChildNode.Name.ToString() != "LINK")
                                        {
                                            if (oddsChildNode.Attributes["NAME"].Value == "Draw")
                                            {
                                                foreach (XmlNode layDrawNode in oddsChildNode)
                                                {   
                                                    if (layDrawNode.Attributes["NAME"].Value.ToString() == "Lay Draw")
                                                    {
                                                        if (layDrawNode.FirstChild.FirstChild != null)
                                                        {
                                                            var match = new Models.Match()
                                                            {
                                                                Id = Convert.ToInt32(matchNode.Attributes["ID"].Value),
                                                                Name = matchNode.Attributes["NAME"].Value.ToString(),
                                                                Bookmaker = BookmakersConstants.BetdaqName,
                                                                BookmakerId = BookmakersConstants.BetdaqId,
                                                                Competition = comp,
                                                                LastUpdated = DateTime.Now,
                                                                Team1 = teams.First(),
                                                                Team2 = teams.Last(),
                                                                Bet = "Draw",
                                                                Odds = Convert.ToDecimal(layDrawNode.FirstChild?.FirstChild?.Attributes["VALUE"]?.Value),
                                                                Date = Convert.ToDateTime(matchNode.Attributes["DATE"].Value),
                                                                Time = ""
                                                            };
                                                            foreach (XmlNode moneyNode in layDrawNode.FirstChild?.FirstChild?.ChildNodes)
                                                            {
                                                                if (moneyNode.Attributes["CURRENCY"].Value == "GBP")
                                                                    match.MoneyInMarket = Convert.ToDecimal(moneyNode.Attributes["VALUE"].Value);
                                                            }
                                                            Matches.Add(match);
                                                        }

                                                    }
                                                }

                                            }
                                            else
                                            {
                                                foreach (XmlNode teamNode in oddsChildNode)
                                                {
                                                    if (teamNode.FirstChild.FirstChild != null)
                                                    { 
                                                        Models.Match match = new Models.Match()
                                                        {
                                                            Id = Convert.ToInt32(matchNode.Attributes["ID"].Value),
                                                            Name = matchNode.Attributes["NAME"].Value.ToString(),
                                                            Bookmaker = BookmakersConstants.BetdaqName,
                                                            BookmakerId = BookmakersConstants.BetdaqId,
                                                            Competition = comp,
                                                            LastUpdated = DateTime.Now,
                                                            Team1 = teams.First(),
                                                            Team2 = teams.Last(),
                                                            Date = Convert.ToDateTime(matchNode.Attributes["DATE"].Value),
                                                            Time = ""
                                                        };

                                                        // todo the below errors sometimes - need to put a check in that the firstchild.firstchild node is there
                                                        foreach (XmlNode moneyNode in teamNode.FirstChild?.FirstChild?.ChildNodes)
                                                        {
                                                            if (moneyNode.Attributes["CURRENCY"].Value == "GBP")
                                                                match.MoneyInMarket = Convert.ToDecimal(moneyNode.Attributes["VALUE"].Value);
                                                        }
                                                        if (teamNode.Attributes["NAME"].Value.ToString() == "Lay " + teams.First().Name)
                                                        {
                                                            match.Bet = teams.First().Name;
                                                            match.Odds = Convert.ToDecimal(teamNode.FirstChild?.FirstChild?.Attributes["VALUE"]?.Value);
                                                        }
                                                        else if (teamNode.Attributes["NAME"].Value.ToString() == "Lay " + teams.Last().Name)
                                                        {
                                                            match.Bet = teams.Last().Name;
                                                            match.Odds = Convert.ToDecimal(teamNode.FirstChild?.FirstChild?.Attributes["VALUE"]?.Value);
                                                        }
                                                        if (match.Bet != null)
                                                            Matches.Add(match);
                                                    }
                                                }

                                            }
                                          }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                
            }


        public void ReadBetdaqHorseRacing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://xml.betdaq.com/horseracing");

            XmlNodeList xnList = doc.SelectNodes("/root/SPORT/EVENT");

            foreach (XmlNode node in xnList)
            {
                if (node.Attributes["NAME"].Value.ToString().ToUpper() != "ANTE POST")
                {
                    foreach (XmlNode eventNode in node.ChildNodes)
                    {
                        var compName = GetMeetNameWH(eventNode.Attributes["NAME"].Value.ToString());
                        CompetitionMaintenance.IsCompetitionRecorded(compName, NewComps, CurrentComps);
                        Models.Competition comp = new Models.Competition(compName, CurrentComps);
                        foreach (XmlNode raceNode in eventNode.ChildNodes)
                        {
                            foreach (XmlNode marketNode in raceNode.ChildNodes)
                            {
                                if (marketNode.Attributes["NAME"].Value.ToString().ToUpper() == "WIN MARKET")
                                {
                                    string link = "";
                                    string mobLink = "";
                                    foreach (XmlNode linkNode in marketNode.ChildNodes)
                                    {
                                        if (linkNode.Name == "LINK")
                                        {
                                            link = linkNode.Attributes["URL"].Value.ToString();
                                            mobLink = linkNode.Attributes["MOBILE_URL"].Value.ToString();
                                        }
                                    }
                                    foreach (XmlNode selectionNode in marketNode.ChildNodes)
                                    {
                                        if (selectionNode.Name != "LINK")
                                        { 
                                            var name = selectionNode.Attributes["NAME"].Value.ToString();
                                            name = name.Substring((name.IndexOf(' ') + 1));
                                            name = name.Substring((name.IndexOf(' ') +1 ));
                                            
                                            XmlNode oddsNode = selectionNode.SelectNodes("OUTCOME/ODDS[@POLARITY='LAY']").Item(0).FirstChild;
                                            decimal moneyInMarket = 0;
                                            if (oddsNode != null)
                                            {
                                                foreach (XmlNode moneyNode in oddsNode.ChildNodes)
                                                {
                                                    if (moneyNode.Attributes["CURRENCY"].Value.ToString() == "GBP")
                                                    {
                                                        moneyInMarket =
                                                            Convert.ToDecimal(moneyNode.Attributes["VALUE"].Value);
                                                    }
                                                }
                                            }
                                            Races.Add(new Models.Race()
                                            {
                                                Id = Convert.ToInt32(selectionNode.Attributes["ID"].Value),
                                                Name = raceNode.Attributes["NAME"].Value.ToString(),
                                                Bookmaker = BookmakersConstants.BetdaqName,
                                                BookmakerId = BookmakersConstants.BetdaqId,
                                                Meeting = comp,
                                                Horse = name,
                                                Odds = (oddsNode == null) ? Convert.ToDecimal(0) : Convert.ToDecimal(oddsNode.Attributes["VALUE"].Value),
                                                Date = Convert.ToDateTime(raceNode.Attributes["DATE"].Value),
                                                Time = raceNode.Attributes["NAME"].Value.ToString().Substring(0, 5),
                                                LastUpdated = DateTime.Now,
                                                Url = link,
                                                MoneyInMarket = moneyInMarket
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public string GetMeetNameWH(string name)
        {
            int l = name.IndexOf("(");
            if (l > 0)
            {
                return name.Substring(0, l);
            }
            return name;
        }



    }
}
