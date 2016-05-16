
using Newtonsoft.Json;
using sandboxWeb.Helpers.Maintenance;
using sandboxWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Ajax.Utilities;
using sandboxWeb.Misc;
using System.Data.SqlClient;
using sandboxWeb.EF;
using EntityFramework.BulkInsert.Extensions;
using System.Diagnostics;

namespace sandboxWeb.Helpers.XML.Exchange
{
    public class Betfair : Company
    {
        //Prem 31
        // 
        //public Betfair(List<EF.Team> teams, List<TeamsNotFound> newTeams, List<EF.Competition> comps, List<EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        //{
        //    BetfairLogin();
        //}
        private BetfairLoginResponse LoginResponse;
        private List<BFCompetition> BfCompetitions;
        private List<string> EventIds;
        private List<BFMarket> BfMarkets;
        private List<BFMarketBook> BfMarketBooks;
        private List<BFEvent> BfEvents;
        private List<string> CompetitionRegions;
        private omproEntities db = new omproEntities();
        public Betfair(List<EF.Team> teams, List<EF.TeamsNotFound> newTeams, List<EF.Competition> comps, List<EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        {
            this.LoginResponse = new BetfairLoginResponse();
            this.BfCompetitions = new List<BFCompetition>();
            this.BfMarkets = new List<BFMarket>();
            this.BfMarketBooks = new List<BFMarketBook>();
            this.BfEvents = new List<BFEvent>();
            //this.CompetitionRegions = new List<string>() {"GBR", "ESP", "ITA", "DEU", "International"};
            
            this.EventIds = new List<string>();
        }

        public void Login()
        {
            BetfairLoginResponse loginReponse = new BetfairLoginResponse();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://identitysso.betfair.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                
                var loginContent = new StringContent("username=kevin@wearedandify.com&password=forest99", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = client.PostAsync("api/login", loginContent).Result;
                LoginResponse = (BetfairLoginResponse)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(BetfairLoginResponse));
                db.Errors.Add(new Error() {Error1 = LoginResponse.status + "|||" + LoginResponse.error});
            }
        }

        public void ReadBetfairFootball(string region)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            this.CompetitionRegions = new List<string>() { region };
            GetCompetitions();
            GetEvents();
            GetMarkets();
            GetMarketBooks();

            FootballLogic();
            stopwatch.Stop();
            Console.WriteLine(">>>>>>>> Time to do logic" + stopwatch.ElapsedMilliseconds.ToString());
            NotFoundToDB();
        }

        public void FootballLogic()
        {
            foreach (var market in BfMarkets)
            {
                var competitionName = market.Competition.Name;
                CompetitionMaintenance.IsCompetitionRecorded(competitionName, NewComps, CurrentComps);
                Models.Competition comp = new Models.Competition(competitionName, CurrentComps);

                var marketBook = BfMarketBooks.Where(x => x.MarketId == market.MarketId).SingleOrDefault();

                List<Models.Team> teamsForMatchFields = new List<Models.Team>();
                foreach (var marketRunner in market.Runners)
                {
                    TeamMaintenance.IsTeamNameRecorded(marketRunner.RunnerName, NewTeams, CurrentTeams);
                    var team = new Models.Team(marketRunner.RunnerName, CurrentTeams);
                    if (team.Name != "The Draw")
                        teamsForMatchFields.Add(team);
                }

                foreach (var marketRunner in market.Runners)
                {
                    var exchangeOdds =
                        marketBook.Runners.Where(x => x.SelectionId == marketRunner.SelectionId)
                            .Select(x => x.ExchangePrices)
                            .Select(x => x.AvailableToLay)
                            .FirstOrDefault();
                    var odds = exchangeOdds.Min(x => x.Price);
                    var money = exchangeOdds.Where(x => x.Price == odds).Select(x => x.Size).SingleOrDefault();

                    var match = new Models.Match()
                    {
                        Id = Convert.ToInt32(Convert.ToDecimal(market.MarketId)),
                        Name = market.Event.Name,
                        Bookmaker = BookmakersConstants.BetfairName,
                        BookmakerId = BookmakersConstants.BetfairId,
                        Competition = comp,
                        LastUpdated = DateTime.Now,
                        Team1 = teamsForMatchFields.First(),
                        Team2 = teamsForMatchFields.Last(),
                        Date = market.Event.OpenDate.Date,
                        Time = market.Event.OpenDate.AddHours(1).TimeOfDay.ToString().Substring(0, 5),
                        Bet = (marketRunner.RunnerName == "The Draw") ? "Draw" : marketRunner.RunnerName,
                        Odds = Convert.ToDecimal(odds),
                        MoneyInMarket = Convert.ToDecimal(money)
                    };
                    this.Matches.Add(match);
                }
            }
        }

        public void RefreshDB()
        {
            SqlConnection conn = new SqlConnection();
            var ConnectionString =
                "data source=mssql2.gear.host;initial catalog=ompro;persist security info=True;user id=ompro;password=Co31?i!8iF74;MultipleActiveResultSets=True;";

            using (SqlConnection sc = new SqlConnection(ConnectionString))
            {
                sc.Open();
                using (var cmd = sc.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM ExchangeMatches WHERE BookmakerId = @id";
                    cmd.Parameters.AddWithValue("@id", BookmakersConstants.BetfairId);
                    cmd.ExecuteNonQuery();
                }
            }

            var bulkExchange = new List<ExchangeMatch>();
            foreach (Models.Match match in this.Matches)
            {
                if (match.Team1.Name != null && match.Team1.Id != 999999)
                {
                    bulkExchange.Add(new ExchangeMatch()
                    {
                        MatchId = match.Id,
                        Name = match.Name,
                        BookmakerId = match.BookmakerId,
                        CompetitionId = match.Competition.Id,
                        CompetitionName = match.Competition.Name,
                        Team1Id = match.Team1.Id,
                        Team1Name = match.Team1.Name,
                        Team2Id = match.Team2.Id,
                        Team2Name = match.Team2.Name,
                        Bet = match.Bet,
                        Odds = match.Odds,
                        Date = match.Date,
                        LastUpdated = match.LastUpdated,
                        Time = match.Time,
                        MoneyInMarket = match.MoneyInMarket,
                        URL = match.Url,
                        MobileURL = match.MobileUrl
                    });
                }
            }
            
            db.BulkInsert(bulkExchange);
            
        }

        public void NotFoundToDB()
        {
            foreach (EF.TeamsNotFound team in NewTeams)
            {
                if (!db.TeamsNotFounds.Any(x => x.TeamName == team.TeamName))
                    db.TeamsNotFounds.Add(team);
            };
            foreach (EF.CompetitionsNotFound comp in NewComps)
            {
                if (!db.CompetitionsNotFounds.Any(x => x.CompetitionName == comp.CompetitionName))
                    db.CompetitionsNotFounds.Add(comp);
            };
        }

        public void GetCompetitions()
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    System.Net.ServicePointManager.Expect100Continue = false;
                    client.BaseAddress = new Uri("https://api.betfair.com/exchange/betting/rest/v1.0/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                    client.DefaultRequestHeaders.Add("X-Authentication", LoginResponse.token);

                    //football
                    int[] eventTypeIds = new int[1];
                    eventTypeIds[0] = 1;

                    BetfairEventRequest request = new BetfairEventRequest()
                    {
                        Filter = new Filter() {EventTypeIds = eventTypeIds},
                        MaxResults = 999
                    };
                    //BetfairEventRequest request = new BetfairEventRequest() { filter = new Filter() {  } };

                    var convertedObject = JsonConvert.SerializeObject(request);
                    var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                    //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                    //error = client.PostAsync("listCompetitions/", httpContent).Result.ToString();
                    response = client.PostAsync("listCompetitions/", httpContent).Result;
                    BfCompetitions =
                        (List<BFCompetition>)
                            JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                                typeof (List<BFCompetition>));
                    BfCompetitions =
                        BfCompetitions.Where(x => CompetitionRegions.Contains(x.CompetitionRegion)).ToList();
                }
                catch (Exception e)
                {
                    db.Errors.Add(new Error() { Error1 = response.Content.ReadAsStringAsync().Result });
                    db.SaveChanges();
                }
                //finally
                //{
                //    db.Errors.Add(new Error() { Error1 = response.ToString() });
                //    db.SaveChanges();
                //}
            }
        }
        public void GetEvents()
        {
            var compIds = BfCompetitions.Where(x => x.MarketCount > 0).Select(x => x.CompetitionDetails.Id).ToList();
            //foreach (var compId in compIds)
            //{
                using (var client = new HttpClient())
                {
                    System.Net.ServicePointManager.Expect100Continue = false;
                    client.BaseAddress = new Uri("https://api.betfair.com/exchange/betting/rest/v1.0/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                    client.DefaultRequestHeaders.Add("X-Authentication", LoginResponse.token);

                    int[] eventTypeIds = new int[1];
                    eventTypeIds[0] = 1;

                    BetfairEventRequest request = new BetfairEventRequest()
                    {
                        Filter = new Filter() {EventTypeIds = eventTypeIds, CompetitionIds = compIds.ToArray()},
                        MaxResults = 999
                    };
                    //BetfairEventRequest request = new BetfairEventRequest() { filter = new Filter() {  } };

                    var convertedObject = JsonConvert.SerializeObject(request);
                    var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                    //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.PostAsync("listEvents/", httpContent).Result;
                    var events = (List<BFEvent>)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(List<BFEvent>));
                    BfEvents.AddRange(events);
                    EventIds.AddRange(events.Select(x => x.eventDetails.Id).ToList());
              //  }
            }
        }
        public void GetMarkets()
        {
            var compIds = BfCompetitions.Where(x => x.MarketCount > 0).Select(x=>x.CompetitionDetails.Id).ToList();
            //foreach (var comp in compIds)
            //{
                using (var client = new HttpClient())
                {
                    System.Net.ServicePointManager.Expect100Continue = false;
                    client.BaseAddress = new Uri("https://api.betfair.com/exchange/betting/rest/v1.0/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                    client.DefaultRequestHeaders.Add("X-Authentication", LoginResponse.token);

                    int[] eventTypeIds = new int[1];
                    eventTypeIds[0] = 1;

                    string[] marketTypeCodes = new string[1];
                    marketTypeCodes[0] = "MATCH_ODDS";

                    BetfairEventRequest request = new BetfairEventRequest()
                    {
                        Filter =
                            new Filter()
                            {
                                EventTypeIds = eventTypeIds,
                                CompetitionIds = compIds.ToArray(),
                                MarketTypeCodes = marketTypeCodes
                            },
                        MaxResults = 999,
                        MarketProjection = new string[] {"COMPETITION","EVENT", "RUNNER_DESCRIPTION", "RUNNER_METADATA"}

                    };
                    //BetfairEventRequest request = new BetfairEventRequest() { filter = new Filter() {  } };

                    var convertedObject = JsonConvert.SerializeObject(request);
                    var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                    //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.PostAsync("listMarketCatalogue/", httpContent).Result;
                    //there are more details available in the description property of a BFMarket - not currently called as deemed as not needed - see MarketCatalogue in the api
                    var markets = (List<BFMarket>)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(List<BFMarket>));
                    this.BfMarkets.AddRange(markets);
                }
            //}
        }

        public void GetMarketBooks()
        {
            for (int i = 0; i < this.BfMarkets.Count; i = i + 10)
            {
                
                using (var client = new HttpClient())
                {
                    System.Net.ServicePointManager.Expect100Continue = false;
                    client.BaseAddress = new Uri("https://api.betfair.com/exchange/betting/rest/v1.0/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                    client.DefaultRequestHeaders.Add("X-Authentication", LoginResponse.token);
                    decimal[] marketIds = this.BfMarkets.Select(x => x.MarketId).ToArray();

                    BetfairMarketRequest request = new BetfairMarketRequest()
                    {

                        MarketIds = this.BfMarkets.Select(x => x.MarketId.ToString()).Skip(i).Take(10).ToArray(),
                        PriceProjection = new PriceProjection()
                        {
                            PriceData = new string[2] {"EX_BEST_OFFERS", "EX_TRADED"},
                            Virtualise = "true"
                        },
                        //OrderProjection = "ALL",
                        //MatchProjection = "NO_ROLLUP"

                    };
                    var convertedObject = JsonConvert.SerializeObject(request);
                    var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                    //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.PostAsync("listMarketBook/", httpContent).Result;
                    var marketBooks =
                        (List<BFMarketBook>)
                            JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result,
                                typeof (List<BFMarketBook>));
                    this.BfMarketBooks.AddRange(marketBooks);
                }
            }
        }

        


    }
}
