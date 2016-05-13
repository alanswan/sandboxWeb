using sandboxWeb.Helpers.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandboxWeb.Models
{
    public class Competition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Competition()
        {
        }

        public Competition(string name, List<sandboxWeb.EF.Competition> correctComps)
        {
            Dictionary<int, string> teamDict = CompetitionMaintenance.GetCompetition(name, correctComps);
            this.Id = teamDict.First().Key;
            this.Name = teamDict.First().Value;
        }
    }

    public class Match
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Bookmaker { get; set; }
        public int BookmakerId { get; set; }
        public Competition Competition { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public string Bet { get; set; }
        public decimal Odds { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Time { get; set; }
        public string Url { get; set; }
        public string MobileUrl { get; set; }
        public decimal MoneyInMarket { get; set; }
    }

    public class Race
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Bookmaker { get; set; }
        public int BookmakerId { get; set; }
        public Competition Meeting { get; set; }
        public string Horse { get; set; }
        public decimal Odds { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Time { get; set; }
        public string Url { get; set; }
        public string MobileUrl { get; set; }
        public decimal MoneyInMarket { get; set; }
    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Team()
        {
        }

        public Team(string name, List<sandboxWeb.EF.Team> correctTeams)
        {
            Dictionary<int, string> teamDict = TeamMaintenance.GetTeam(name, correctTeams);
            this.Id = teamDict.First().Key;
            this.Name = teamDict.First().Value;
        }
    }

    public class Odds
    {
        public decimal Team1 { get; set; }
        public decimal Team2 { get; set; }
        public decimal Draw { get; set; }
    }
}
