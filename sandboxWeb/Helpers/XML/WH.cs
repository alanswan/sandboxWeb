using sandboxWeb.EF;
using sandboxConsole.Helpers.Maintenance;
using sandboxConsole.Misc;
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sandboxConsole.Helpers.XML
{
    public class WH : Company
    {
        public WH(List<sandboxWeb.EF.Team> teams, List<sandboxWeb.EF.TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps) :base(teams, newTeams, comps, newComps)
        {
        }
        
        public void ReadWHUKFootball()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://cachepricefeeds.williamhill.com/openbet_cdn?action=template&template=getHierarchyByMarketType&classId=1&marketSort=MR&filterBIR=N");
            FootballLogic(doc);
        }

        public void ReadWHInternationalFootball()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://cachepricefeeds.williamhill.com/openbet_cdn?action=template&template=getHierarchyByMarketType&classId=36&marketSort=MR&filterBIR=N");
            FootballLogic(doc);
        }

        public void ReadWHEuroFootball()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://cachepricefeeds.williamhill.com/openbet_cdn?action=template&template=getHierarchyByMarketType&classId=275&marketSort=MR&filterBIR=N");
            FootballLogic(doc);
        }

        public void ReadHorseRacing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("http://cachepricefeeds.williamhill.com/openbet_cdn?action=template&template=getHierarchyByMarketType&classId=2&marketSort=--&filterBIR=N");
            HorseRacingLogic(doc);
        }


        public void FootballLogic(XmlDocument doc)
        {
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "response")
                {
                    //if error thrown, don't continue - i.e no data returned
                    if(node.Attributes["code"].Value.ToString() != "100") { 
                        foreach (XmlNode responseNode in doc.DocumentElement.ChildNodes)
                        {
                            var bookNode = responseNode.SelectSingleNode("williamhill");
                            var sportNode = bookNode?.SelectSingleNode("class");

                            foreach (XmlNode compNode in sportNode.ChildNodes)
                            {
                                //store competition ids and names
                                var competitionName = compNode.Attributes["name"].Value.ToString();
                                CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                                Models.Competition comp = new Models.Competition(competitionName, CurrentComps);

                                foreach (XmlNode matchNode in compNode.ChildNodes)
                                {
                                    var matchNodeName = matchNode.Attributes["name"].Value.ToString();
                                    if (matchNodeName.Contains("Match Betting"))
                                    {
                                        List<Models.Team> teamsForMatchFields = new List<Models.Team>();
                                        foreach (XmlNode parNode in matchNode.ChildNodes)
                                        {
                                            TeamMaintenance.IsTeamNameRecorded(parNode.Attributes["name"].Value.ToString(),
                                                NewTeams, CurrentTeams);
                                            var team = new Models.Team(parNode.Attributes["name"].Value.ToString(),
                                                CurrentTeams);
                                            if (team.Name != "Draw")
                                                teamsForMatchFields.Add(team);
                                        }
                                        foreach (XmlNode parNode in matchNode.ChildNodes)
                                        {
                                            var match = new Models.Match()
                                            {
                                                Id = Convert.ToInt32(matchNode.Attributes["id"].Value),
                                                Name = matchNodeName,
                                                Bookmaker = BookmakersConstants.WilliamHillName,
                                                BookmakerId = BookmakersConstants.WilliamHillId,
                                                Competition = comp,
                                                LastUpdated =
                                                    Convert.ToDateTime(matchNode.Attributes["lastUpdateTime"].Value),
                                                Team1 = teamsForMatchFields.First(),
                                                Team2 = teamsForMatchFields.Last(),
                                                Date = Convert.ToDateTime(matchNode.Attributes["date"].Value),
                                                Time = matchNode.Attributes["time"].Value.ToString()
                                            };
                                            //store team if not already there

                                            var team = new Models.Team(parNode.Attributes["name"].Value.ToString(),
                                                CurrentTeams);
                                            match.Bet = team.Name;
                                            match.Odds = Convert.ToDecimal(parNode.Attributes["oddsDecimal"].Value);

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

        public void HorseRacingLogic(XmlDocument doc)
        {
            XmlNodeList xnList = doc.SelectNodes("/oxip/response/williamhill/class/type/market");

            foreach (XmlNode marketNode in xnList)
            {
                if (marketNode.Attributes["name"].Value.ToString().ToUpper().Contains("WIN"))
                {
                    var competitionName = marketNode.ParentNode.Attributes["name"].Value.ToString();
                    //todo check CompetitionHere
                    CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                    Models.Competition meeting = new Models.Competition(competitionName, CurrentComps);

                    var time = marketNode.Attributes["time"].Value.ToString();
                    var name = marketNode.Attributes["name"].Value.ToString();

                    foreach (XmlNode participant in marketNode.ChildNodes)
                    {

                        Decimal oddsDecimal;
                        if (Decimal.TryParse(participant.Attributes["oddsDecimal"].Value, out oddsDecimal))
                        {
                            Models.Race race = new Models.Race()
                            {
                                Id = Convert.ToInt32(participant.Attributes["id"].Value),
                                Name = marketNode.Attributes["name"].Value.ToString(),
                                BookmakerId = BookmakersConstants.WilliamHillId,
                                Bookmaker = BookmakersConstants.WilliamHillName,
                                Meeting = meeting,
                                Horse = participant.Attributes["name"].Value.ToString().Trim(),
                                Odds = oddsDecimal,
                                Date = Convert.ToDateTime(marketNode.Attributes["date"].Value.ToString()),
                                Time = marketNode.Attributes["time"].Value.ToString(),
                                LastUpdated = Convert.ToDateTime(participant.Attributes["lastUpdateDate"].Value),
                                Url = marketNode.Attributes["url"].Value.ToString()
                            };
                            Races.Add(race);
                        }
                    }
                }
            }
        }
       
        


    }
}
