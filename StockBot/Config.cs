using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StockBot
{
    public class Config
    {
        #testing13
          #testing14
        private const string configFile = "Config.json";
        private const string configFolder = "Resources";
        public static string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static BotConfig bot;

        static Config()
        {
            if (!Directory.Exists(path + "\\" + configFolder))
                Directory.CreateDirectory(path + "\\" + configFolder);

            if (!File.Exists(path + "\\" + configFolder + "\\" + configFile))
            {
                bot = new BotConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(path + "\\" + configFolder + "\\" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(path + "\\" + configFolder + "\\" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }


        public struct BotConfig
        {
            public string token;
            public string cmdPrefix;
            public string AvAPI;
            public string NmAPI;
        }
    }
}
