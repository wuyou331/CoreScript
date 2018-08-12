using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace CoreScript.Tokens
{
    public static class TokenParser
    {
        public static readonly Parser<TokenFunctionDefine> FuncParser =
            (from id in FactorParser.Keyword("func").Token()
                from name in FactorParser.Identifier
                from args in FactorParser.TupleDefine.Optional()
                from block in FactorParser.Block
                select new TokenFunctionDefine
                {
                    Name = name.Text(),
                    Parameters = args.GetOrDefault(() => new TokenTupleDefine()),
                    CodeBlock = block
                }).Token();
    }

    public static class FactorParser
    {
        public static readonly Func<string, Parser<string>> Keyword =
            keywork => (from s in Parse.String(keywork)
                from s1 in Parse.WhiteSpace.AtLeastOnce()
                select new string(s.ToArray())).Token();

        public static readonly Parser<string> Identifier =
            from s1 in Parse.Letter.Or(Parse.Char('_')).Once()
            from s2 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select s1.Concat(s2).Text();

        /// <summary>
        ///     ex:int a
        /// </summary>
        public static readonly Parser<TokenVariableDefine> VariableDefine = (from type in Identifier
                from space in Parse.WhiteSpace.AtLeastOnce()
                from value in Identifier.Token()
                select new TokenVariableDefine
                {
                    DataType = type.Text(),
                    Variable = value.Text()
                }
            ).Token();

        /// <summary>
        ///     ex:int a,int b
        /// </summary>
        public static readonly Parser<IEnumerable<TokenVariableDefine>> Variables =
            (from s1 in VariableDefine.Once()
                from s2 in (from s21 in Parse.Char(',').Token()
                    from s22 in VariableDefine
                    select s22).Many()
                select new List<TokenVariableDefine>(s1.Concat(s2))).Token();

        /// <summary>
        ///     ex:(int a,int b)
        /// </summary>
        public static readonly Parser<TokenTupleDefine> TupleDefine = (
            from s1 in Parse.Char('(').Token()
            from vars in Variables.Optional()
            from s2 in Parse.Char(')').Token()
            select new TokenTupleDefine
            {
                Variables = vars.ToArray().ToList()
            }).Token();


        public static readonly Parser<TokenVariableRef> VariableRef = from s in Identifier
            select new TokenVariableRef
            {
                Variable = s
            };

        public static readonly Parser<IReturnValue> LiteralInt =
            (from sign in Parse.Char('-').Optional()
                from n in Parse.Number.AtLeastOnce()
                select new TokenLiteral
                {
                    DataType = nameof(Int32),
                    Value = int.Parse(sign.ToArray().Concat(n).Text())
                }).Token();

        public static readonly Parser<IReturnValue> LiteralDouble =
            (from sign in Parse.Char('-').Optional()
                from a in Parse.Number.AtLeastOnce()
                from n in Parse.Char('.').Once()
                from c in Parse.Number.AtLeastOnce()
                select new TokenLiteral
                {
                    DataType = nameof(Double),
                    Value = double.Parse(sign.ToArray().Concat(a).Concat(n).Concat(c).Text())
                }).Token();

        public static readonly Parser<IReturnValue> LiteralString = (from open in Parse.Char('"')
            from content in LiteralStringPart.Many()
            from close in Parse.Char('"')
            select new TokenLiteral
            {
                DataType = nameof(String),
                Value = content.Text()
            }).Token();

        public static readonly Parser<IReturnValue> Literal =
            LiteralDouble.Or(LiteralInt).Or(LiteralString);

        /// <summary>
        ///     ex:("abc",id,123)
        /// </summary>
        public static readonly Parser<IEnumerable<IReturnValue>> Tuple =
            (from left in Parse.Char('(').Token()
                from args in (from argFirst in VariableRef.Or(Literal).Once()
                    from argOther in (from comma in Parse.Char(',').Token()
                        from argOtherItem in VariableRef.Or(Literal)
                        select argOtherItem).Many()
                    select argFirst.Concat(argOther)).Optional()
                from right in Parse.Char(')').Token()
                select args.ToArray()).Token();


        /// <summary>
        ///     ex:Console.WriteLine("123");
        /// </summary>
        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
            (from first in Identifier.Once().Token()
                from other in (from s22 in Parse.Char('.')
                    from s23 in Identifier
                    select s23.Text()).Many()
                from argResult in Tuple
                from end in Parse.Char(';').Token()
                select new TokenFunctionCallStement
                {
                    CallChain = new List<string>(first.Concat(other)),
                    Parameters = argResult.ToList()
                }
            ).Token();

        /// <summary>
        ///     ex: int a =1; or  a=1;
        /// </summary>
        public static readonly Parser<TokenStement> Assignment =
            (from left in VariableDefine.Or<IReturnValue>(VariableRef)
                from cent in Parse.Char('=').Token()
                from right in Literal.Or(VariableRef).Or(CallMethodStatement)
                from last in Parse.Char(';').Token()
                select new TokenAssignment(left.TokenType == TokenType.VariableDefine
                    ? TokenType.AssignmentInit
                    : TokenType.Assignment)
                {
                    Left = left,
                    Right = right
                }).Token();

        public static readonly Parser<TokenStement> Statement = CallMethodStatement.Or(Assignment);

        /// <summary>
        ///     代码段
        /// </summary>
        public static readonly Parser<TokenBlockStement> Block = (
            from s1 in Parse.Char('{').Token()
            from statements in Statement.Many()
            from s3 in Parse.Char('}').Token()
            select new TokenBlockStement
            {
                Stements = statements.ToList()
            }).Token();

        #region Literal

        /// <summary>
        ///     ex: asfd\"
        /// </summary>
        private static readonly Parser<string> LiteralStringPart = from first in Parse.CharExcept("\\\"").Many()
            from second in Parse.String("\\\"").Then(x => Parse.Return("\"")).Optional()
            select first.Concat(second.GetOrDefault(() => string.Empty)).Text();

        #endregion
    }
}