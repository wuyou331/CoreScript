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
        public string Name { get; set; }
        private readonly TokenFunctionDefine _token;

        private readonly IDictionary<string , ScriptVariable> _localVars =new Dictionary<string, ScriptVariable>();

        public ScriptFunction(TokenFunctionDefine token, ScriptEngine context)
        {
            _token = token;
            this.Name = _token.Name;
        }

        public object Excute(IList<object> args)
        {
            foreach (var stement in _token.CodeBlock.Stements)
            {
                if (stement is TokenFunctionCallStement call)
                    Excute(call);
                else if(stement is TokenAssignment assignment)
                    Excute(assignment);
            }
            return null;
        }

        /// <summary>
        /// 变量赋值
        /// </summary>
        /// <param name="stement"></param>
        private void Excute(TokenAssignment stement)
        {
            ScriptVariable varItem = null;
            if (stement.Left is TokenVariableDefine define)
            {
                 varItem = new ScriptVariable();
                _localVars[define.Variable] = varItem;
            }
            else if (stement.Left is TokenVariableRef varRef)
            {
                varItem = _localVars[varRef.Variable];
            }

        
            if (stement.Right is TokenLiteral literal)
            {
                varItem.DataType = literal.DataType;
                varItem.Value = literal.Value;
            }

        }
        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="stement"></param>
        /// <returns></returns>
        private object Excute(TokenFunctionCallStement stement)
        {
            var first = stement.CallChain.First();
            var argTypes = new List<Type>();
            var paremeters = new List<object>();
            foreach (var value in stement.Parameters)
            {
                if (value is TokenLiteral literal)
                {
                    var dataType = GetTypeByString(literal.DataType);
                    argTypes.Add(dataType);
                    paremeters.Add(literal.Value);
                }
                else if (value is TokenVariableRef varRef)
                {
                    var vars = _localVars[varRef.Variable];
                    var dataType = GetTypeByString(vars.DataType);
                    argTypes.Add(dataType);
                    paremeters.Add(vars.Value);
                }
            }
            //从程序集中找出名称和参数定义相同的方法
             var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany
            (it =>
                it.DefinedTypes.Where(it1 => it1.Name == first)
                    .Select(t => t.GetMethod(stement.CallChain.Last(), argTypes.ToArray()))
                    .Where(m => m != null)
            ).ToList();
            if (!methods.Any()) throw new Exception("没有找到对应的方法");
            //   if(methods.Count()>1) throw new Exception("找到多个方法，无法确定调用哪个");
            var method = methods.First();



            return method.Invoke(null, paremeters.ToArray());
        }

        /// <summary>
        /// 根据字面量字符串获取Type类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type GetTypeByString(string type)
        {
            switch (type)
            {
                case nameof(Int32):
                    return typeof(int);
                case nameof(Double):
                    return typeof(double);
                case nameof(String):
                    return typeof(string);
                default:
                    throw new Exception("未知的数据类型");
            }
        }
    }
}