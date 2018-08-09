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
                Parameters = args.GetOrDefault(() => new TokenTupleDefine()),
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

        #region Literal

        public static readonly Parser<TokenValue> LiteralInt =
        (from sign in Parse.Char('-').Optional()
            from n in Parse.Number.Many()
            select new TokenLiteral()
            {
                DateType = "Int",
                Value = sign.ToArray().Concat(n).Text()
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

        public static readonly Parser<TokenValue> Literal =
            LiteralDouble.Or(LiteralInt);
        #endregion




        /// <summary>
        /// ex:int a
        /// </summary>
        public static readonly Parser<TokenVariableDefine> VariableDefine = (from type in Identifier.Token()
            from space in Parse.WhiteSpace.Many()
            from value in Identifier.Token()
            select new TokenVariableDefine()
            {
                DataType = type.Text(),
                Variable = value.Text()
            }
        ).Token();



        /// <summary>
        /// ex:int a,int b
        /// </summary>
        public static readonly Parser<IEnumerable<TokenVariableDefine>> Variables =
        (from s1 in VariableDefine.Once()
            from s2 in (from s21 in Parse.Char(',').Token()
                from s22 in VariableDefine
                select s22).Many()
            select new List<TokenVariableDefine>(s1.Concat(s2))).Token();

        /// <summary>
        /// ex:(int a,int b)
        /// </summary>
        public static readonly Parser<TokenTupleDefine> TupleDefine = (
            from s1 in Parse.Char('(').Token()
            from vars in Variables.Optional()
            from s2 in Parse.Char(')').Token()
            select new TokenTupleDefine
            {
                Values = vars.ToArray().ToList()
            }).Token();


        public static readonly Parser<TokenVariableRef> VariableRef = (from s in Identifier
            select new TokenVariableRef()
            {
                Variable = s
            });

        /// <summary>
        /// ex:("abc",id,123)
        /// </summary>
        public static readonly Parser<IEnumerable<TokenValue>> Tuple =
        (from left in Parse.Char('(').Token()
            from argFirst in VariableRef.Or(Literal).Once()
            from argOther in (from comma in Parse.Char(',').Token()
                from argOtherItem in VariableRef.Or(Literal)
                select argOtherItem).Many()
            from right in Parse.Char(')').Token()
            select argFirst.Concat(argOther)).Token();

        /// <summary>
        /// ex:Console.WriteLine("123");
        /// </summary>
        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
        (from first in Identifier.Once()
            from other in (from s22 in Parse.Char('.')
                from s23 in Identifier
                select s23.Text()).Many()
            from argResult in Tuple
            from end in Parse.Char(';').Token()
            select new TokenFunctionCallStement()
            {
                CallChain = new List<string>(first.Concat(other)),
                Parameters = argResult.ToList()
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