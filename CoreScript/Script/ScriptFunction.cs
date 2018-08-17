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
        public ScriptValue Excute(VariableStack stack, IList<ScriptValue> args = null)
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
                       ScriptEngine. ExcuteCall(call, stack);
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


  
    }
}