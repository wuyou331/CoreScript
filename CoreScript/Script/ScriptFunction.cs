using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Tokens;

namespace CoreScript.Script
{
    public class ScriptVariable
    {
        public string DataType { get; set; }
        public object Value { get; set; }
    }

    public class ScriptFunction
    {
        private readonly ScriptEngine _context;

        private readonly IDictionary<string, ScriptVariable> _localVars = new Dictionary<string, ScriptVariable>();
        private readonly TokenFunctionDefine _token;

        public ScriptFunction(TokenFunctionDefine token, ScriptEngine context)
        {
            _token = token;
            _context = context;
            Name = _token.Name;
        }

        public string Name { get; set; }

        public object Excute(IList<object> args)
        {
            foreach (var stement in _token.CodeBlock.Stements)
                if (stement is TokenFunctionCallStement call)
                    CallFunction(call);
                else if (stement is TokenAssignment assignment)
                    CallFunction(assignment);
            return null;
        }

        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void CallFunction(TokenAssignment stement)
        {
            _context.ExcuteAssignment(stement, _localVars);
        }


        /// <summary>
        ///     方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        private object CallFunction(TokenFunctionCallStement stement)
        {
            var first = stement.CallChain.First();

            var argTypes = new List<Type>();
            var paremeters = new List<object>();
            foreach (var value in stement.Parameters)
                if (value is TokenLiteral literal)
                {
                    var dataType = _context.GetTypeByString(literal.DataType);
                    argTypes.Add(dataType);
                    paremeters.Add(literal.Value);
                }
                else if (value is TokenVariableRef varRef)
                {
                    ScriptVariable vars = null;
                    //先找局部变量，在找全局变量
                    if (_localVars.ContainsKey(varRef.Variable))
                    {
                        vars = _localVars[varRef.Variable];
                    }
                    else if (_context.Variable.ContainsKey(varRef.Variable))
                    {
                        vars = _context.Variable[varRef.Variable];
                    }
       

                    var dataType = _context.GetTypeByString(vars.DataType);
                    argTypes.Add(dataType);
                    paremeters.Add(vars.Value);
                }

            if (_context.Functions.ContainsKey(first))
            {

                return _context.Functions[first].Excute(paremeters);
            }



            //从程序集中找出名称和参数定义相同的方法
            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(it => it.DefinedTypes.Where(it1 => it1.Name == first)
                    .Select(t => t.GetMethod(stement.CallChain.Last(), argTypes.ToArray()))
                    .Where(m => m != null)
                ).ToList();
            if (!methods.Any()) throw new Exception("没有找到对应的方法");
            //   if(methods.Count()>1) throw new Exception("找到多个方法，无法确定调用哪个");
            var method = methods.First();

            return method.Invoke(null, paremeters.ToArray());
        }
    }
}