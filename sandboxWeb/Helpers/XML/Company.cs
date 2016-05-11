using sandboxWeb.EF;
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandboxConsole.Helpers.XML
{
    public class Company
    {
        public List<Models.Competition> Competitions;
        public List<Models.Match> Matches;
        public List<Models.Race> Races; 
        public List<sandboxWeb.EF.Team> CurrentTeams;
        public List<sandboxWeb.EF.TeamsNotFound> NewTeams;
        public List<sandboxWeb.EF.Competition> CurrentComps;
        public List<sandboxWeb.EF.CompetitionsNotFound> NewComps;

        public Company(List<sandboxWeb.EF.Team> teams, List<TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps )
        {
            this.Competitions = new List<Models.Competition>();
            this.CurrentTeams = teams;
            this.NewTeams = newTeams;
            this.CurrentComps = comps;
            this.NewComps = newComps;
            this.Matches = new List<Models.Match>();
            this.Races = new List<Models.Race>();
        }
    }
}
