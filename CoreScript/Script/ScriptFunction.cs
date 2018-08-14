using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Tokens;

namespace CoreScript.Script
{
    public class ScriptValue
    {
        public string DataType { get; set; }
        public object Value { get; set; }
    }


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

        public object Excute(VariableStack stack, IList<ScriptValue> args = null)
        {
            if ((args?.Count ?? 0) != _token.Parameters.Variables.Count) throw new Exception("函数调用缺少参数");

            var index = 0;
            foreach (var variableDefine in _token.Parameters.Variables)
            {
                var svar = args[index++];
                stack.Push(variableDefine.Variable, svar);
            }

            ExcuteBlock(_token.CodeBlock, stack);

            stack.Pop(index);
            return null;
        }

        /// <summary>
        /// 执行代码块
        /// </summary>
        /// <param name="block"></param>
        public void ExcuteBlock(TokenBlockStement block, VariableStack stack)
        {
            var size = stack.Count();
            foreach (var stement in block.Stements)
                if (stement is TokenFunctionCallStement call)
                    ExcuteCall(call, stack);
                else if (stement is TokenAssignment assignment)
                    ExcutAassignment(assignment, stack);
                else if (stement is TokenConditionBlock condition)
                    ExcuteCondition(condition, stack);
            stack.Pop(stack.Count() - size);
        }

        public void ExcuteCondition(TokenConditionBlock stement, VariableStack stack)
        {
            var scriptVariable = ScriptEngine.ReturnValue(stement.Condition, stack);
            if (scriptVariable.DataType != nameof(Boolean)) throw new Exception("非bool值");
            if (scriptVariable.Value is Boolean val)
            {
                if (val)
                {
                    ExcuteBlock(stement.TrueBlock, stack);
                }
                else if (stement.Else != null)
                {
                    ExcuteCondition(stement.Else, stack);
                }
            }
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