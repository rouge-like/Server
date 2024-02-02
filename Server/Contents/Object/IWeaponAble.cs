using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Contents
{

	public interface IWeaponAble
	{
        public Dictionary<int, Trigon> Trigons { get; set; }
        public Dictionary<int, Passive> Passives { get; set; }
        public Dictionary<EquipType, int> EquipsA { get; set; }
        public Dictionary<EquipType, int> EquipsS { get; set; }
        public Dictionary<EquipType, AdditionalWeaponStat> AdditionalStat { get; set; }
        public PlayerStatInfo PlayerStat { get; set; }
        public Player Target { get; set; }
        public Dictionary<EquipType, int> TotalDamages { get; set; }

        public void SetDamages(EquipType type, int damage);
    }
}

