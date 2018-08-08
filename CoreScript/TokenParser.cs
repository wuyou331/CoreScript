using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;

namespace CoreScript
{
    public static class TokenParser
    {
        public static readonly Parser<string> FuncParser =
        (from s in FactorParser.Keyword("func").AtLeastOnce().Token()
            from s1 in FactorParser.Identifier.AtLeastOnce().Token()
            from s2 in FactorParser.Tuple
            select "").Token();
    }

    public static class FactorParser
    {
        public static readonly Parser<String> Tuple = (
            from s1 in Parse.Char('(').AtLeastOnce().Token()
            from s2 in Parse.Char(')').AtLeastOnce().Token()
            select "").Token();

        private static readonly string[] Keywords = new[] {"func"};

        public static readonly Func<string, Parser<string>> Keyword =
            (keywork) => (from s in Parse.String(keywork)
                from s1 in Parse.WhiteSpace.AtLeastOnce()
                select new string(s.ToArray())).Token();

        public static readonly Parser<string> Identifier =
        (from s in Parse.Letter.Or(Parse.Char('_')).Once()
            from s1 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select new string(s.Concat(s1).ToArray())).Token();

        public static Parser<string> Key()
        {
            var parser = Keyword("");
            foreach (var key in Keywords)
            {
                parser.Or(Keyword(key));
            }
            return parser;
        }
    }
}