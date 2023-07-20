using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class JobSerializer
    {
		JobTimer _timer = new JobTimer();
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		//bool _flush = false;

		public void PushAfter(int tickAfter, Action action) { PushAfter(tickAfter, new Job(action)); }
		public void PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { PushAfter(tickAfter, new Job<T1>(action, t1)); }
		public void PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
		public void PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }

		public void PushAfter(int tickAfter, IJob job)
        {
			_timer.Push(job, tickAfter);
        }

		public void Push(Action action) { Push(new Job(action)); }
		public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
		public void Push<T1, T2>(Action<T1,T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
		public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }

		public void Push(IJob job)
		{
			// Push 와 Flush 하는 역할의 분리 or 통합
			// 1. 통합 : 처음 Push한 스레드가 Flush 실행
			//bool flush = false;

			lock (_lock)
			{
				_jobQueue.Enqueue(job);
				//if (_flush == false)
					//flush = _flush = true;
			}

			//if (flush)
				//Flush();
			// 2. 분리 : Update 문에서 Flush 실행 - 반응속도 느림
		}
		public void Init()
        {

        }

		public virtual void Update()
        {
			Flush();
        }

		void Flush()
		{
			_timer.Flush();

			while (true)
			{
				IJob job = Pop();
				if (job == null)
					return;

				job.Excute();
			}
		}

		IJob Pop()
		{
			lock (_lock)
			{
				if (_jobQueue.Count == 0)
				{
					//_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
