using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CoreScript.Script
{
    public class VariableStack : IEnumerable<KeyValuePair<string, ScriptValue>>
    {
        private Stack<string> varKeys = null;
        private SortedDictionary<string, ScriptValue> Variables = null;

        public VariableStack()
        {
            Variables = new SortedDictionary<string, ScriptValue>();
            varKeys = new Stack<string>();
        }

        public bool Contains(string key)
        {
            return this.Variables.ContainsKey(key);
        }


        public ScriptValue Get(string key)
        {
            if (!Contains(key)) throw new Exception($"未找到的变量引用：{key}");
            return Variables[key];
        }

        public void Set(string key, ScriptValue value)
        {
            if (!Contains(key)) throw new Exception("变量未声明");

            Variables[key] = value;
        }

        /// <summary>
        /// 将变量插入变量栈
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Push(string key, ScriptValue value)
        {
            if (Contains(key)) throw new Exception("变量已存在");
            varKeys.Push(key);
            this.Variables.Add(key, value);
        }

        public int Count()
        {
            return this.varKeys.Count;
        }

        /// <summary>
        /// 方法结束时弹出尾部变量多次
        /// </summary>
        /// <param name="num"></param>
        public void Pop(int num)
        {
            for (int i = 0; i < num; i++)
            {
                var key = varKeys.Pop();
                this.Variables.Remove(key);
            }
        }

        public IEnumerator<KeyValuePair<string, ScriptValue>> GetEnumerator()
        {
            return this.Variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}