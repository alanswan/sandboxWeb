
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sandboxWeb.EF;

namespace sandboxConsole.Helpers.Maintenance
{
    public class CompetitionMaintenance
    {
        public static void IsCompetitionRecorded(string competitionName, List<sandboxWeb.EF.CompetitionsNotFound> newCompetitions, List<sandboxWeb.EF.Competition> currentCompetitions)
        {
            competitionName = competitionName.Trim();
            if (!currentCompetitions.Any(x => x.CompetitionName.ToUpper() == competitionName.ToUpper()) &&
                !newCompetitions.Any(x => x.CompetitionName.ToUpper() == competitionName.ToUpper()) &&
                (competitionName != "" && competitionName != null))
            {
                newCompetitions.Add(new sandboxWeb.EF.CompetitionsNotFound() { CompetitionName = competitionName});
            }
        }

        ///<summary>
        ///Returns Correct Team from list past by the database, or the original
        ///<para>name : team name from bookmaker data feed </para>
        ///<para>correctteams : List of teams, hopefully from the database, giving teams already recorded</para>
        ///</summary>
        public static Dictionary<int, string> GetCompetition(string name, List<sandboxWeb.EF.Competition> correctCompetitions)
        {
            name = name.Trim();
            sandboxWeb.EF.Competition comp = correctCompetitions.FirstOrDefault(x => x.CompetitionName.ToUpper() == name.ToUpper());

            if (comp == null)
            {
                var dict = new Dictionary<int, string>();
                dict.Add(999999, name);
                return dict;
            }

            try
            {
                var correctComp = correctCompetitions.Single(x => x.OMCompetitionId == comp.OMCompetitionId && x.DefaultName == true);
                var newDict = new Dictionary<int, string>();
                newDict.Add(correctComp.OMCompetitionId, correctComp.CompetitionName);
                return newDict;
            }
            catch (Exception e)
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
