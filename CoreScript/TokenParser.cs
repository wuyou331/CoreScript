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
        (from s1 in Parse.Letter.Or(Parse.Char('_')).AtLeastOnce()
            from s2 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select new string(s1.Concat(s2).ToArray())).Token();

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


        public static readonly Parser<IEnumerable<string>> CallMethodStatementArgs=(from argFirst in Identifier.Once()
            from argOther in (from comma in Parse.Char(',').Token()
        from argOtherItem in Identifier
            select argOtherItem).Many()
        select argFirst.Concat(argOther)).Token();

        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
        (from first in Identifier.Once()
            from other in (from s22 in Parse.Char('.')
                from s23 in Identifier
                select s23.Text()).Many()
            from left in Parse.Char('(').Token()
            from argResult in CallMethodStatementArgs.Optional()
         from right in Parse.Char(')').Token()
            from end in Parse.Char(';').Token()
            select new TokenFunctionCallStement()
            {
                CallChain = new List<string>(first.Concat(other))
                
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