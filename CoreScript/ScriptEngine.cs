using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Script;
using CoreScript.Tokens;

namespace CoreScript
{
    public class ScriptEngine
    {
        private Dictionary<string, ScriptFunction> _functions;
        private Dictionary<string, ScriptVariable> _variable;

        /// <summary>
        ///     函数定义
        /// </summary>
        public IDictionary<string, ScriptFunction> Functions => _functions;

        /// <summary>
        ///     全局变量
        /// </summary>
        public IDictionary<string, ScriptVariable> Variable => _variable;

        public void Excute(string script)
        {
            var rs = Lexer.Analyzer(script);
            _variable = new Dictionary<string, ScriptVariable>();
            //初始化全局变量
            foreach (var token in rs.Where(it => it.TokenType == TokenType.AssignmentDefine).Cast<TokenAssignment>())
                ExcuteAssignment(token);
            //获取所有函数定义
            _functions = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .Select(it => new ScriptFunction(it, this))
                .ToDictionary(it => it.Name, it => it);

            if (!_functions.ContainsKey("main"))
                throw new Exception("没有找到main方法");

            var main = Functions["main"];
            main.Excute();
        }


        private void ExcuteAssignment(TokenAssignment stement)
        {
            ExcuteAssignment(stement, Variable);
        }


        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        internal void ExcuteAssignment(TokenAssignment stement, IDictionary<string, ScriptVariable> varDict)
        {
            ScriptVariable varItem = null;
            if (stement.Left is TokenVariableDefine define)
            {
                varItem = new ScriptVariable();
                varDict[define.Variable] = varItem;
            }
            else if (stement.Left is TokenVariableRef varRef)
            {
                varItem = varDict[varRef.Variable];
            }

            if (stement.Right is TokenLiteral literal)
            {
                varItem.DataType = literal.DataType;
                varItem.Value = literal.Value;
            }
        }


        /// <summary>
        ///     根据字面量字符串获取Type类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Type GetTypeByString(string type)
        {
            switch (type)
            {
                case nameof(Int32):
                    return typeof(int);
                case nameof(Double):
                    return typeof(double);
                case nameof(String):
                    return typeof(string);
                case nameof(Boolean):
                    return typeof(bool);
                default:
                    throw new Exception("未知的数据类型");
            }
        }
    }
}