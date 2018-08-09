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
        (from id in FactorParser.Keyword("func").Token()
            from name in FactorParser.Identifier.Token()
            from args in FactorParser.TupleDefine.Optional().Token()
            from block in FactorParser.Block
            select new TokenFunctionDefine()
            {
                Name = name.Text(),
                Parameters = args.IsEmpty ? args.Get().ToList() : new List<TokenVariableDefine>(),
                CodeBlock = block
            }).Token();
    }

    public static class FactorParser
    {
        public static readonly Func<string, Parser<string>> Keyword =
            (keywork) => (from s in Parse.String(keywork)
                from s1 in Parse.WhiteSpace.AtLeastOnce()
                select new string(s.ToArray())).Token();

        public static readonly Parser<string> Identifier =
        (from s1 in Parse.Letter.Or(Parse.Char('_')).Once()
            from s2 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select s1.Concat(s2).Text()).Token();


        public static readonly Parser<TokenValue> LiteralInt =
            (from sign in Parse.Char('-').Optional()
            from n in Parse.Number.Many()
            select new TokenLiteral()
            {
                DateType = "Int",
                Value = sign .ToArray() .Concat(n).Text()
            }).Token();

        public static readonly Parser<TokenValue> LiteralDouble =
        (from sign in Parse.Char('-').Optional()
            from a in Parse.Number.Many()
            from n in Parse.Char('.').Once()
            from c in Parse.Number.Many()
            select new TokenLiteral()
            {
                DateType = "Double",
                Value = sign.ToArray().Concat(a).Concat(n).Concat(c).Text()
            }).Token();

        public static readonly Parser<string> LiteralString = (from open in Parse.Char('"')
            from content in (from a in Parse.AnyChar.Until(Parse.String("\\\""))
                select a.Text()).Many()
            from close in Parse.Char('"')
            select content.Text()).Token();

        public static readonly Parser<TokenVariableDefine> VariableDefine = (from type in Identifier.Token()
            from space in Parse.WhiteSpace.Many()
            from value in Identifier.Token()
            select new TokenVariableDefine()
            {
                DataType = type.Text(),
                Variable = value.Text()
            }
        ).Token();

        public static readonly Parser<IEnumerable<TokenVariableDefine>> Variables =
        (from s1 in VariableDefine.Once()
            from s2 in (from s21 in Parse.Char(',').Token()
                from s22 in VariableDefine
                select s22).Many()
            select new List<TokenVariableDefine>(s1.Concat(s2))).Token();

        public static readonly Parser<IEnumerable<TokenVariableDefine>> TupleDefine = (
            from s1 in Parse.Char('(').Token()
            from vars in Variables.Optional()
            from s2 in Parse.Char(')').Token()
            select vars.GetOrDefault()).Token();


        public static readonly Parser<TokenVariableRef> VariableRef = (from s in Identifier
            select new TokenVariableRef()
            {
                Variable = s
            });

        public static readonly Parser<IEnumerable<TokenValue>> CallMethodStatementArgs =
        (from argFirst in VariableRef.Or(LiteralDouble).Or(LiteralInt).Once()
            from argOther in (from comma in Parse.Char(',').Token()
                from argOtherItem in VariableRef.Or(LiteralDouble).Or(LiteralInt)
                select argOtherItem).Many()
            select argFirst.Concat(argOther)).Token();

        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
        (from first in Identifier.Once()
            from other in (from s22 in Parse.Char('.')
                from s23 in Identifier
                select s23.Text()).Many()
            from left in Parse.Char('(').Token()
            from argResult in CallMethodStatementArgs.Many()
            from right in Parse.Char(')').Token()
            from end in Parse.Char(';').Token()
            select new TokenFunctionCallStement()
            {
                CallChain = new List<string>(first.Concat(other)),
                Parameters = argResult.SelectMany()
            }
        ).Token();

        public static readonly Parser<TokenStement> Statement = CallMethodStatement;

        /// <summary>
        /// 代码段
        /// </summary>
        public static readonly Parser<TokenBlockStement> Block = (
            from s1 in Parse.Char('{').Token()
            from statements in Statement.Many()
            from s3 in Parse.Char('}').Token()
            select new TokenBlockStement()
            {
                Stements = statements.ToList()
            }).Token();
    }
}