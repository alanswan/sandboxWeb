
using Newtonsoft.Json;
using sandboxConsole.Helpers.Maintenance;
using sandboxConsole.Models;
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

namespace sandboxConsole.Helpers.XML.Exchange
{
    public class Betfair 
    {
        //Prem 31
        // 
        //public Betfair(List<EF.Team> teams, List<TeamsNotFound> newTeams, List<EF.Competition> comps, List<EF.CompetitionsNotFound> newComps) : base(teams, newTeams, comps, newComps)
        //{
        //    BetfairLogin();
        //}
        private BetfairLoginResponse LoginResponse;
        private List<BFCompetition> BfCompetitions;
        private List<int> EventIds;
        private List<BFMarket> BfMarkets;
        private List<string> CompetitionRegions;
        public Betfair()
        {
            this.LoginResponse = new BetfairLoginResponse();
            this.BfCompetitions = new List<BFCompetition>();
            this.BfMarkets = new List<BFMarket>();
            //this.CompetitionRegions = new List<string>() {"GBR", "ESP", "ITA", "DEU", "International"};
            this.CompetitionRegions = new List<string>() { "GBR" };
            this.EventIds = new List<int>();
        }

        public void Login()
        {
            BetfairLoginResponse loginReponse = new BetfairLoginResponse();
            using (var client = new HttpClient())
            {
                //BetfairLoginParams loginParams = new BetfairLoginParams()
                //{
                //    username = "kevin@wearedandify.com",
                //    password = "forest99"
                //};
                
                //client.BaseAddress = new Uri("https://api.betfair.com/exchange/betting/rest/v1.0/listEventTypes/");
                client.BaseAddress = new Uri("https://identitysso.betfair.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Application", "MoHFRmBODsw9VxE1");
                

               // var stringContent = new StringContent(@"""username""=""kevin@wearedandify.com""&""password""=""forest99""");
                //var convertedObject = JsonConvert.SerializeObject(loginParams);
                //var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/x-www-form-urlencoded");
                var loginContent = new StringContent("username=kevin@wearedandify.com&password=forest99", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = client.PostAsync("api/login", loginContent).Result;
                LoginResponse = (BetfairLoginResponse)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(BetfairLoginResponse));
            }
        }

        public void GetCompetitions()
        {
            using (var client = new HttpClient())
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

                BetfairEventRequest request = new BetfairEventRequest() {Filter = new Filter() {EventTypeIds = eventTypeIds}, MaxResults = 999 };
                //BetfairEventRequest request = new BetfairEventRequest() { filter = new Filter() {  } };

                var convertedObject = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = client.PostAsync("listCompetitions/", httpContent).Result;
                BfCompetitions = (List<BFCompetition>)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(List<BFCompetition>));
                BfCompetitions = BfCompetitions.Where(x => CompetitionRegions.Contains(x.CompetitionRegion)).ToList();

            }
        }
        public void GetEvents()
        {
            var compIds = BfCompetitions.Where(x => x.MarketCount > 0).Select(x => x.CompetitionDetails.Id).ToList();
            foreach (var compId in compIds)
            {
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
                    EventIds.AddRange(events.Select(x => x.eventDetails.Id).ToList());
                }
            }
        }
        public void GetMarkets()
        {
            var compIds = BfCompetitions.Where(x => x.MarketCount > 0).Select(x=>x.CompetitionDetails.Id).ToList();
            foreach (var comp in compIds)
            {
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
                        MaxResults = 999
                    };
                    //BetfairEventRequest request = new BetfairEventRequest() { filter = new Filter() {  } };

                    var convertedObject = JsonConvert.SerializeObject(request);
                    var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                    //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.PostAsync("listMarketCatalogue/", httpContent).Result;
                    var markets = (List<BFMarket>)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(List<BFMarket>));
                    this.BfMarkets.AddRange(markets);
                }
            }
        }

        public void GetMarketBooks()
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
                    Filter =
                        new MarketBookFilter()
                        {
                            MarketIds = new string[1] { "1.124646756" },
                            PriceProjection = new PriceProjection()
                            {
                                PriceData = new string[1] { "EX_BEST_OFFERS" },
                                Virtualise = true
                            }
                        }
                };
                var convertedObject = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(convertedObject, Encoding.UTF8, "application/json");
                //var httpContent = new StringContent("filter=", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = client.PostAsync("listMarketBook/", httpContent).Result;
            }
        }


    }
}
