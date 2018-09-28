using MySql.Data.MySqlClient;
using System;

namespace CryproProcessor
{

   /**
    * Database Helper
    * Contains all functions to transact with the database
    * @author Vance Field
    * @version 28-Mar-2018
    */ 
    public class DatabaseController 
    {
        // database controller object
        internal static object dbController;

        // mysql connection object
        MySqlConnection conn = null;
         

        /**
         * Initiates a connection to the MySql database
         */ 
        public void InitConnection()
        {

            try
            {
                // init connection to the database
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
                builder.Server = "tradeblazer.io";
                builder.Port = 3306;
                builder.Database = "tradeblazer";
                builder.UserID = "";
                builder.Password = "";

                String connectionStr = builder.ToString();

                builder = null;

                //Console.WriteLine(connectionStr);
                conn = new MySqlConnection(connectionStr);
                Console.WriteLine(DateTime.Now + " - MySqlConnection created.");
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now + " - MySqlConnection not created.");
                Console.WriteLine(e.Message);
            }


        }

        /**
         * Test function.
         * Select all rows from table BTC-ETH
         */ 
        public void SelectAllFromBTC_ETH()
        {
            String query = "SELECT * FROM BTC_ETH;";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            conn.Open();

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                String exchange = (String) reader["Exchange"];
                //DateTime dts = (DateTime) reader["Timestamp"];
                decimal rate = (decimal) reader["Rate"];
                decimal volume = (decimal) reader["Volume"];

                Console.WriteLine("Exchange: " + exchange);
                //Console.WriteLine("Timestamp: " + timestamp);
                Console.WriteLine("Rate: " + rate);
                Console.WriteLine("Volume: " + volume + "\n");

            }
            conn.Close();
        }


        /**
         * Inserts the given coinPair into the given `table`
         * @param table    : the database table
         * @param exchange : the exchange the coin/pair is pulled from
         * @param rate     : the coin/pair rate from the given `exchange`
         * @param volume   : the coin/pair volume from the given `exchange`
         */
        public void InsertCoinPair(string table, string exchange, decimal rate, decimal volume)
        {
            try
            {
                String query = String.Format("insert into {0} (Exchange, Time, Rate, Volume)  values ('{1}', NOW(), '{2}', '{3}')", table, exchange, rate, volume);
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine(DateTime.Now + " - Insert successful: " + table + ", " + exchange);
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " - FAIL: Insert unsuccessful:  " + table + ", " + exchange);
                Console.WriteLine(e.Message);
            }
            
        }

        /**
         * Inserts the given coinPair into the given `table`
         * @param table    : the database table
         * @param exchange : the exchange the coin/pair is pulled from
         * @param rate     : the coin/pair rate from the given `exchange`
         * @param volume   : the coin/pair volume from the given `exchange`
         */
        public void InsertCoinPairRates(string table,  decimal ratePoloniex, decimal rateBittrex, decimal rateCoinsquare)
        {
            try
            {
                String query = String.Format("insert into {0} (Time, Poloniex, Bittrex, Coinsquare)  values (NOW(), '{1}', '{2}', '{3}')", table, ratePoloniex, rateBittrex, rateCoinsquare);
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine(DateTime.Now + " - Insert successful: " + table);
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " - FAIL: Insert unsuccessful:  " + table);
                Console.WriteLine(e.Message);
            }

        }

        /**
         * Inserts the given exchange, coin pair and rate into the MarketState table
         * Used to calculate optimal trade routes
         * @param table         : the database table
         * @param exchange      : the exchange the coin/pair is pulled from
         * @param baseTicker    : the base coin being compared
         * @param tradingTicker : the trading coin being compared
         * @param rate          : the latest coin/pair rate from the given `exchange`
         */
        public void UpdateTradeRoute(string table, string exchange, string baseTicker, string tradingTicker, decimal rate)
        {
            //Console.WriteLine("InsertTradeRoute()");
            try
            {
                String query = String.Format("UPDATE {0} SET Exchange_Rate='{4}' WHERE Exchange='{1}' AND Base_Ticker='{2}' AND Trading_Ticker='{3}'", table, exchange, baseTicker, tradingTicker, rate);
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine(DateTime.Now + " - Update successful: " + table + ", " + exchange);
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " - FAIL: Update unsuccessful:  " + table + ", " + exchange);
                Console.WriteLine(e.Message);
            }
        }
    }
}
