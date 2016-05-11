using sandboxWeb.EF;
using sandboxConsole.Helpers.Maintenance;
using sandboxConsole.Misc;
using sandboxConsole.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sandboxConsole.Helpers.XML
{
    public class Eight88 : Company
    {
        public Eight88(List<sandboxWeb.EF.Team> teams, List<sandboxWeb.EF.TeamsNotFound> newTeams, List<sandboxWeb.EF.Competition> comps, List<sandboxWeb.EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        {}

        public void Read888Football()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://www.smart-feeds.com/getfeeds.aspx?Param=betoffer/group/1000094985");
            }
            
        }
        
        
    }
}
