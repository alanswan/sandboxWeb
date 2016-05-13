using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandboxWeb.Models
{
    public class BetfairLoginParams
    {
        public string username { get; set; }
        public string password { get; set; }    
    }

    public class BetfairLoginResponse
    {
        public string token { get; set; }
        public string product { get; set; }
        public string status { get; set; }
        public string error { get; set; }
    }

    public class BetfairEventRequest
    {
        [JsonProperty(PropertyName = "filter")]
        public Filter Filter { get; set; }
        [JsonProperty(PropertyName = "maxResults")]
        public int MaxResults { get; set; }
        [JsonProperty(PropertyName = "marketProjection")]
        public string[] MarketProjection { get; set; }
    }

    public class BetfairMarketRequest
    {
        //[JsonProperty(PropertyName = "filter")]
        //public MarketBookFilter Filter { get; set; }
        //[JsonProperty(PropertyName = "maxResults")]
        //public int MaxResults { get; set; }
        [JsonProperty(PropertyName = "marketIds")]
        public string[] MarketIds { get; set; }
        [JsonProperty(PropertyName = "priceProjection")]
        public PriceProjection PriceProjection { get; set; }
        [JsonProperty(PropertyName = "matchProjection")]
        public string MatchProjection { get; set; }
        [JsonProperty(PropertyName = "orderProjection")]
        public string OrderProjection { get; set; }
    }


    public class Filter
    {
        [JsonProperty(PropertyName = "eventTypeIds")]
        public int[] EventTypeIds { get; set; }
        [JsonProperty(PropertyName = "competitionIds")]
        public int[] CompetitionIds { get; set; }
        [JsonProperty(PropertyName = "marketTypeCodes")]
        public string[] MarketTypeCodes { get; set; }
    }

    public class MarketBookFilter
    {
        [JsonProperty(PropertyName = "marketIds")]
        public string[] MarketIds { get; set; }
        [JsonProperty(PropertyName = "priceProjection")]
        public PriceProjection PriceProjection { get; set; }
        [JsonProperty(PropertyName = "matchProjection")]
        public string MatchProjection { get; set; }
        [JsonProperty(PropertyName = "orderProjection")]
        public string OrderProjection { get; set; }
    }

    public class PriceProjection
    {
        [JsonProperty(PropertyName = "priceData")]
        public string[] PriceData { get; set; }
        [JsonProperty(PropertyName = "virtualise")]
        public string Virtualise { get; set; }
    }

    public class BFCompetition
    {
        [JsonProperty(PropertyName = "competition")]
        public CompetitonDetails CompetitionDetails { get; set; }
        [JsonProperty(PropertyName = "marketCount")]
        public int MarketCount { get; set; }
        [JsonProperty(PropertyName = "competitionRegion")]
        public string CompetitionRegion { get; set; }
    }

    public class CompetitonDetails
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class BFEvent
    {
        [JsonProperty(PropertyName = "event")]
        public EventDetails eventDetails { get; set; }
        [JsonProperty(PropertyName = "marketCount")]
        public int MarketCount { get; set; }
    }

    public class EventDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }
        [JsonProperty(PropertyName = "timezone")]
        public string Timezone { get; set; }
        [JsonProperty(PropertyName = "openDate")]
        public DateTime OpenDate  { get; set; }
        [JsonProperty(PropertyName = "venue")]
        public string Venue { get; set; }
    }

    public class BFMarket
    {
        [JsonProperty(PropertyName = "marketId")]
        public decimal MarketId { get; set; }
        [JsonProperty(PropertyName = "marketName")]
        public string MarketName { get; set; }
        [JsonProperty(PropertyName = "marketStartTime")]
        public DateTime MarketStartTime { get; set; }
        [JsonProperty(PropertyName = "totalMatched")]
        public double TotalMatched { get; set; }
        [JsonProperty(PropertyName = "runners")]
        public List<Runner> Runners { get; set; }
        [JsonProperty(PropertyName = "competition")]
        public CompetitonDetails Competition { get; set; }
        [JsonProperty(PropertyName = "event")]
        public EventDetails Event { get; set; }

    }

    public class Runner
    {
        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }
        [JsonProperty(PropertyName = "runnerName")]
        public string RunnerName { get; set; }
        [JsonProperty(PropertyName = "handicap")]
        public double Handicap { get; set; }
        [JsonProperty(PropertyName = "sortPriority")]
        public int SortPriority { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string RunnerStatus { get; set; }
        [JsonProperty(PropertyName = "adjustmentFactor")]
        public double AdjustmentFactor { get; set; }
        [JsonProperty(PropertyName = "lastPriceTraded")]
        public double LastPriceTraded { get; set; }
        [JsonProperty(PropertyName = "totalMatched")]
        public double TotalMatched { get; set; }
        [JsonProperty(PropertyName = "removalDate")]
        public DateTime RemovalDate { get; set; }
        [JsonProperty(PropertyName = "ex")]
        public ExchangePrices ExchangePrices { get; set; }
    }

    public class BFMarketBook {
        [JsonProperty(PropertyName = "marketId")]
        public decimal MarketId { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "runners")]
        public List<Runner> Runners { get; set; }
    }


    public class ExchangePrices
    {
        [JsonProperty(PropertyName = "availableToBack")]
        public List<PriceSize> AvailableToBack { get; set; }
        [JsonProperty(PropertyName = "availableToLay")]
        public List<PriceSize> AvailableToLay { get; set; }
        [JsonProperty(PropertyName = "tradedVolume")]
        public List<PriceSize> TradedVolume { get; set; }
    }

    public class PriceSize
    {
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }
        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }
    }


    public class MarketFilter
    {
        [JsonProperty(PropertyName = "textQuery")]
        public string TextQuery { get; set; }

        [JsonProperty(PropertyName = "exchangeIds")]
        public ISet<string> ExchangeIds { get; set; }

        [JsonProperty(PropertyName = "eventTypeIds")]
        public ISet<string> EventTypeIds { get; set; }

        [JsonProperty(PropertyName = "eventIds")]
        public ISet<string> EventIds { get; set; }

        [JsonProperty(PropertyName = "competitionIds")]
        public ISet<string> CompetitionIds { get; set; }

        [JsonProperty(PropertyName = "marketIds")]
        public ISet<string> MarketIds { get; set; }

        [JsonProperty(PropertyName = "venues")]
        public ISet<string> Venues { get; set; }

        [JsonProperty(PropertyName = "bspOnly")]
        public bool? BspOnly { get; set; }

        [JsonProperty(PropertyName = "turnInPlayEnabled")]
        public bool? TurnInPlayEnabled { get; set; }

        [JsonProperty(PropertyName = "inPlayOnly")]
        public bool? InPlayOnly { get; set; }

        [JsonProperty(PropertyName = "marketBettingTypes")]
        public ISet<MarketBettingType> MarketBettingTypes { get; set; }

        [JsonProperty(PropertyName = "marketCountries")]
        public ISet<string> MarketCountries { get; set; }

        [JsonProperty(PropertyName = "marketTypeCodes")]
        public ISet<string> MarketTypeCodes { get; set; }

        [JsonProperty(PropertyName = "marketStartTime")]
        public TimeRange MarketStartTime { get; set; }

        [JsonProperty(PropertyName = "withOrders")]
        public ISet<OrderStatus> WithOrders { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketBettingType
    {
        ODDS,
        LINE,
        RANGE,
        ASIAN_HANDICAP_DOUBLE_LINE,
        ASIAN_HANDICAP_SINGLE_LINE,
        FIXED_ODDS
    }

    public class TimeRange
    {
        [JsonProperty(PropertyName = "from")]
        public DateTime From { get; set; }

        [JsonProperty(PropertyName = "to")]
        public DateTime To { get; set; }

        public override string ToString()
        {
            return new StringBuilder().AppendFormat("{0}", "TimeRange")
                        .AppendFormat(" : From={0}", From)
                        .AppendFormat(" : To={0}", To)
                        .ToString();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        EXECUTION_COMPLETE,
        EXECUTABLE
    }


}
