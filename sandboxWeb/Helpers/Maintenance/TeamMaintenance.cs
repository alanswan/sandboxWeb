
using sandboxWeb.Models;
using sandboxWeb.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandboxWeb.Helpers.Maintenance
{
    public class TeamMaintenance
    {
        public static void IsTeamNameRecorded(string teamName, List<TeamsNotFound> newTeams, List<sandboxWeb.EF.Team> currentTeams)
        {
            if (!currentTeams.Any(x => x.TeamName.ToUpper() == teamName.Trim().ToUpper()) && !newTeams.Any(x => x.TeamName.ToUpper() == teamName.Trim().ToUpper()) && (teamName != "" || teamName != null)) {
                newTeams.Add(new TeamsNotFound() { TeamName = teamName.Trim() });
            }
        }

        ///<summary>
        ///Returns Correct Team from list past by the database, or the original
        ///<para>name : team name from bookmaker data feed </para>
        ///<para>correctteams : List of teams, hopefully from the database, giving teams already recorded</para>
        ///</summary>
        public static Dictionary<int, string> GetTeam(string name, List<sandboxWeb.EF.Team> correctTeams)
        {
            name = name.Trim();
            sandboxWeb.EF.Team team = correctTeams.FirstOrDefault(x => x.TeamName.ToUpper() == name.ToUpper());
            
            if(team == null)
            {
                var dict = new Dictionary<int, string>();
                dict.Add(999999, name);
                return dict;
            }

            try {
                var correctTeam = correctTeams.Single(x => x.OMTeamId == team.OMTeamId && x.DefaultName == true);
                var newDict = new Dictionary<int, string>();
                newDict.Add(team.OMTeamId, correctTeam.TeamName);
                return newDict;
            }
            catch(Exception e )
            {
                //******* TODO - MUST NOTIFY US THAT THIS HAS HAPPENED ***********//
                //******* IT MEANS THERE IS AN ENTRY IN OUR TEAMS DATABASE THAT DOESN'T HAVE A DEFAULT NAME ********//

                var dict = new Dictionary<int, string>();
                dict.Add(999999, name);
                return dict;
            }
        }
    }
}
