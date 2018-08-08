using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;

namespace CoreScript
{
    public static class TokenParser
    {
        public static readonly Parser<TokenFunctionDefine> FuncParser =
        (from s in FactorParser.Keyword("func").Once().Token()
            from name in FactorParser.Identifier.Once().Token()
            from s2 in FactorParser.Tuple.Once()
            from s3 in FactorParser.Block.Once()
            select new TokenFunctionDefine()
            {
                Name = name.Text(),
                Parameters = s2.First().TokenVariables,
                CodeBlock = s3.First()
            }).Token();
    }

    public static class FactorParser
    {
        public static readonly Func<string, Parser<string>> Keyword =
            (keywork) => (from s in Parse.String(keywork)
                from s1 in Parse.WhiteSpace.AtLeastOnce()
                select new string(s.ToArray())).Token();

        public static readonly Parser<string> Identifier =
        (from s1 in Parse.Letter.Or(Parse.Char('_')).AtLeastOnce()
            from s2 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select new string(s1.Concat(s2).ToArray())).Token();

        public static readonly Parser<TokenVariable> Variable = (from s1 in Identifier.Once()
            from s2 in Identifier.Once()
            from s3 in Parse.Char(',').Optional().Token()
            select new TokenVariable()
            {
                DataType = s1.Text(),
                Variable = s2.Text()
            }
        );

        public static readonly Parser<TokenTuple> Tuple = (
            from s1 in Parse.Char('(').Once().Token()
            from vars in Variable.Many()
            from s2 in Parse.Char(')').Once().Token()
            select new TokenTuple()
            {
                TokenVariables = vars.ToList()
            }).Token();

        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
        (from s1 in Identifier.AtLeastOnce()
            from s2 in (from s22 in Parse.Char('.').Once()
                from s23 in Identifier.Once()
                select s22.Text()).Many()
            from s3 in Tuple.Once()
            from s4 in Parse.Char(';').AtLeastOnce().Token()
            select new TokenFunctionCallStement()
            {
                CallChain = new List<string>(new[] {s1.Text()}.Concat(s2))
            }
        ).Token();

        public static readonly Parser<TokenStement> Statement = CallMethodStatement;

        /// <summary>
        /// 代码段
        /// </summary>
        public static readonly Parser<TokenBlockStement> Block = (
            from s1 in Parse.Char('{').AtLeastOnce().Token()
            from s2 in Statement.Many()
            from s3 in Parse.Char('}').AtLeastOnce().Token()
            select new TokenBlockStement()
            {
                Stements = s2.ToList()
            }).Token();
    }
}