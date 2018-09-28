using System;
using System.Timers;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CryproProcessor
{
    /**
     * Prices - Pulls price data of currencies 
     * @author Vance Field
     * @version 28-Mar-2018
     */ 
    public class Prices
    {
        // databasecontroller object
        public static DatabaseController dbController = new DatabaseController();

        // httpclient 
        private static HttpClient client = new HttpClient();

        // httpresponse
        private static HttpResponseMessage responsePoloniex;
        private static HttpResponseMessage responseBittrex;
        private static HttpResponseMessage responseCoinsquare;

        // API links for coins
        private const string URL_POLONIEX = "https://poloniex.com/public?command=returnTicker"; // has all 4 coins for USDT
        private const string URL_BITTREX = "https://bittrex.com/api/v1.1/public/getmarketsummaries"; // has all 4 coins for USDT
        private const string URL_COINSQUARE = "https://coinsquare.io/api/v1/data/quotes"; // has btc_usd in strange format

        // timer time interval
        private const double TIMER_INTERVAL = 60000;


        /**
         * Constructor
         */
        public Prices()
        {
            // required for httpclient pulling from poloniex
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // create database connection
            dbController.InitConnection();
        }


        /**
         * Starts `TIMER_INTERVAL` timer
         */
        public void StartTimer()
        {
            Console.WriteLine(DateTime.Now + " - Starting timer.\n");
            Timer timer = new Timer();
            timer.Interval = TIMER_INTERVAL;
            timer.Elapsed += new ElapsedEventHandler(Timer_ElapsedAsync);
            timer.Start();
            Console.ReadLine();
        }

        /**
         * Timer event. 
         * Fires every `TIMER_INTERVAL` (60ms).
         */ 
        private static async void Timer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            // pull from exchanges
            responsePoloniex = await client.GetAsync(URL_POLONIEX);
            responseBittrex = await client.GetAsync(URL_BITTREX);
            responseCoinsquare = await client.GetAsync(URL_COINSQUARE);            
            try
            { 
                responsePoloniex.EnsureSuccessStatusCode();
                responseBittrex.EnsureSuccessStatusCode();
                responseCoinsquare.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - pull from exchanges");
                Console.WriteLine("     - " + ex.Message);
            }
           
            // parse json into string
            string contentPoloniex = await responsePoloniex.Content.ReadAsStringAsync();
            string contentBittrex = await responseBittrex.Content.ReadAsStringAsync();
            string contentCoinsquare = await responseCoinsquare.Content.ReadAsStringAsync(); 
         

            // insert data into database...

            // insert btc-eth into BTC_ETH_RATE table
            try
            {
                dbController.InsertCoinPairRates("BTC_ETH_RATE", GetPoloniexCoinPairLast(contentPoloniex, "BTC_ETH"), GetBitTrexCoinPairLast(contentBittrex, "BTC-ETH"), GetCoinSquareCoinPairLast(contentCoinsquare, "ETH"));
                dbController.InsertCoinPairRates("BTC_LTC_RATE", GetPoloniexCoinPairLast(contentPoloniex, "BTC_LTC"), GetBitTrexCoinPairLast(contentBittrex, "BTC-LTC"), GetCoinSquareCoinPairLast(contentCoinsquare, "LTC"));
                dbController.InsertCoinPairRates("BTC_BCH_RATE", GetPoloniexCoinPairLast(contentPoloniex, "BTC_BCH"), GetBitTrexCoinPairLast(contentBittrex, "BTC-BCC"), GetCoinSquareCoinPairLast(contentCoinsquare, "BCH"));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - RATE pair");
                Console.WriteLine("     - " + ex.Message);
            }


            
            // insert btc-eth from all exchanges
            try { 
                dbController.InsertCoinPair("BTC_ETH", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "BTC_ETH"), GetPoloniexCoinPairVolume(contentPoloniex, "BTC_ETH"));
                dbController.InsertCoinPair("BTC_ETH", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "BTC-ETH"), GetBitTrexCoinPairVolume(contentBittrex, "BTC-ETH"));
                dbController.InsertCoinPair("BTC_ETH", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "ETH"), GetCoinSquareCoinPairVolume(contentCoinsquare, "ETH"));
            }
            catch(Exception ex){            
                Console.WriteLine("Fail - BTC_ETH pair");
                Console.WriteLine("     - " + ex.Message);
            }

            // insert btc-ltc from all exchanges
            try { 
                dbController.InsertCoinPair("BTC_LTC", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "BTC_LTC"), GetPoloniexCoinPairVolume(contentPoloniex, "BTC_LTC"));
                dbController.InsertCoinPair("BTC_LTC", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "BTC-LTC"), GetBitTrexCoinPairVolume(contentBittrex, "BTC-LTC"));
                dbController.InsertCoinPair("BTC_LTC", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "LTC"), GetCoinSquareCoinPairVolume(contentCoinsquare, "LTC"));
            }
            catch (Exception ex) {
                Console.WriteLine("Fail - BTC_LTC pair");
                Console.WriteLine("     - " + ex.Message);
            }

            // insert btc-bch from all exchanges
            try {
                dbController.InsertCoinPair("BTC_BCH", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "BTC_BCH"), GetPoloniexCoinPairVolume(contentPoloniex, "BTC_BCH"));
                dbController.InsertCoinPair("BTC_BCH", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "BTC-BCC"), GetBitTrexCoinPairVolume(contentBittrex, "BTC-BCC"));
                dbController.InsertCoinPair("BTC_BCH", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "BCH"), GetCoinSquareCoinPairVolume(contentCoinsquare, "BCH"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - BTC_BCH pair");
                Console.WriteLine("     - " + ex.Message);
            }
            
            // insert btc-usdt from all exchanges
            try
            {
                dbController.InsertCoinPair("BTC_USDT", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "USDT_BTC"), GetPoloniexCoinPairVolume(contentPoloniex, "USDT_BTC"));
                dbController.InsertCoinPair("BTC_USDT", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "USDT-BTC"), GetBitTrexCoinPairVolume(contentBittrex, "USDT-BTC"));
                //dbController.InsertCoinPair("BTC_USDT", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "USD"), GetCoinSquareCoinPairVolume(contentCoinsquare, "USD"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - BTC_USDT pair");
                Console.WriteLine("     - " + ex.Message);
            }

            // insert eth-usdt from all exchanges
            try
            {
                dbController.InsertCoinPair("ETH_USDT", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "USDT_ETH"), GetPoloniexCoinPairVolume(contentPoloniex, "USDT_ETH"));
                dbController.InsertCoinPair("ETH_USDT", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "USDT-ETH"), GetBitTrexCoinPairVolume(contentBittrex, "USDT-ETH"));
                //dbController.InsertCoinPair("BTC_USDT", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "USD"), GetCoinSquareCoinPairVolume(contentCoinsquare, "USD"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - ETH_USDT pair");
                Console.WriteLine("     - " + ex.Message);
            }

            // insert ltc-usdt from all exchanges
            try
            {
                dbController.InsertCoinPair("LTC_USDT", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "USDT_LTC"), GetPoloniexCoinPairVolume(contentPoloniex, "USDT_LTC"));
                dbController.InsertCoinPair("LTC_USDT", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "USDT-LTC"), GetBitTrexCoinPairVolume(contentBittrex, "USDT-LTC"));
                //dbController.InsertCoinPair("BTC_USDT", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "USD"), GetCoinSquareCoinPairVolume(contentCoinsquare, "USD"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - LTC_USDT pair");
                Console.WriteLine("     - " + ex.Message);
            }

            // insert bch-usdt from all exchanges
            try
            {
                dbController.InsertCoinPair("BCH_USDT", "Poloniex", GetPoloniexCoinPairLast(contentPoloniex, "USDT_BCH"), GetPoloniexCoinPairVolume(contentPoloniex, "USDT_BCH"));
                dbController.InsertCoinPair("BCH_USDT", "Bittrex", GetBitTrexCoinPairLast(contentBittrex, "USDT-BCC"), GetBitTrexCoinPairVolume(contentBittrex, "USDT-BCC"));
                //dbController.InsertCoinPair("BTC_USDT", "CoinSquare", GetCoinSquareCoinPairLast(contentCoinsquare, "USD"), GetCoinSquareCoinPairVolume(contentCoinsquare, "USD"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail - BCH_USDT pair");
                Console.WriteLine("     - " + ex.Message);
            }
            
            // insert optimal trade route data
            try
            {
                dbController.UpdateTradeRoute("MarketState", "Poloniex"  , "BTC", "ETH", GetPoloniexCoinPairLast(contentPoloniex, "BTC_ETH")); // Poloniex: BTC_ETH
                dbController.UpdateTradeRoute("MarketState", "Poloniex"  , "BTC", "LTC", GetPoloniexCoinPairLast(contentPoloniex, "BTC_LTC")); // Poloniex: BTC_LTC
                dbController.UpdateTradeRoute("MarketState", "Poloniex"  , "BTC", "BCH", GetPoloniexCoinPairLast(contentPoloniex, "BTC_BCH")); // Poloniex: BTC_BCH
                dbController.UpdateTradeRoute("MarketState", "Bittrex"   , "BTC", "ETH", GetBitTrexCoinPairLast(contentBittrex, "BTC-ETH")  ); // Bittrex : BTC_ETH
                dbController.UpdateTradeRoute("MarketState", "Bittrex"   , "BTC", "LTC", GetBitTrexCoinPairLast(contentBittrex, "BTC-LTC")  ); // Bittrex : BTC_LTC
                dbController.UpdateTradeRoute("MarketState", "Bittrex"   , "BTC", "BCH", GetBitTrexCoinPairLast(contentBittrex, "BTC-BCC")  ); // Bittrex : BTC_BCH
                dbController.UpdateTradeRoute("MarketState", "Coinsquare", "BTC", "ETH", GetCoinSquareCoinPairLast(contentCoinsquare, "ETH")); // Coinsquare : BTC_ETH
                dbController.UpdateTradeRoute("MarketState", "Coinsquare", "BTC", "LTC", GetCoinSquareCoinPairLast(contentCoinsquare, "LTC")); // Coinsquare : BTC_LTC
                dbController.UpdateTradeRoute("MarketState", "Coinsquare", "BTC", "BCH", GetCoinSquareCoinPairLast(contentCoinsquare, "BCH")); // Coinsquare : BTC_BCH
            }
            catch(Exception ex)
            {
                Console.WriteLine(DateTime.Now + " Fail - Inserting optimal trade route data.");
                Console.WriteLine("                     - " + ex.Message);
            }
            

            // get BTC_ETH last from exchanges
            /*
            // get BTC_ETH last from poloniex           
            Console.WriteLine("Poloniex BTC_ETH last: \t\t" + GetPoloniexCoinPairLast(contentPoloniex, "BTC_ETH"));
            // get BTC_ETH last from bittrex            
            Console.WriteLine("BitTrex BTC_ETH last: \t\t" + GetBitTrexCoinPairLast(contentBittrex, "BTC-ETH"));
            // get BTC_ETH last from coinsquare
            Console.WriteLine("CoinSquare BTC_ETH last: \t" + GetCoinSquareCoinPairLast(contentCoinsquare, "ETH"));
            Console.WriteLine();

            // get BTC_ETH volume from exchanges

            // get BTC_ETH volume from poloniex  
            Console.WriteLine("Poloniex BTC_ETH volume: \t" + GetPoloniexCoinPairVolume(contentPoloniex, "BTC_ETH"));
            // get BTC_ETH volume from bittrex
            Console.WriteLine("BitTrex BTC_ETH volume: \t" + GetBitTrexCoinPairVolume(contentBittrex, "BTC-ETH"));
            // get BTC_ETH volume from coinsquare
            Console.WriteLine("CoinSquare BTC_ETH volume: \t" + GetCoinSquareCoinPairVolume(contentCoinsquare, "ETH"));
            Console.WriteLine();

            // get BTC_LTC last from exchanges

            // get BTC_LTC last from poloniex
            Console.WriteLine("Poloniex BTC_LTC last: \t\t" + GetPoloniexCoinPairLast(contentPoloniex, "BTC_LTC"));
            // get BTC_LTC last from bittrex            
            Console.WriteLine("BitTrex BTC_LTC last: \t\t" + GetBitTrexCoinPairLast(contentBittrex, "BTC-LTC"));
            // get BTC_LTC last from coinsquare
            Console.WriteLine("CoinSquare BTC_LTC last: \t" + GetCoinSquareCoinPairLast(contentCoinsquare, "LTC"));
            Console.WriteLine();

            // get BTC_LTC volume from exchanges

            // get BTC_LTC volume from poloniex
            Console.WriteLine("Poloniex BTC_LTC volume: \t" + GetPoloniexCoinPairVolume(contentPoloniex, "BTC_LTC"));
            // get BTC_LTC volume from bittrex
            Console.WriteLine("BitTrex BTC_LTC volume: \t" + GetBitTrexCoinPairVolume(contentBittrex, "BTC-LTC"));
            // get BTC_LTC volume from coinsquare
            Console.WriteLine("CoinSquare BTC_LTC volume: \t" + GetCoinSquareCoinPairVolume(contentCoinsquare, "LTC"));
            Console.WriteLine();

            // get BTC_BCH last from exchanges

            // get BTC_BCH (Bitcoin cash) last from poloniex
            Console.WriteLine("Poloniex BTC_BCH last: \t\t" + GetPoloniexCoinPairLast(contentPoloniex, "BTC_BCH"));
            // get BTC-BCH (Bitcoin cash) last from bittrex
            Console.WriteLine("BitTrex BTC_BCH last: \t\t" + GetBitTrexCoinPairLast(contentBittrex, "BTC-BCC"));
            // get BTC-BCH (Bitcoin cash) last from coinsquare
            Console.WriteLine("CoinSquare BTC_BCH last: \t" + GetCoinSquareCoinPairLast(contentCoinsquare, "BCH"));
            Console.WriteLine();

            // get BTC_BCH (Bitcoin cash) volume from poloniex
            Console.WriteLine("Poloniex BTC_BCH volume: \t" + GetPoloniexCoinPairVolume(contentPoloniex, "BTC_BCH"));
            // get BTC-BCH (Bitcoin cash) volume from bittrex
            Console.WriteLine("BitTrex BTC_BCH volume: \t" + GetBitTrexCoinPairVolume(contentBittrex, "BTC-BCC"));
            // get BTC-BCH (Bitcoin cash) volume from coinsquare
            Console.WriteLine("CoinSquare BTC_BCH volume: \t" + GetCoinSquareCoinPairVolume(contentCoinsquare, "BCH"));
            Console.WriteLine();                     
            */



        }


        /**
         * Task to parse the given Poloniex `coinPair` and return its Last price
         * @param content  : the json string
         * @param coinPair : the coin pair to retrieve 
         */
        private static decimal GetPoloniexCoinPairLast(string content, string coinPair)
        {
            // parse here
            JObject obj = JObject.Parse(content);
            return Decimal.Parse(obj.SelectToken(coinPair).SelectToken("last").ToString());
            
        }

        /**
         * Task to parse the given Poloniex `coinPair` and return its Volume
         * @param content  : the json string
         * @param coinPair : the coin pair to retrieve 
         */
        private static decimal GetPoloniexCoinPairVolume(string content, string coinPair)
        {
            // parse here
            JObject obj = JObject.Parse(content);
            return Decimal.Parse(obj.SelectToken(coinPair).SelectToken("baseVolume").ToString());
        }

        /**
         * Task to parse the given BitTrex `coinPair` and return its Last price
         * @param content  : the json string
         * @param coinPair : the coin pair to retrieve 
         */
        private static decimal GetBitTrexCoinPairLast(string content, string coinPair)
        {           

            // parse here
            JObject obj = JObject.Parse(content);
            JArray arr = (JArray)obj.SelectToken("result");
            JObject coinDetails = new JObject();

            // loop through the BitTrex results array
            foreach (var coin in arr)
            {
                // find the coinPair
                if (coin.SelectToken("MarketName").ToString().Equals(coinPair))
                {
                    // initialize a new JObject as the coinPair
                    coinDetails = (JObject) coin;
                    break;
                }
            }

            return Decimal.Parse(coinDetails.SelectToken("Last").ToString());            
        }

        /**
         * Task to parse the given BitTrex `coinPair` and return its volume
         * @param content  : the json string
         * @param coinPair : the coin pair to retrieve 
         */
        private static decimal GetBitTrexCoinPairVolume(string content, string coinPair)
        {

            // parse here
            JObject obj = JObject.Parse(content);
            JArray arr = (JArray)obj.SelectToken("result");
            JObject coinDetails = new JObject();

            // loop through the BitTrex results array
            foreach (var coin in arr)
            {
                // find the coinPair
                if (coin.SelectToken("MarketName").ToString().Equals(coinPair))
                {
                    // initialize a new JObject as the coinPair
                    coinDetails = (JObject)coin;
                    break;
                }
            }

            return Decimal.Parse(coinDetails.SelectToken("Volume").ToString());
        }

        /**
         * Task to parse the given CoinSquare `coinPair` and return its Last price
         * @param content  : the json string
         * @param ticker   : the coin pair to retrieve 
         */
        private static decimal GetCoinSquareCoinPairLast(string content, string ticker)
        {

            // parse here
            JObject obj = JObject.Parse(content);
            JArray arr = (JArray)obj.SelectToken("quotes");
            JObject coinDetails = new JObject();

            // loop through the CoinSquare results array
            foreach (var coin in arr)
            {
                // find the coinPair
                if (coin.SelectToken("ticker").ToString().Equals(ticker))
                {
                    // initialize a new JObject as the coinPair
                    coinDetails = (JObject)coin;
                    break;
                }
            }
            
            decimal d = 0.0M;
            if ("USD".Equals(ticker))
            {
                d = Decimal.Parse(coinDetails.SelectToken("last").ToString());
            }
            else
            {            
                // must convert to double to fix decimal notation
                d = Decimal.Parse((Convert.ToDecimal(coinDetails.SelectToken("last").ToString()) / 100000000).ToString());
            }

            return d;
        }

        /**
         * Task to parse the given CoinSquare `coinPair` and return its volume
         * @param content  : the json string
         * @param ticker   : the coin pair to retrieve 
         */
        private static decimal GetCoinSquareCoinPairVolume(string content, string ticker)
        {

            // parse here
            JObject obj = JObject.Parse(content);
            JArray arr = (JArray)obj.SelectToken("quotes");
            JObject coinDetails = new JObject();

            // loop through the CoinSquare results array
            foreach (var coin in arr)
            {
                // find the coinPair
                if (coin.SelectToken("ticker").ToString().Equals(ticker))
                {
                    // initialize a new JObject as the coinPair
                    coinDetails = (JObject)coin;
                    break;
                }
            }

            decimal d = 0.0M;
            if ("USD".Equals(ticker))
            {
                d = Decimal.Parse(coinDetails.SelectToken("volume").ToString());
            }
            else
            {            
                // must convert to double to fix decimal notation
                d = Decimal.Parse((Convert.ToDecimal(coinDetails.SelectToken("volume").ToString()) / 100000000).ToString());
            }

            return d;
        }

    }
}

