using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Script;
using CoreScript.Tokens;

namespace CoreScript
{
    public class ScriptEngine
    {
        private Dictionary<string, TokenFunctionDefine> _functions;
        private VariableStack stack;

        public void Excute(string script)
        {
            var rs = Lexer.Analyzer(script);
            stack = new VariableStack();
            
            //初始化全局变量
            foreach (var token in rs.Where(it => it.TokenType == TokenType.AssignmentDefine).Cast<TokenAssignment>())
                ExcuteAssignment(token);
            
            //获取所有函数定义
            _functions = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .ToDictionary(it => it.Name, it => it);

            if (!_functions.ContainsKey("main"))
                throw new Exception("没有找到main方法");

            var main = _functions["main"];
            ExcuteFunction(main);
            stack.Pop(stack.Count());
        }


        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void ExcuteAssignment(TokenAssignment stement)
        {
            switch (stement.Left)
            {
                case TokenVariableDefine define:
                    stack.Push(define.Variable, ReturnValue(stement.Right));
                    break;
                case TokenVariableRef varRef:
                    stack.Set(varRef.Variable, ReturnValue(stement.Right));
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
        private ScriptValue ReturnValue(IReturnValue value)
        {
            switch (value)
            {
                case TokenLiteral literal:
                    return new ScriptValue {DataType = literal.DataType, Value = literal.Value};
                case TokenVariableRef varRef:
                    return stack.Get(varRef.Variable);
                case TokenJudgmentExpression expr:
                    return ExcuteJudgment(expr);
                case TokenBinaryExpression binExpr:
                    return SumBinaryExpression(binExpr);
                case TokenFunctionCallStement call:
                    return ExcuteCall(call);
            }

            throw new Exception("不支持的取值方式.");
        }

        /// <summary>
        ///     方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        private ScriptValue ExcuteCall(TokenFunctionCallStement stement)
        {
            var first = stement.CallChain.First();

            var paremeters = new List<ScriptValue>();
            foreach (var value in stement.Parameters)
            {
                var scriptVar = ReturnValue(value);
                paremeters.Add(scriptVar);
            }

            if (_functions.ContainsKey(first)) return ExcuteFunction(_functions[first], paremeters);

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
                    return null;
                return new ScriptValue
                {
                    Value = value
                };
            }
        }

        /// <summary>
        ///     执行自定义方法
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private ScriptValue ExcuteFunction(TokenFunctionDefine _token,
            IList<ScriptValue> args = null)
        {
            if ((args?.Count ?? 0) != _token.Parameters.Variables.Count) throw new Exception("函数调用缺少参数");

            var index = 0;
            try
            {
                //方法参数入栈
                foreach (var variableDefine in _token.Parameters.Variables)
                {
                    var svar = args[index];
                    stack.Push(variableDefine.Variable, svar);
                    index++;
                }

                return ExcuteBlock(_token.CodeBlock);
            }
            finally
            {
                stack.Pop(index);
            }

            return null;
        }


        /// <summary>
        ///     执行条件判断
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ScriptValue ExcuteJudgment(TokenJudgmentExpression expr)
        {
            var left = ReturnValue(expr.Left);
            var right = ReturnValue(expr.Right);
            var result = new ScriptValue
            {
                DataType = nameof(Boolean)
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
        ///     执行代码块
        /// </summary>
        /// <param name="block"></param>
        private ScriptValue ExcuteBlock(TokenBlockStement block)
        {
            var size = stack.Count();
            try
            {
                ScriptValue value = null;
                foreach (var stement in block.Stements)
                    if (stement is TokenFunctionCallStement call)
                    {
                        ExcuteCall(call);
                    }
                    else if (stement is TokenAssignment assignment)
                    {
                        ExcutAassignment(assignment);
                    }
                    else if (stement is TokenConditionBlock condition)
                    {
                        value = ExcuteCondition(condition);
                        if (value != null) break;
                    }
                    else if (stement is TokenReturnStement returnStement)
                    {
                        value = ExcuteReturnStement(returnStement);

                        break;
                    }

                return value;
            }
            finally
            {
                stack.Pop(stack.Count() - size);
            }
        }

        /// <summary>
        ///     执行条件语句
        /// </summary>
        /// <param name="stement"></param>
        /// <param name="stack"></param>
        private ScriptValue ExcuteCondition(TokenConditionBlock stement)
        {
            ScriptValue retValue = null;
            while (true)
            {
                var scriptVariable = ReturnValue(stement.Condition);
                if (scriptVariable.DataType != ScriptType.Boolean) throw new Exception("非bool值");
                if (scriptVariable.Value is bool val)
                {
                    if (val)
                    {
                        retValue = ExcuteBlock(stement.TrueBlock);
                    }
                    else if (stement.Else != null)
                    {
                        stement = stement.Else;
                        continue;
                    }
                }

                break;
            }

            return retValue;
        }


        private ScriptValue ExcuteReturnStement(TokenReturnStement stement)
        {
            if (stement.Value == null)
                return new ScriptValue
                {
                    DataType = ScriptType.Void
                };
            return ReturnValue(stement.Value);
        }


        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void ExcutAassignment(TokenAssignment stement)
        {
            ExcuteAssignment(stement);
        }


        /// <summary>
        ///     二元运算符计算
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ScriptValue SumBinaryExpression(TokenBinaryExpression expr)
        {
            var left = ReturnValue(expr.Left);
            var right = ReturnValue(expr.Right);

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