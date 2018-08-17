using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Script;
using CoreScript.Tokens;

namespace CoreScript
{
    public class ScriptEngine
    {
        private static Dictionary<string, ScriptFunction> _functions;

        /// <summary>
        ///     函数定义
        /// </summary>
        public static IDictionary<string, ScriptFunction> Functions => _functions;


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
            switch (stement.Left)
            {
                case TokenVariableDefine define:
                    stack.Push(define.Variable, ReturnValue(stement.Right, stack));
                    break;
                case TokenVariableRef varRef:
                    stack.Set(varRef.Variable, ReturnValue(stement.Right, stack));
                    break;
                default:
                    throw new Exception("不支持的变量赋值");
            }
        }

        /// <summary>
        ///     计算带返回值的语句或表达式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static ScriptValue ReturnValue(IReturnValue value, VariableStack stack)
        {
            switch (value)
            {
                case TokenLiteral literal:
                    return new ScriptValue {DataType = literal.DataType, Value = literal.Value};
                case TokenVariableRef varRef:
                    return stack.Get(varRef.Variable);
                case TokenJudgmentExpression expr:
                    return ExcuteJudgment(expr, stack);
                case TokenBinaryExpression binExpr:
                    return SumBinaryExpression(binExpr, stack);
                case TokenFunctionCallStement call:
                    return ExcuteCall(call, stack);
            }

            throw new Exception("不支持的取值方式.");
        }

        /// <summary>
        ///     方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        internal static ScriptValue ExcuteCall(TokenFunctionCallStement stement, VariableStack stack)
        {
            var first = stement.CallChain.First();

            var paremeters = new List<ScriptValue>();
            foreach (var value in stement.Parameters)
            {
                var scriptVar = ScriptEngine.ReturnValue(value, stack);
                paremeters.Add(scriptVar);
            }

            if (Functions.ContainsKey(first))
            {
                //调用脚本中定义的函数
                return Functions[first].Excute(stack, paremeters);
            }
            else
            {
                var argTypes = new List<Type>();
                var argValues = new List<object>();

                foreach (var paremeter in paremeters)
                {
                    var dataType = ScriptType.GetType(paremeter.DataType);
                    argTypes.Add(dataType);
                    argValues.Add(paremeter.Value);
                }

                //调用.Net框架中的方法
                //从程序集中找出名称和参数定义相同的方法
                var methods = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(it => it.DefinedTypes.Where(it1 => it1.Name == first)
                        .Select(t => t.GetMethod(stement.CallChain.Last(), argTypes.ToArray()))
                        .Where(m => m != null)
                    ).ToList();
                if (!methods.Any()) throw new Exception("没有找到对应的方法");
                //   if(methods.Count()>1) throw new Exception("找到多个方法，无法确定调用哪个");
                var method = methods.First();
                var retValue = new ScriptValue();
                var value = method.Invoke(null, argValues.ToArray());
                if (method.ReturnType == typeof(void))
                {
                    return null;
                }
                else
                {
                    return new ScriptValue()
                    {
                        Value = value
                    };
                }
            }
        }
        public static ScriptValue ExcuteJudgment(TokenJudgmentExpression expr, VariableStack stack)
        {
            var left = ReturnValue(expr.Left, stack);
            var right = ReturnValue(expr.Right, stack);
            var result = new ScriptValue
            {
                DataType = nameof(Boolean),
            };

            switch (expr.Operator)
            {
                case JudgmentExpressionType.Equal:
                    result.Value = left.Value.Equals(right.Value);
                    break;
                case JudgmentExpressionType.NotEqual:
                    result.Value = !left.Value.Equals(right.Value);
                    break;
                case JudgmentExpressionType.And:
                    result.Value = (bool) left.Value && (bool) right.Value;
                    break;
                case JudgmentExpressionType.Or:
                    result.Value = (bool) left.Value || (bool) right.Value;
                    break;
            }

            return result;
        }


        /// <summary>
        ///     二元运算符计算
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static ScriptValue SumBinaryExpression(TokenBinaryExpression expr, VariableStack stack)
        {
            var left = ReturnValue(expr.Left, stack);
            var right = ReturnValue(expr.Right, stack);

            switch (expr.Operator)
            {
                #region 二元运算

                case '+':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value + (int) right.Value
                        };
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (int) left.Value + (double) right.Value
                        };
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (double) left.Value + (int) right.Value
                        };
                    else if (left.DataType == ScriptType.String || right.DataType == ScriptType.String)
                        return new ScriptValue
                        {
                            DataType = ScriptType.String,
                            Value = left.Value + right.Value.ToString()
                        };
                    throw new Exception("不支持的运算数据类型");
                case '-':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value - (int) right.Value
                        };
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (int) left.Value - (double) right.Value
                        };
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (double) left.Value - (int) right.Value
                        };
                    throw new Exception("不支持的运算数据类型");
                case '*':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value * (int) right.Value
                        };
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (int) left.Value * (double) right.Value
                        };
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (double) left.Value * (int) right.Value
                        };
                    throw new Exception("不支持的运算数据类型");
                case '/':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value / (int) right.Value
                        };
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (int) left.Value / (double) right.Value
                        };
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Double,
                            Value = (double) left.Value / (int) right.Value
                        };
                    throw new Exception("不支持的运算数据类型");
                case '%':
                    if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Int)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value % (int) right.Value
                        };
                    else if (left.DataType == ScriptType.Int && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (int) left.Value % (double) right.Value
                        };
                    else if (left.DataType == ScriptType.Double && right.DataType == ScriptType.Double)
                        return new ScriptValue
                        {
                            DataType = ScriptType.Int,
                            Value = (double) left.Value % (int) right.Value
                        };
                    throw new Exception("不支持的运算数据类型");
                default:
                    throw new Exception("不支持的运算符");

                #endregion
            }
        }
    }
}