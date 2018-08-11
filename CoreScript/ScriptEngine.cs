using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreScript.Script;
using CoreScript.Tokens;
using Sprache;

namespace CoreScript
{
    public class ScriptEngine
    {
        private IDictionary<string, ScriptFunction> _functions = null;
        private IDictionary<string,ScriptVariable> _variable =null;
        public ScriptEngine()
        {
        }

        public void Excute(string script)
        {
            var rs = Lexer.Analyzer(script);
            foreach (var token in rs.Where(it => it.TokenType == TokenType.Assignment))
            {

            }
            _variable = new ConcurrentDictionary<string, ScriptVariable>();
            _functions = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .Select(it => new ScriptFunction(it, this))
                .ToDictionary(it => it.Name, it => it);

            var main = _functions["main"];
            main.Excute(null);
        }
    }
}