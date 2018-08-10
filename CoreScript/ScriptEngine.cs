using System.Text;
using CoreScript.Script;
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
}