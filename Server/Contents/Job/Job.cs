﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public abstract class IJob
    {
        public abstract void Excute();
        public bool Cancel { get; set; }
    }
    public class Job : IJob
    {
        Action _action;
        public Job(Action action)
        {
            _action = action;
        }
        public override void Excute()
        {
            if(Cancel == false)
                _action.Invoke();
        }
    }
    public class Job<T> : IJob
    {
        Action<T> _action;
        T _t;
        public Job(Action<T> action, T t)
        {
            _action = action;
            _t = t;
        }
        public override void Excute()
        {
            if (Cancel == false)
                _action.Invoke(_t);
        }
    }
    public class Job<T1,T2> : IJob
    {
        Action<T1,T2> _action;
        T1 _t1;
        T2 _t2;
        public Job(Action<T1,T2> action, T1 t1, T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }
        public override void Excute()
        {
            if (Cancel == false)
                _action.Invoke(_t1,_t2);
        }
    }
    public class Job<T1, T2, T3> : IJob
    {
        Action<T1, T2, T3> _action;
        T1 _t1;
        T2 _t2;
        T3 _t3;
        public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
        }
        public override void Excute()
        {
            if (Cancel == false)
                _action.Invoke(_t1, _t2, _t3);
        }
    }
}
