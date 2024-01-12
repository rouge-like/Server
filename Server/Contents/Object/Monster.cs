using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Contents
{
	public class Monster : GameObject
	{
		public Monster()
		{
			ObjectType = GameObjectType.Monster;
		}
		public bool IsMetal;
		public override void Init()
		{
			base.Init();

			State = State.Idle;
			Info.Prefab = StatInfo.Level - 1;
			StatInfo.Hp = StatInfo.MaxHp;
			if (Info.Prefab == 2 || Info.Prefab == 3)
				IsMetal = true;
			else
				IsMetal = false;
			Update();
        }
        protected override void OnDead(GameObject attacker)
        {
			base.OnDead(attacker);
            // 아이템을 떨군
            if (Room == null)
                return;
            if (State == State.Dead)
                return;
	
            Info.PosInfo.State = State.Dead;

            S_Die packet = new S_Die();
            packet.ObjectId = Id;
            packet.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, packet);

            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
			for(int i = 0; i < StatInfo.Level; i++)
			{
                Random rand = new Random();
                int v = rand.Next(10);

                Item item = ObjectManager.Instance.Add<Item>();
                item.PosInfo.PosX = PosInfo.PosX;
                item.PosInfo.PosY = PosInfo.PosY;
                if (v > 7)
                    item.Info.Prefab = 0;
                else
                    item.Info.Prefab = 1;
                item.value = 10;
                Room.Push(Room.EnterRoom, item);
            }

            Room.PushAfter(1000, Respone);
        }

		public override void Respone()
		{
			base.Respone();
            Room.Push(Room.LeaveRoom, Id);
            //Room.PushAfter(500, Room.EnterRoom, this);
        }
		IJob _job;
        public override void Update()
        {
			_isInvincibility = false;
			
            base.Update();
			switch (State)
			{
				case State.Idle:
					UpdateIdle();
					break;
				case State.Moving:
					UpdateMoving();
					break;
				case State.Skill:
					UpdateSkill();
					break;
				case State.Dead:
					UpdateDead();
					break;
			}
			// 목표 지정 및 추격
			// 일정 거리 이후 추격 취소
			if(Room != null)
				_job = Room.PushAfter(200, Update);
        }

		Player _target;
		long _searchTick = 0;
        Dir GetRightDir(Dir dir)
        {
            switch (dir)
            {
                case Dir.Up:
                    return Dir.Upright;
                case Dir.Down:
                    return Dir.Downleft;
                case Dir.Right:
                    return Dir.Downright;
                case Dir.Left:
                    return Dir.Upleft;
                case Dir.Upright:
                    return Dir.Right;
                case Dir.Upleft:
                    return Dir.Up;
                case Dir.Downright:
                    return Dir.Down;
                case Dir.Downleft:
                    return Dir.Left;

            }
            return dir;
        }
        protected virtual void UpdateIdle()
		{
			if (_moveTick > (Environment.TickCount & Int32.MaxValue))
				return;
			int moveTick = (int)(1000 / Speed);
			_moveTick = Environment.TickCount & Int32.MaxValue + moveTick;
            List<Zone> zones = Room.GetAdjacentZones(CellPos, 10);
            // 플레이어 감지
            int d = int.MaxValue;
			if (_searchTick > (Environment.TickCount & Int32.MaxValue))
				return;
			_searchTick = Environment.TickCount + 1000;
            foreach (Zone zone in zones)
            {
                foreach (Player p in zone.Players)
                {
                    int dx = p.CellPos.x - CellPos.x;
                    int dy = p.CellPos.y - CellPos.y;
					int distance = dx + dy;

                    if (Math.Abs(dx) > 7 || Math.Abs(dy) > 7 || distance > d || p.State == State.Dead)
                        continue;

					_target = p;
					d = distance;
                }
            }

			if (_target == null)
				return;
			State = State.Moving;
			_searchTick = 0;
        }
		long _moveTick = 0;
		protected virtual void UpdateMoving()
		{
			if (_target == null || _target.Room != Room)
            {
                _target = null;
                State = State.Idle;
                BroadcaseMove();

                return;
            }
			Vector2Int dir = _target.CellPos - CellPos;
            float dist = dir.magnitude;
			if (dist > 10.0f || _target.State == State.Dead)
			{
				_target = null;
				State = State.Idle;
				BroadcaseMove();
                return;
			}

            if (dist <= 1.5)
            {
                State = State.Skill;
                return;
            }

            Dir destDir = GetDirFromVec(dir);
            Vector2Int destPos = GetFrontCellPos(destDir);

            /*if (Room.Map.CanGo(destPos))
            {
                Dir = destDir;
                Room.Map.MoveObject(this, destPos);
            }*/
            
            for (int i = 0; i < 8; i++)
			{
                if (Room.Map.CanGo(destPos))
                {
                    Dir = destDir;
                    Room.Map.MoveObject(this, destPos);
					break;
                }
				else
				{
                    destDir = GetRightDir(destDir);
                    destPos = GetFrontCellPos(destDir);
				}

            }

            /*List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos);

			if (path.Count < 2 || path.Count > 20)
			{
				_target = null;
				State = State.Idle;
				BroadcaseMove();

                return;
			}
			if (dist <= 1.5)
			{
				State = State.Skill;
				return;
			}
			Dir = GetDirFromVec(path[1] - CellPos);
			Room.Map.MoveObject(this, path[1]);*/

            BroadcaseMove();
        }

		void BroadcaseMove()
		{
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
			Room.Broadcast(CellPos, movePacket);
        }
		protected virtual void UpdateSkill()
		{
            if (_target == null || _target.Room != Room || _target.State == State.Dead)
            {
				_target = null;
                State = State.Idle;
                BroadcaseMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            float dist = dir.magnitude;
            Dir = GetDirFromVec(dir);

            if (dist > 1.5f)
			{
				State = State.Moving;
                BroadcaseMove();
                return;
			}
			_target.OnDamaged(this, StatInfo.Attack);
			BroadcaseMove();
        }
		protected virtual void UpdateDead()
		{
			//Console.WriteLine("OnDead");
		}
        public override void OnDamaged(GameObject attacker, int damage)
        {
            if (_isInvincibility || State == State.Dead)
                return;
			if(_job != null)
			{
				_job.Cancel = true;
				_job = null;
			}
            _job = Room.PushAfter(200, Update);
            base.OnDamaged(attacker, damage);
            //_isInvincibility = true;
            //Console.WriteLine($"OnDamaged Monster by {attacker.Id}");
        }
    }
}

