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
            ScriptFunction func= new ScriptFunction(rs.Value,this);
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
              var assembile=  AppDomain.CurrentDomain.GetAssemblies()
                    .First(it => 
                    it.DefinedTypes.Any(it1=>it1.Name==first));
                var type = assembile.DefinedTypes.First(it1 => it1.Name == first);

          var argTypes=      stement.Parameters.Select(it =>
                {
                    if (it.DateType == "Int")
                    {
                        return typeof(string);
                    }else if (it.DateType == "Double")
                    {
                        return typeof(string);
                    }
                    else
                    {
                        return typeof(string);
                    }
                }).ToArray();
                 var method = type.GetMethod(stement.CallChain.Last(), argTypes.ToArray());
                method.Invoke(null,stement.Parameters.Cast<TokenLiteral>().Select(it=>it.Value).ToArray());
            }
            return null;
        }
    }
}
