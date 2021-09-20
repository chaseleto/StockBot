using Discord.Addons.Interactive;
using System;
using System.Collections.Generic;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;
using System.Linq;
using ServiceStack;
using System.Threading;
using Microsoft.Data.Sqlite;
namespace StockBot.Modules
{
    public class Stock : InteractiveBase
    {
        private CommandService _service;
        public Stock(CommandService service)
        {
            _service = service;

        }

        [Command("buy", RunMode = RunMode.Async), Summary("Retrieves price of stock"),]
        public async Task Buy(String stock)
        {
            bool running = true;
            Symbols symbols = new Symbols();
            Connections conn = new Connections();
            stock = stock.ToUpper();
            double current = 999;
            current = Math.Round(conn.getPrice('c', stock), 3);
            //await Context.Channel.SendMessageAsync(stock.ToUpper() + ": " + conn.getPrice(t, stock));

            while (running == true)
            {
                if (symbols.symbols.ContainsKey(stock))
                {
                    try
                    {
                        if (current == 0 || current == 999)
                        {
                            await Context.Channel.SendMessageAsync("Stock does not exist in current context please contact admin if this is an error.");
                            running = false;
                        }
                        else
                        {
                            Discord.EmbedBuilder builder = new Discord.EmbedBuilder();
                            builder.WithTitle(stock);
                            builder.AddField("Bought at", "$" + current, true);
                            builder.WithCurrentTimestamp();
                            builder.WithColor(Discord.Color.Red);
                            conn.writeDB('c', stock, Context.Channel.Id);
                            await Context.Channel.SendMessageAsync("", false, builder.Build());
                            running = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(1300);
                        }
                        else if (current == 0)
                        {
                            await Context.Channel.SendMessageAsync("Stock does not exist in current context please contact admin if this is an error.");
                            running = false;
                        }
                        else if (e.Message.Contains("19"))
                        {
                            await Context.Channel.SendMessageAsync("Stock already in portfolio!");
                            running = false;
                        }


                        else
                        {
                            await Context.Channel.SendMessageAsync("Other errro: " + e.Message);
                            running = false;
                        }
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        if (current == 0 || current == 999)
                        {
                            await Context.Channel.SendMessageAsync("Stock does not exist in current context please contact admin if this is an error.");
                            running = false;
                        }
                        else
                        {
                            conn.writeDB('s', stock, Context.Channel.Id);
                            Discord.EmbedBuilder builder = new Discord.EmbedBuilder();
                            builder.WithTitle(stock);
                            builder.AddField("Bought at", "$" + conn.getPrice('s', stock), true);
                            builder.WithCurrentTimestamp();
                            builder.WithColor(Discord.Color.Red);
                            await Context.Channel.SendMessageAsync("", false, builder.Build());
                            running = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (current == 0 || current == 999)
                        {
                            await Context.Channel.SendMessageAsync("Stock does not exist in current context please contact admin if this is an error.");
                            running = false;
                        }
                        else if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(1300);
                        }
                        else if (e.Message.Contains("19"))
                        {
                            await Context.Channel.SendMessageAsync("Stock already in portfolio!");
                            running = false;
                        }

                        else
                        {
                            running = false;
                            await Context.Channel.SendMessageAsync(e.Message);
                        }
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [Command("ClearTable", RunMode = RunMode.Async), Summary("Clears the database for this channel -- effectively resets balance and stocks"),]
        public async Task ClearTable()
        {
            Connections conn = new Connections();
            await conn.ResetTable(Context.Channel.Id);
            await Context.Channel.SendMessageAsync("Table cleared..");
        }
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [Command("CreateTable", RunMode = RunMode.Async), Summary("Creates a table for the current channel"),]
        public async Task CreateTable()
        {
            Connections conn = new Connections();
            conn.CreateTable(Context.Channel.Id);
            await Context.Channel.SendMessageAsync($@"Created table for channel ID: {Context.Channel.Id}");
        }
        [Command("Bal", RunMode = RunMode.Async), Summary("Checks balance for given channel")]
        public async Task Bal()
        {
            var msg = await Context.Channel.SendMessageAsync("Getting current balance.. this might take a minute...");
            Connections conn = new Connections();
            await msg.ModifyAsync(msg => msg.Content = "$" + (Math.Round(conn.CheckBal(Context.Channel.Id), 3)));
        }
        [Command("Sell", RunMode = RunMode.Async), Summary("Closes position")]
        public async Task Sell(String stock)
        {
            bool running = true;
            stock = stock.ToUpper();
            Connections conn = new Connections();
            Symbols symbol = new Symbols();
            Char t = 't';
            while (running)
            {
                if (symbol.symbols.ContainsValue(stock))
                {
                    t = 'c';
                }
                else
                {
                    t = 's';
                }
                double profit = conn.Sell(stock, Context.Channel.Id, t);
                double current = Math.Round(conn.getPrice(t, stock), 2);
                if (profit != 123456789666)
                {
                    try
                    {
                        Discord.EmbedBuilder builder = new Discord.EmbedBuilder();
                        builder.WithTitle("SOLD 1 " + stock);
                        builder.AddField("Sold at", "$" + current, false);
                        builder.AddField("Profit made:", "$" + Math.Round(profit, 2), true);
                        builder.WithCurrentTimestamp();
                        builder.WithColor(Discord.Color.Red);
                        await Context.Channel.SendMessageAsync("", false, builder.Build());
                        running = false;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(1300);
                        }
                        else
                        {
                            running = false;
                            await Context.Channel.SendMessageAsync(e.Message);
                        }
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Stock not owned.");
                }
            }

        }
        [Command("price", RunMode = RunMode.Async), Summary("Gets price (testing)")]
        public async Task Price(String stock)
        {
            Connections conn = new Connections();
            Symbols symbol = new Symbols();
            Char t = 't';
            if (symbol.symbols.ContainsValue(stock))
            {
                t = 'c';
            }
            else
            {
                t = 's';
            }
            var current = conn.getPrice(t, stock);
            if (current == 0 || current == 999)
            {
                try
                {
                    Discord.EmbedBuilder builder = new Discord.EmbedBuilder();
                    builder.WithTitle(stock.ToUpper());
                    builder.AddField("Cost: ", current, true);
                    builder.WithCurrentTimestamp();
                    builder.WithColor(Discord.Color.Red);
                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync("Stock now found OR ERROR: " + e.Message, true);
                }
            }
            else
            { }
        }


        public class SecurityData
        {
            public DateTime Timestamp { get; set; }
            public double Close { get; set; }
        }
        public class CoinData
        {
            public DateTime price_timestamp { get; set; }
            public double price { get; set; }
        }

        public class Connections
        {
            private readonly string _AVapiKey;
            private readonly string _NMapiKey;
            private double bal = 100000;
            public Connections()
            {
                this._AVapiKey = Config.bot.AvAPI;
                this._NMapiKey = Config.bot.NmAPI;
            }
            public double getPrice(Char t, String stock)
            {
                Symbols symbols = new Symbols();
                bool running = true;
                Thread.Sleep(1200);
                if (symbols.symbols.ContainsKey(stock))
                {
                    t = 'c';
                    stock = symbols.symbols[stock];
                }
                while (running)
                {
                    try
                    {
                        if (t == 's' || t == 'S')
                        {
                            List<SecurityData> prices = GetDailyPrices(stock);
                            PrimitiveDataFrameColumn<DateTime> date = new PrimitiveDataFrameColumn<DateTime>("Date", prices.Select(sd => sd.Timestamp));
                            PrimitiveDataFrameColumn<double> priceCol = new PrimitiveDataFrameColumn<double>("Close Price", prices.Select(sd => sd.Close));
                            DataFrame df = new DataFrame(date, priceCol);
                            return prices.Select(sd => sd.Close).FirstOrDefault();
                        }
                        else if (t == 'c' || t == 'C')
                        {
                            List<CoinData> prices = GetDailyPricesC(stock);
                            PrimitiveDataFrameColumn<DateTime> date = new PrimitiveDataFrameColumn<DateTime>("Date", prices.Select(sd => sd.price_timestamp));
                            PrimitiveDataFrameColumn<double> priceCol = new PrimitiveDataFrameColumn<double>("Close Price", prices.Select(sd => sd.price));
                            DataFrame df = new DataFrame(date, priceCol);
                            return prices.Select(sd => sd.price).FirstOrDefault();
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("429"))
                        {
                            Thread.Sleep(1300);
                        }
                        else
                        {
                            running = false;
                        }
                        Console.WriteLine(e.Message);
                    }

                }
                return 0;
            }



            public List<SecurityData> GetDailyPrices(string symbol)
            {
                List<SecurityData> prices;
                symbol = symbol.ToUpper();
                string function = "TIME_SERIES_DAILY";
                string connectionString = "https://" + $@"www.alphavantage.co/query?function={function}&symbol={symbol}&apikey={this._AVapiKey}&datatype=csv";
                try { prices = connectionString.GetStringFromUrl().FromCsv<List<SecurityData>>(); }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                    prices = connectionString.GetStringFromUrl().FromCsv<List<SecurityData>>();
                }

                return prices;
            }
            public List<CoinData> GetDailyPricesC(string symbol)
            {
                List<CoinData> prices;
                symbol = symbol.ToUpper();
                String connectionString = "https://" + $@"api.nomics.com/v1/currencies/ticker?key={this._NMapiKey}&ids={symbol}&interval=1d,30d&per-page=100&page=1";
                prices = connectionString.GetStringFromUrl().FromJson<List<CoinData>>();
                return prices;
            }
            public void writeDB(Char t, String stock, ulong channel)
            {

                var connectionStringBuilder = new SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = "./db1.db";

                Symbols symbols = new Symbols();
                if (symbols.symbols.ContainsKey(stock) || symbols.symbols.ContainsValue(stock))
                {
                    t = 'c';

                    stock = symbols.symbols[stock];
                }

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    double price = getPrice(t, stock);
                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = $@"SELECT Balance FROM '{channel}' ORDER BY Balance ASC LIMIT 1;";
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bal = reader.GetDouble(0);
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = $@"INSERT INTO '{channel}' VALUES('{bal - price}','{stock}', NULL, '{price}')";
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();

                    }
                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd1 = connection.CreateCommand();
                        insertCmd1.CommandText = $@"UPDATE '{channel}' SET Balance = '{bal - price}' WHERE Stocks = 'USD';";
                        insertCmd1.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
            public async Task ResetTable(ulong channel)
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = "./db1.db";
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = $@"DELETE FROM '{channel}';";
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = $@"INSERT INTO '{channel}' VALUES('{100000}','USD', NULL, '0')";
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
            public async Task CreateTable(ulong channel)
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = "./db1.db";
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = $@"CREATE TABLE '{channel}' ('Balance' INTEGER NOT NULL, 'Stocks' TEXT NOT NULL UNIQUE, 'ID' INTEGER, 'Bought' INTEGER, PRIMARY KEY('ID')); ";
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    using (var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = $@"INSERT INTO '{channel}' VALUES('{100000}','USD', NULL, '0')";
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();

                    }
                }
            }
            public double GetBalance()
            {
                return bal;
            }
            public double Sell(String stock, ulong channel, Char t)
            {
                Symbols symbols = new Symbols();
                if (symbols.symbols.ContainsKey(stock))
                {
                    t = 'c';
                    stock = symbols.symbols[stock];
                }

                var connectionStringBuilder = new SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = "./db1.db";
                double bought = 123456789666;
                double current = getPrice(t, stock);
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = $@"SELECT x.Bought FROM '{channel}' AS x WHERE Stocks = '{stock}'";
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bought = reader.GetDouble(0);
                        }
                    }
                    var selectCmd3 = connection.CreateCommand();
                    selectCmd3.CommandText = $@"SELECT x.Balance FROM '{channel}' AS x WHERE Stocks = 'USD';";
                    using (var reader3 = selectCmd3.ExecuteReader())
                    {
                        while (reader3.Read())
                        {
                            bal = reader3.GetDouble(0);
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var insertCmd = connection.CreateCommand();
                            insertCmd.CommandText = $@"DELETE FROM '{channel}' WHERE Stocks = '{stock}';";
                            insertCmd.ExecuteNonQuery();
                            var insertCmd1 = connection.CreateCommand();
                            insertCmd1.CommandText = $@"UPDATE '{channel}' SET Balance = '{bal + current}' WHERE Stocks = 'USD';";
                            insertCmd1.ExecuteNonQuery();
                            transaction.Commit();
                            connection.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                }
                if (bought == 123456789666)
                {
                    return 123456789666;
                }

                return current - bought;
            }
            public double CheckBal(ulong channel)
            {
                Symbols symbols = new Symbols();
                double total = 0;
                List<String> stocks = new List<string>();
                List<Double> totals = new List<double>();
                double all = 0;
                var connectionStringBuilder = new SqliteConnectionStringBuilder();
                connectionStringBuilder.DataSource = "./db1.db";
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = $@"SELECT Stocks FROM '{channel}';";
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader.GetString(0).Equals("USD"))) { stocks.Add(reader.GetString(0)); }
                        }
                    }
                    var selectCmd1 = connection.CreateCommand();
                    selectCmd1.CommandText = $@"SELECT x.Bought FROM '{channel}' AS x;";
                    using (var reader1 = selectCmd1.ExecuteReader())
                    {
                        while (reader1.Read())
                        {

                            totals.Add(reader1.GetDouble(0));
                        }
                    }
                    foreach (double num in totals)
                    {
                        all += num;
                    }
                    var selectCmd3 = connection.CreateCommand();
                    selectCmd3.CommandText = $@"SELECT x.Balance FROM '{channel}' AS x WHERE Stocks = 'USD';";
                    using (var reader3 = selectCmd3.ExecuteReader())
                    {
                        while (reader3.Read())
                        {
                            bal = reader3.GetDouble(0);
                        }
                    }
                }
                foreach (String stock in stocks)
                {
                    //Thread.Sleep(1200);
                    if (symbols.symbols.ContainsValue(stock))
                    {
                        total += getPrice('c', stock);
                    }
                    else
                    {
                        total += getPrice('s', stock);
                    }

                }
                total = Math.Round(total, 2);
                return total + bal;
            }
        }
    }
}
