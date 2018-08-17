using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Tokens;

namespace CoreScript.Script
{
    public class ScriptFunction
    {
        private readonly TokenFunctionDefine _token;
        private readonly ScriptEngine _context;

        public ScriptFunction(TokenFunctionDefine token, ScriptEngine scriptEngine)
        {
            _token = token;
            _context = scriptEngine;
            Name = _token.Name;
        }

        public string Name { get; set; }

        /// <summary>
        /// 执行自定义方法
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Excute(VariableStack stack, IList<ScriptValue> args = null)
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

                return ExcuteBlock(_token.CodeBlock, stack);
            }
            finally
            {
                stack.Pop(index);
            }

            return null;
        }

        /// <summary>
        /// 执行代码块
        /// </summary>
        /// <param name="block"></param>
        public ScriptValue ExcuteBlock(TokenBlockStement block, VariableStack stack)
        {
            var size = stack.Count();
            try
            {
                ScriptValue value= null;
                foreach (var stement in block.Stements)
                {
                    if (stement is TokenFunctionCallStement call)
                    {
                        ExcuteCall(call, stack);
                    }
                    else if (stement is TokenAssignment assignment)
                    {
                        ExcutAassignment(assignment, stack);
                    }
                    else if (stement is TokenConditionBlock condition)
                    {
                        value = ExcuteCondition(condition, stack);
                        if (value != null )
                        {
                            break;
                        }
                    }
                    else if (stement is TokenReturnStement returnStement)
                    {
                        value = ExcuteReturnStement(returnStement,stack);
                        
                        break;
                    }
                }
                return value;
            }
            finally
            {
                stack.Pop(stack.Count() - size);
            }
      
        }

        public ScriptValue ExcuteReturnStement(TokenReturnStement stement, VariableStack stack)
        {
            if (stement.Value == null)
            {
                return new ScriptValue()
                {
                    DataType = ScriptType.Void
                };
            }
            else
            {
                return ScriptEngine.ReturnValue(stement.Value,stack);
            }
        }

        /// <summary>
        /// 执行条件语句
        /// </summary>
        /// <param name="stement"></param>
        /// <param name="stack"></param>
        public ScriptValue ExcuteCondition(TokenConditionBlock stement, VariableStack stack)
        {
            ScriptValue retValue= null;
            while (true)
            {
                var scriptVariable = ScriptEngine.ReturnValue(stement.Condition, stack);
                if (scriptVariable.DataType != ScriptType.Boolean) throw new Exception("非bool值");
                if (scriptVariable.Value is Boolean val)
                {
                    if (val)
                    {
                        retValue= ExcuteBlock(stement.TrueBlock, stack); 
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

        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void ExcutAassignment(TokenAssignment stement, VariableStack stack)
        {
            ScriptEngine.ExcuteAssignment(stement, stack);
        }


        /// <summary>
        ///     方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        private object ExcuteCall(TokenFunctionCallStement stement, VariableStack stack)
        {
            var first = stement.CallChain.First();

            var paremeters = new List<ScriptValue>();
            foreach (var value in stement.Parameters)
            {
                var scriptVar = ScriptEngine.ReturnValue(value, stack);
                paremeters.Add(scriptVar);
            }

            if (_context.Functions.ContainsKey(first))
            {
                //调用脚本中定义的函数
                return _context.Functions[first].Excute(stack, paremeters);
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

                return method.Invoke(null, argValues.ToArray());
            }
        }
    }
}