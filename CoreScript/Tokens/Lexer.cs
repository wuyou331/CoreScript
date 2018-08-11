using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sprache;

namespace CoreScript.Tokens
{
    public class Lexer
    {
        public static IList<Token> Analyzer(string script)
        {
            var parser =( TokenParser.FuncParser.XOr<Token>(FactorParser.Assignment)).Many();
            var rs = parser.TryParse(script);
            return rs.Value.ToList();
        }
    }
}
