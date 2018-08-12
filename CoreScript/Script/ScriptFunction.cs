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
        private readonly TokenFunctionDefine _token;

        /// <summary>
        ///     函数内的局部变量
        /// </summary>
        private  IDictionary<string, ScriptVariable> _variable = null;

        public ScriptFunction(TokenFunctionDefine token, ScriptEngine context)
        {
            _token = token;
            _context = context;
            Name = _token.Name;
        }

        public string Name { get; set; }

        public object Excute(IList<ScriptVariable> args=null)
        {
            if ((args?.Count ?? 0) != _token.Parameters.Variables.Count) throw new Exception("函数调用缺少参数");
            _variable= new Dictionary<string, ScriptVariable>();
            var index = 0;
            foreach (var variableDefine in _token.Parameters.Variables)
            {
                var svar = args[index++];
                _variable[variableDefine.Variable] = svar;
            }

            foreach (var stement in _token.CodeBlock.Stements)
                if (stement is TokenFunctionCallStement call)
                    ExcuteCall(call);
                else if (stement is TokenAssignment assignment)
                    ExcutAassignment(assignment);
            return null;
        }

        /// <summary>
        ///     变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void ExcutAassignment(TokenAssignment stement)
        {
            _context.ExcuteAssignment(stement, _variable);
        }


        /// <summary>
        ///     方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        private object ExcuteCall(TokenFunctionCallStement stement)
        {
            var first = stement.CallChain.First();

            var paremeters = new List<ScriptVariable>();
     
            foreach (var value in stement.Parameters)
            {
                if (value is TokenLiteral literal)
                {
                    paremeters.Add(new ScriptVariable() {DataType = literal.DataType, Value = literal.Value});
                }
                else if (value is TokenVariableRef varRef)
                {
                    ScriptVariable vars = null;
                    //先找局部变量，在找全局变量
                    if (_variable.ContainsKey(varRef.Variable))
                        vars = _variable[varRef.Variable];
                    else if (_context.Variable.ContainsKey(varRef.Variable)) vars = _context.Variable[varRef.Variable];
                    
                    if(vars==null) throw  new Exception("未找到的变量引用");
                    var dataType = _context.GetTypeByString(vars.DataType);
                    paremeters.Add(vars);
                }
            }

            if (_context.Functions.ContainsKey(first))
            {
                //调用脚本中定义的函数
                return _context.Functions[first].Excute(paremeters);
            }
            else
            {

                var argTypes = new List<Type>();
                var argValues = new List<object>();

                foreach (var paremeter in paremeters)
                {

                    var dataType = _context.GetTypeByString(paremeter.DataType);
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