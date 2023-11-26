﻿using System;
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
        public override void Init()
        {
            base.Init();

			StatInfo.Hp = 200;
			StatInfo.MaxHp = 200;
			StatInfo.Speed = 5.0f;
			StatInfo.Attack = 1;

			State = State.Idle;
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

            Random rand = new Random();
			int v = rand.Next(10);

            Item item = ObjectManager.Instance.Add<Item>();
            item.PosInfo.PosX = PosInfo.PosX;
            item.PosInfo.PosY = PosInfo.PosY;
			if (v > 7)
				item.ItemType = ItemType.Food;
			else
				item.ItemType = ItemType.Armor;
			item.value = 10;
            Console.WriteLine($"Monster {Id} DEAD Drop Item {item.Id}");
            Room.Push(Room.EnterRoom, item);

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
		int _searchTick = 0;

		protected virtual void UpdateIdle()
		{
			if (_moveTick > Environment.TickCount)
				return;
			int moveTick = (int)(1000 / Speed);
			_moveTick = Environment.TickCount + moveTick;
            List<Zone> zones = Room.GetAdjacentZones(CellPos, 10);
            // 플레이어 감지
            int d = int.MaxValue;
			if (_searchTick > Environment.TickCount)
				return;
			_searchTick = Environment.TickCount + 1000;
            foreach (Zone zone in zones)
            {
                foreach (Player p in zone.Players)
                {
                    int dx = p.CellPos.x - CellPos.x;
                    int dy = p.CellPos.y - CellPos.y;
					int distance = dx + dy;

                    if (Math.Abs(dx) > 5 || Math.Abs(dy) > 5 || distance > d || p.State == State.Dead)
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
			if (dist > 15.0f || _target.State == State.Dead)
			{
				_target = null;
				State = State.Idle;
				BroadcaseMove();
                return;
			}

			List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos);

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
			Room.Map.MoveObject(this, path[1]);
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

