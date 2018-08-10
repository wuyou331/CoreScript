using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Tokens;

namespace CoreScript.Script
{
    public class ScriptFunction
    {
        public string Name { get; set; }
        private readonly TokenFunctionDefine _token;


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
            }
            return null;
        }

        private object Excute(TokenFunctionCallStement stement)
        {
            var first = stement.CallChain.First();
            if (first != null)
            {
                var argTypes = stement.Parameters.Select(it =>
                {
                    switch (it.DateType)
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
                }).ToArray();
                //从程序集中找出名称和参数定义相同的方法
                var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany
                (it =>
                    it.DefinedTypes.Where(it1 =>  it1.Name == first)
                        .Select(t => t.GetMethod(stement.CallChain.Last(), argTypes))
                        .Where(m => m != null)
                ).ToList();
                if(!methods.Any()) throw  new Exception("没有找到对应的方法");
                //   if(methods.Count()>1) throw new Exception("找到多个方法，无法确定调用哪个");
                var method = methods.First();
                method.Invoke(null, stement.Parameters.Cast<TokenLiteral>().Select(it => it.Value).ToArray());
            }
            return null;
        }
    }
}