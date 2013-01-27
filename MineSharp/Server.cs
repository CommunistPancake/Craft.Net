using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Craft.Net;
using Craft.Net.Data;
using Craft.Net.Data.Generation;
using Craft.Net.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MineSharp
{
    public class Server
    {
        public ServerConfig Config;
        public IWorldGenerator Generator;

        private IPAddress ServerIP;
        private int ServerPort;
        private MinecraftServer MCServer;

        public Server()
        {
            Config = new ServerConfig();
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            //Parse the config file. If the config isn't valid, throw an error.
            if (File.Exists("config.json"))
                try { Config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText("config.json"), settings); }
                catch (JsonException e) { Logger.Log("Config file is invalid!", LogType.Error); }
            else
                File.WriteAllText("config.json", JsonConvert.SerializeObject(Config, Formatting.Indented));

            //Parse the IP from the config file.
            if (!string.IsNullOrWhiteSpace(Config.IP))
            {
                if (!IPAddress.TryParse(Config.IP, out ServerIP))
                    ServerIP = IPAddress.Any;
            }
            else
                ServerIP = IPAddress.Any;
            ServerPort = Config.Port;
            MCServer = new MinecraftServer(new IPEndPoint(ServerIP, ServerPort));

            //Choose the world generator based on the config file.
            if (string.IsNullOrWhiteSpace(Config.World))
                Config.World = "world";
            switch (Config.WorldType)
            {
                case LevelGenerator.Debug:
                    Generator = new DebugGenerator();
                    break;
                case LevelGenerator.Flatland:
                    Generator = new FlatlandGenerator();
                    break;
                default:
                    //No default generator yet!
                    Generator = new DebugGenerator();
                    break;
            }
            MCServer.AddLevel(new Level(Generator, Config.World));

            MCServer.Settings.MotD = Config.MOTD;
            MCServer.Settings.MaxPlayers = Config.MaxPlayers;
            MCServer.Settings.OnlineMode = Config.Online;

            MCServer.Start();
        }
    }
}
