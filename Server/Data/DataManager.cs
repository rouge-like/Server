using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, StatInfo> MonsterDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, SwordInfo> SwordDict { get; private set; } = new Dictionary<int, SwordInfo>();
        public static Dictionary<int, ArrowInfo> ArrowDict { get; private set; } = new Dictionary<int, ArrowInfo>();
        public static Dictionary<int, FireInfo> FireDict { get; private set; } = new Dictionary<int, FireInfo>();
        public static Dictionary<int, LightningInfo> LightningDict { get; private set; } = new Dictionary<int, LightningInfo>();
        public static Dictionary<int, EarthInfo> EarthDict { get; private set; } = new Dictionary<int, EarthInfo>();
        public static Dictionary<int, AirInfo> AirDict { get; private set; } = new Dictionary<int, AirInfo>();
        public static Dictionary<int, IceInfo> IceDict { get; private set; } = new Dictionary<int, IceInfo>();
        public static Dictionary<int, LightInfo> LightDict { get; private set; } = new Dictionary<int, LightInfo>();
        public static Dictionary<int, DarkInfo> DarkDict { get; private set; } = new Dictionary<int, DarkInfo>();
        public static Dictionary<int, PoisonInfo> PoisonDict { get; private set; } = new Dictionary<int, PoisonInfo>();

        public static void LoadData()
        {
            StatDict = LoadJson<StatData, int, StatInfo>("StatData").MakeDict();
            MonsterDict = LoadJson<StatData, int, StatInfo>("MonsterData").MakeDict();
            SwordDict = LoadJson<SwordData, int, SwordInfo>("EquipSwordData").MakeDict();
            ArrowDict = LoadJson<ArrowData, int, ArrowInfo>("EquipArrowData").MakeDict();
            FireDict = LoadJson<FireData, int, FireInfo>("EquipFireData").MakeDict();
            LightningDict = LoadJson<LightningData, int, LightningInfo>("EquipLightningData").MakeDict();
            EarthDict = LoadJson<EarthData, int, EarthInfo>("EquipEarthData").MakeDict();
            AirDict = LoadJson<AirData, int, AirInfo>("EquipAirData").MakeDict();
            IceDict = LoadJson<IceData, int, IceInfo>("EquipIceData").MakeDict();
            LightDict = LoadJson<LightData, int, LightInfo>("EquipLightData").MakeDict();
            DarkDict = LoadJson<DarkData, int, DarkInfo>("EquipDarkData").MakeDict();
            PoisonDict = LoadJson<PoisonData, int, PoisonInfo>("EquipPoisonData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string txt = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(txt);
        }
    }
}
