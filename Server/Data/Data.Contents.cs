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
				stat.Hp = stat.MaxHp;
				dict.Add(stat.Level, stat);
			}
				
			return dict;
		}
	}

	[Serializable]
	public class Skill
    {
		public int id;
		public string name;
		public float cooldown;
		public int damage;
		public SkillType skillType;
		public ProjectileInfo projectile;
		public AreaInfo area;
		public CircleInfo circle;

    }

	public class ProjectileInfo
    {
		public string name;
		public float speed;
		public int range;
		public string prefab;
    }

	public class AreaInfo
    {
		public List<List<int>> posList;
    }

	public class CircleInfo
	{
		public int len;
		public List<List<int>> posList;
	}
	[Serializable]
	public class SkillData : ILoader<int, Skill>
	{
		public List<Skill> skills = new List<Skill>();

		public Dictionary<int, Skill> MakeDict()
		{
			Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
			foreach (Skill skill in skills)
				dict.Add(skill.id, skill);
			return dict;
		}
	}
}
