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
            var tokens = new List<Token>();
            var parser =( TokenParser.FuncParser.XOr<Token>(FactorParser.Assignment)).Many();
            var index = 0;
            var rs = parser.TryParse(script);
            tokens = rs.Value.ToList();
            return tokens;
        }
    }
}
