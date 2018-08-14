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
                ExcuteAssignment(token, stack);
            //获取所有函数定义
            _functions = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .Select(it => new ScriptFunction(it, this))
                .ToDictionary(it => it.Name, it => it);

            if (!_functions.ContainsKey("main"))
                throw new Exception("没有找到main方法");

            var main = Functions["main"];
            main.Excute(stack);
            stack.Pop(stack.Count());
        }


        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        internal static void ExcuteAssignment(TokenAssignment stement, VariableStack stack)
        {
            if (stement.Left is TokenVariableDefine define)
            {
                stack.Push(define.Variable, ReturnValue(stement.Right, stack));
            }
            else if (stement.Left is TokenVariableRef varRef)
            {
                 stack.Set(varRef.Variable,  ReturnValue(stement.Right, stack));
            }
            throw  new Exception("不支持的变量赋值");
        }

        /// <summary>
        ///     取值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static ScriptValue ReturnValue(IReturnValue value, VariableStack stack)
        {
            if (value is TokenLiteral literal)
                return new ScriptValue {DataType = literal.DataType, Value = literal.Value};

            if (value is TokenVariableRef varRef) return stack.Get(varRef.Variable);

            if (value is TokenJudgmentExpression expr)
            {
                var left = ReturnValue(expr.Left, stack);
                var right = ReturnValue(expr.Right, stack);
                return new ScriptValue
                {
                    DataType = nameof(Boolean),
                    Value = left.Value.Equals(right.Value)
                };
            }

            if (value is TokenBinaryExpression binExpr) return SumBinaryExpression(binExpr, stack);

            throw new Exception("不支持的取值方式.");
        }

        private static ScriptValue SumBinaryExpression(TokenBinaryExpression expr, VariableStack stack)
        {
            var left = ReturnValue(expr.Left, stack);
            var right = ReturnValue(expr.Right, stack);
            var rs = new ScriptValue();

            switch (expr.Operator)
            {
                #region 二元运算

                case '+':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                    {
                        rs.DataType = ScriptType.Int;
                        rs.Value = (int) left.Value + (int) right.Value;
                    }
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (int) left.Value + (double) right.Value;
                        rs.DataType = ScriptType.Double;
                    }
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (double) left.Value + (int) right.Value;
                        rs.DataType = ScriptType.Double;
                    }
                    else if (left.DataType == ScriptType.String || right.DataType == ScriptType.String)
                    {
                        rs.Value = left.Value + right.Value.ToString();
                        rs.DataType = ScriptType.String;
                    }

                    break;
                case '-':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                    {
                        rs.DataType = ScriptType.Int;
                        rs.Value = (int) left.Value - (int) right.Value;
                    }
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (int) left.Value - (double) right.Value;
                        rs.DataType = ScriptType.Double;
                    }
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (double) left.Value - (int) right.Value;
                        rs.DataType = ScriptType.Double;
                    }

                    break;
                case '*':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                    {
                        rs.DataType = ScriptType.Int;
                        rs.Value = (int) left.Value * (int) right.Value;
                    }
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (int) left.Value * (double) right.Value;
                        rs.DataType = ScriptType.Double;
                    }
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (double) left.Value * (int) right.Value;
                        rs.DataType = ScriptType.Double;
                    }

                    break;
                case '/':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                    {
                        rs.DataType = ScriptType.Int;
                        rs.Value = (int) left.Value / (int) right.Value;
                    }
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (int) left.Value / (double) right.Value;
                        rs.DataType = ScriptType.Double;
                    }
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (double) left.Value / (int) right.Value;
                        rs.DataType = ScriptType.Double;
                    }

                    break;
                case '%':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                    {
                        rs.DataType = ScriptType.Int;
                        rs.Value = (int) left.Value % (int) right.Value;
                    }
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (int) left.Value % (double) right.Value;
                        rs.DataType = ScriptType.Int;
                    }
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                    {
                        rs.Value = (double) left.Value % (int) right.Value;
                        rs.DataType = ScriptType.Int;
                    }

                    break;

                #endregion
            }

            return rs;
        }
    }
}