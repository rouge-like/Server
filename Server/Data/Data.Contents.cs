using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
	[Serializable]
	public class StatData : ILoader<int, StatInfo>
	{
		public List<StatInfo> stats = new List<StatInfo>();

		public Dictionary<int, StatInfo> MakeDict()
		{
			Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
			foreach (StatInfo stat in stats)
            {
				dict.Add(stat.Level, stat);
			}
				
			return dict;
		}
	}

    [Serializable]
    public class SwordInfo
    {
        public int level;
        public int attack;
        public int speed;
		public int range;
		public int cooltime;
		public int number;
    }
	[Serializable]
	public class SwordData : ILoader<int, SwordInfo>
	{
		public List<SwordInfo> swords = new List<SwordInfo>();

		public Dictionary<int, SwordInfo> MakeDict()
		{
			Dictionary<int, SwordInfo> dict = new Dictionary<int, SwordInfo>();
			foreach (SwordInfo sword in swords)
				dict.Add(sword.level, sword);

			return dict;
		}
	}

    [Serializable]
    public class ArrowInfo
    {
        public int level;
        public int attack;
        public int speed;
        public int range;
		public int cooltime;
        public int number;
    }
    [Serializable]
    public class ArrowData : ILoader<int, ArrowInfo>
    {
        public List<ArrowInfo> arrows = new List<ArrowInfo>();

        public Dictionary<int, ArrowInfo> MakeDict()
        {
            Dictionary<int, ArrowInfo> dict = new Dictionary<int, ArrowInfo>();
            foreach (ArrowInfo arrow in arrows)
                dict.Add(arrow.level, arrow);

            return dict;
        }
    }

	[Serializable]
	public class FireInfo
	{
		public int level;
		public int attack;
        public List<List<int>> area;
		public int cooltime;
    }
	[Serializable]
	public class FireData : ILoader<int, FireInfo>
	{
		public List<FireInfo> fires = new List<FireInfo>();

		public Dictionary<int, FireInfo> MakeDict()
		{
			Dictionary<int, FireInfo> dict = new Dictionary<int, FireInfo>();
			foreach (FireInfo fire in fires)
				dict.Add(fire.level, fire);

			return dict;
		}
	}

	[Serializable]
	public class LightningInfo
	{
		public int level;
		public int attack;
		public int speed;
		public int range;
		public int cooltime;
		public int duration;
		public int number;
	}
	[Serializable]
	public class LightningData : ILoader<int, LightningInfo>
	{
		public List<LightningInfo> lightnings = new List<LightningInfo>();

		public Dictionary<int, LightningInfo> MakeDict()
		{
			Dictionary<int, LightningInfo> dict = new Dictionary<int, LightningInfo>();
			foreach (LightningInfo lightning in lightnings)
				dict.Add(lightning.level, lightning);

			return dict;
		}
	}

    [Serializable]
    public class EarthInfo
    {
        public int level;
        public int attack;
        public int speed;
        public int range;
        public int cooltime;
        public int number;
    }
    [Serializable]
    public class EarthData : ILoader<int, EarthInfo>
    {
        public List<EarthInfo> earths = new List<EarthInfo>();

        public Dictionary<int, EarthInfo> MakeDict()
        {
            Dictionary<int, EarthInfo> dict = new Dictionary<int, EarthInfo>();
            foreach (EarthInfo earth in earths)
                dict.Add(earth.level, earth);

            return dict;
        }
    }

    [Serializable]
    public class AirInfo
    {
        public int level;
        public int range;
        public int attack;
		public int speed;
        public int cooltime;
    }
    [Serializable]
    public class AirData : ILoader<int, AirInfo>
    {
        public List<AirInfo> airs = new List<AirInfo>();

        public Dictionary<int, AirInfo> MakeDict()
        {
            Dictionary<int, AirInfo> dict = new Dictionary<int, AirInfo>();
            foreach (AirInfo air in airs)
                dict.Add(air.level, air);

            return dict;
        }
    }

	[Serializable]
	public class IceInfo
	{
		public int level;
		public int attack;
        public int cooltime;
		public int attackcount;
		public int number;
        public List<List<int>> area;
    }
	[Serializable]
	public class IceData : ILoader<int, IceInfo>
	{
		public List<IceInfo> ices = new List<IceInfo>();

		public Dictionary<int, IceInfo> MakeDict()
		{
			Dictionary<int, IceInfo> dict = new Dictionary<int, IceInfo>();
			foreach (IceInfo ice in ices)
				dict.Add(ice.level, ice);

			return dict;
		}
	}

	[Serializable]
	public class LightInfo
	{
		public int level;
		public int attack;
		public int cooltime;
		public int number;
	}
	[Serializable]
	public class LightData : ILoader<int, LightInfo>
	{
		public List<LightInfo> lights = new List<LightInfo>();

		public Dictionary<int, LightInfo> MakeDict()
		{
			Dictionary<int, LightInfo> dict = new Dictionary<int, LightInfo>();
			foreach (LightInfo light in lights)
				dict.Add(light.level, light);

			return dict;
		}
	}

	[Serializable]
	public class DarkInfo
	{
        public int level;
        public int attack;
        public int speed;
        public int range;
        public int cooltime;
        public int duration;
        public int number;
    }
	[Serializable]
	public class DarkData : ILoader<int, DarkInfo>
	{
		public List<DarkInfo> darks = new List<DarkInfo>();

		public Dictionary<int, DarkInfo> MakeDict()
		{
			Dictionary<int, DarkInfo> dict = new Dictionary<int, DarkInfo>();
			foreach (DarkInfo dark in darks)
				dict.Add(dark.level, dark);

			return dict;
		}
	}

	[Serializable]
	public class PoisonInfo
	{
		public int level;
		public int attack;
		public int attackcount;
	}
	[Serializable]
	public class PoisonData : ILoader<int, PoisonInfo>
	{
		public List<PoisonInfo> poisons = new List<PoisonInfo>();

		public Dictionary<int, PoisonInfo> MakeDict()
		{
			Dictionary<int, PoisonInfo> dict = new Dictionary<int, PoisonInfo>();
			foreach (PoisonInfo poison in poisons)
				dict.Add(poison.level, poison);

			return dict;
		}
	}

	[Serializable]
    public class AdditionalWeaponStat
    {
        public int attack;
        public int speed;
        public int range;
        public int cooltime;
        public int duraion;

        public AdditionalWeaponStat()
        {
            this.attack = 100;
            this.speed = 100;
            this.range = 100;
            this.cooltime = 100;
            this.duraion = 100;
        }
    }

    [Serializable]
    public class Weapons
    {
        public WeaponStat sword;
        public WeaponStat arrow;
        public WeaponStat fire;
        public WeaponStat lightning;
        public WeaponStat earth;
        public WeaponStat air;
        public WeaponStat ice;
        public WeaponStat light;
        public WeaponStat dark;
        public WeaponStat poison;
    }

    [Serializable]
    public class WeaponStat
    {
        public List<int> gems;
        public int maximum;
    }
}
