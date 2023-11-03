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
        public override void Init()
        {
            PosInfo.State = State.Idle;
            StatInfo.Hp = StatInfo.MaxHp;
            _onDead = false;
            _isInvincibility = true;
            Room.PushAfter(3000, AfterInvincibility);
        }
        void AfterInvincibility()
        {
            _isInvincibility = false;

            Trigon t = ObjectManager.Instance.Add<Trigon>();
            t.Owner = this;
            t.Room = Room;
            t.PosInfo = PosInfo;
            t.Info.Name = Id.ToString();
            Drones.Add(t.Id, t);

            Room.Push(Room.EnterRoom, t);
        }
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
        public override void Update()
        {
            base.Update();
        }
        public void EarnItem(Item item)
        {
            Trigon t = ObjectManager.Instance.Add<Trigon>();
            t.Owner = this;
            t.Room = Room;
            t.PosInfo = PosInfo;
            t.Info.Name = Id.ToString();
            Drones.Add(t.Id, t);

            Room.Push(Room.LeaveRoom, item.Id);
            Room.Push(Room.EnterRoom, t);
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

		public override void OnDead(GameObject attacker)
		{
            base.OnDead(attacker);

            if (Room == null)
                return;
            if (_onDead)
                return;

            Info.PosInfo.State = State.Dead;

            S_Die packet = new S_Die();
            packet.ObjectId = Id;
            packet.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, packet);
            Console.WriteLine($"{Id} DEAD");
            _onDead = true;

            foreach (Trigon t in Drones.Values)
            {
                t.Destroy();
            }

            Drones.Clear();

            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            Item item = ObjectManager.Instance.Add<Item>();
            item.PosInfo.PosX = PosInfo.PosX;
            item.PosInfo.PosY = PosInfo.PosY;

            Room.Push(Room.EnterRoom, item);

            Room.PushAfter(2000, Respone);
        }
        public override void Respone()
        {
            base.Respone();
 
            if (Room == null)
                return;
            Room.Push(Room.LeaveRoom, Id);
            Room.Push(Room.EnterRoom, this);
        }
    }
}
