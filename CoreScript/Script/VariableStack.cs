using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CoreScript.Script
{
    /// <summary>
    /// 变量栈
    /// </summary>
    public class VariableStack : IEnumerable<KeyValuePair<string, ScriptValue>>
    {
        private readonly Stack<string> _keys = null;
        private readonly SortedDictionary<string, ScriptValue> _variables = null;

        public VariableStack()
        {
            _variables = new SortedDictionary<string, ScriptValue>();
            _keys = new Stack<string>();
        }


        public bool Contains(string key)
        {
            return this._variables.ContainsKey(key);
        }

        /// <summary>
        /// 获取一个变量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ScriptValue Get(string key)
        {
            if (!Contains(key)) throw new Exception($"未找到的变量引用：{key}");
            return _variables[key];
        }
        /// <summary>
        /// 为变量赋值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, ScriptValue value)
        {
            if (!Contains(key)) throw new Exception("变量未声明");

            _variables[key] = value;
        }

        /// <summary>
        /// 将变量插入变量栈，用于声明变量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Push(string key, ScriptValue value)
        {
            if (Contains(key)) throw new Exception("变量已存在");
            _keys.Push(key);
            this._variables.Add(key, value);
        }

        public int Count()
        {
            return this._keys.Count;
        }

        /// <summary>
        /// 方法结束时弹出尾部变量多次
        /// </summary>
        /// <param name="num"></param>
        public void Pop(int num)
        {
            for (int i = 0; i < num; i++)
            {
                var key = _keys.Pop();
                this._variables.Remove(key);
            }
        }


        public IEnumerator<KeyValuePair<string, ScriptValue>> GetEnumerator()
        {
            return this._variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}