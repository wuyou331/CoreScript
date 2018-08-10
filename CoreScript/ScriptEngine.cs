using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreScript.Tokens;
using Sprache;

namespace CoreScript
{
    public class ScriptEngine
    {
        public ScriptEngine()
        {
        }

        public void Excute(string script)
        {
            var rs = TokenParser.FuncParser.TryParse(script);
            ScriptFunction func = new ScriptFunction(rs.Value, this);
            func.Excute(null);
        }
    }

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

                var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany
                (it =>
                    it.DefinedTypes.Where(it1 => it1.Name == first)
                        .Select(t => t.GetMethod(stement.CallChain.Last(), argTypes))
                        .Where(m => m != null)
                );
                var method = methods.First();
                method.Invoke(null, stement.Parameters.Cast<TokenLiteral>().Select(it => it.Value).ToArray());
            }
            return null;
        }
    }
}