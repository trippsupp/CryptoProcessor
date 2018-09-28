using System;
using System.Timers;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CryproProcessor
{    

    /**
     * CryptoProcessor - Pulls crypto prices and inserts them into our database
     * @author Vance Field
     * @version 6-Feb-2018
     */
    class Program
    {

        static void Main(string[] args)
        {
            Prices prices = new Prices();
            prices.StartTimer();
        }

       
    }
}
