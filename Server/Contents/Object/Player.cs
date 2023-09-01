using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public Dictionary<int, Trigon> Drones { get; set; } = new Dictionary<int, Trigon>();

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }
        IJob _job;
        public void UpdatePassive(int skillId)
		{
            if (Room == null)
                return;

            Skill skillData;
            
            if (DataManager.SkillDict.TryGetValue(skillId, out skillData))
            {
                int cooldown = (int)skillData.cooldown * 1000;
                _job = Room.PushAfter(cooldown, UpdatePassive, skillId);

                switch (skillData.skillType)
                {
                    case SkillType.SkillArea:
                        {
                            foreach (List<int> list in skillData.area.posList)
                            {
                                Area area = ObjectManager.Instance.Add<Area>();

                                if (area == null)
                                    continue;


                                area.Owner = this;
                                area.Data = skillData;
                                area.Info.Name = $"Area_{area.Id}";
                                area.PosInfo.PosX = CellPos.x + list[0];
                                area.PosInfo.PosY = CellPos.y + list[1];

                                if (area.PosInfo.PosX < 0 || area.PosInfo.PosX >= Room.Map.SizeX || area.PosInfo.PosY < 0 || area.PosInfo.PosY >= Room.Map.SizeY)
                                    continue;

                                Room.Push(Room.EnterRoom, area);
                            }
                        }
                        break;
                    case SkillType.SkillCircler:
                        {
                            Circler circler = ObjectManager.Instance.Add<Circler>();

                            if (circler == null)
                                return;
                           
                            circler.Owner = this;
                            circler.Data = skillData;
                            circler.Info.Name = $"Circler_{circler.Id}";
                            circler.PosInfo.PosX = CellPos.x;
                            circler.PosInfo.PosY = CellPos.y;


                            if (circler.PosInfo.PosX < 0 || circler.PosInfo.PosX >= Room.Map.SizeX || circler.PosInfo.PosY < 0 || circler.PosInfo.PosY >= Room.Map.SizeY)
                                return;

                            //Drones.Add(circler);
                            Room.Push(Room.EnterRoom, circler);
                        }
                        break;
                }        
            }
        }

		public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            Console.WriteLine($"{Info.Name} On Damaged, HP : {StatInfo.Hp} by {attacker.Info.Name}");
        }
		public override void OnDead(GameObject attacker)
		{
            base.OnDead(attacker);
            foreach(Trigon t in Drones.Values)
            {
                t.Destroy();
                Room.LeaveRoom(t.Id);
            }

            Drones.Clear();

            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
		}
	}
}
