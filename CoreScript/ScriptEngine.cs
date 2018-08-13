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

        /// <summary>
        ///     函数定义
        /// </summary>
        public IDictionary<string, ScriptFunction> Functions => _functions;

    

        public void Excute(string script)
        {
            var rs = Lexer.Analyzer(script);
            var stack = new VariableStack();
            //初始化全局变量
            foreach (var token in rs.Where(it => it.TokenType == TokenType.AssignmentDefine).Cast<TokenAssignment>())
            {
                ExcuteAssignment(token,stack);
            }
            //获取所有函数定义
            _functions = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .Select(it => new ScriptFunction(it,this))
                .ToDictionary(it => it.Name, it => it);

            if (!_functions.ContainsKey("main"))
                throw new Exception("没有找到main方法");

            var main = Functions["main"];
            main.Excute(stack);
        }





        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        internal static void ExcuteAssignment(TokenAssignment stement, VariableStack stack)
        {
            ScriptValue varItem = null;
            if (stement.Left is TokenVariableDefine define)
            {
                varItem = new ScriptValue();
                stack.Push(define.Variable,varItem);
            }
            else if (stement.Left is TokenVariableRef varRef)
            {
                varItem = stack.Get(varRef.Variable) ;
            }

            var right = ReturnValue(stement.Right,stack);
            varItem.DataType = right.DataType;
            varItem.Value = right.Value;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static ScriptValue ReturnValue(IReturnValue value, VariableStack stack)
        {
            if (value is TokenLiteral literal)
            {
                return new ScriptValue() { DataType = literal.DataType, Value = literal.Value };
            }
            else if (value is TokenVariableRef varRef)
            {
                return stack.Get(varRef.Variable);
            }
            else if (value is TokenJudgmentExpression expr)
            {
                var left = ReturnValue(expr.Left, stack);
                var right = ReturnValue(expr.Right, stack);
                return new ScriptValue()
                {
                    DataType = nameof(Boolean),
                    Value = left.Value.Equals(right.Value)
                };
            }

            throw new Exception("不支持的取值方式.");
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