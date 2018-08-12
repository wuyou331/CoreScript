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
            var parser = (TokenParser.FuncParser.Token().Or<Token>(FactorParser.Assignment).Token()).Many().End();
            var rs = parser.TryParse(script);
            return rs.Value.ToList();
        }
    }
}
