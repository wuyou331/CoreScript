using System;

namespace CoreScript.Script
{
    /// <summary>
    /// 脚本支持的基础数据类型
    /// </summary>
    public static class ScriptType
    {
        internal const  string Int = nameof(Int32);
        internal const string Double = nameof(Double);
        internal const string String = nameof(String);
        internal const string Boolean = nameof(Boolean);

        /// <summary>
        ///     根据字面量字符串获取Type类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetType(string type)
        {
            switch (type)
            {
                case ScriptType.Int:
                    return typeof(int);
                case ScriptType.Double:
                    return typeof(double);
                case ScriptType.String:
                    return typeof(string);
                case ScriptType.Boolean:
                    return typeof(bool);
                default:
                    throw new Exception("未知的数据类型");
            }
        }

    }
}