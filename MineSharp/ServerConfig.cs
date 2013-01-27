using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineSharp
{
    public class ServerConfig
    {
        public string IP = "";
        public int Port = 25565;
        public string World = "world";
        public bool Query = false;
        public bool Flight = false;
        public LevelGenerator WorldType = LevelGenerator.Debug;
        public bool RCON = false;
        public string Seed = "";
        public int MaxWorldHeight = 256;
        public bool NPCs = true;
        public bool Whitelist = false;
        public bool Animals = true;
        public bool Monsters = true;
        public string TexturePack = "";
        public bool Online = true;
        public bool PVP = true;
        public Gamemode Gamemode = Gamemode.Classic;
        public byte MaxPlayers = 20;
        public int ViewDistance = 10;
        public string MOTD = "A Minecraft Server";
    }
}
