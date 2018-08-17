using System;
using System.Collections.Generic;
using System.Linq;
using CoreScript.Script;
using Sprache;

namespace CoreScript.Tokens
{
    public static class TokenParser
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public static readonly Func<string, Parser<string>> Keyword =
            keywork => (from key in Parse.String(keywork)
                from spacke in Parse.WhiteSpace.AtLeastOnce()
                select new string(key.ToArray())).Token();

        /// <summary>
        /// 标识符
        /// </summary>
        public static readonly Parser<string> Identifier =
            from s1 in Parse.Letter.Or(Parse.Char('_')).Once()
            from s2 in Parse.LetterOrDigit.Or(Parse.Char('_')).Many()
            select s1.Concat(s2).Text();

        #region Variable

        /// <summary>
        /// ex:int a
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
        /// ex:int a,int b
        /// </summary>
        public static readonly Parser<IEnumerable<TokenVariableDefine>> Variables =
            (from s1 in VariableDefine.Once()
                from s2 in (from s21 in Parse.Char(',')
                    from s22 in VariableDefine.Token()
                    select s22).Many()
                select new List<TokenVariableDefine>(s1.Concat(s2))).Token();



        public static readonly Parser<TokenVariableRef> VariableRef = from s in Identifier
            select new TokenVariableRef
            {
                Variable = s
            };

        #endregion

        #region Tuple

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


        /// <summary>
        ///     ex:("abc",id,123)
        /// </summary>
        public static readonly Parser<IEnumerable<IReturnValue>> Tuple =
            (from left in Parse.Char('(').Token()
                from args in (from argFirst in CallMethodStatementPart.Or<IReturnValue>( BinaryExpression).Or(JudgmentExpression).Or(Literal).Or(VariableRef).Once()
                    from spit in (from comma in Parse.Char(',').Token()
                        from argOther in  CallMethodStatementPart.Or<IReturnValue>( BinaryExpression).Or(JudgmentExpression).Or(Literal).Or(VariableRef)
                        select argOther).Many()
                    select argFirst.Concat(spit)).Optional()
                from right in Parse.Char(')').Token()
                select args.ToArray()).Token();

        #endregion

        #region Literal

        public static readonly Parser<IReturnValue> LiteralInt =
            from sign in Parse.Char('-').Optional()
            from n in Parse.Number.AtLeastOnce()
            select new TokenLiteral
            {
                DataType = ScriptType.Int,
                Value = int.Parse(sign.ToArray().Concat(n).Text())
            };

        public static readonly Parser<IReturnValue> LiteralDouble =
            from sign in Parse.Char('-').Optional()
            from a in Parse.Number.AtLeastOnce()
            from n in Parse.Char('.').Once()
            from c in Parse.Number.AtLeastOnce()
            select new TokenLiteral
            {
                DataType = ScriptType.Double,
                Value = double.Parse(sign.ToArray().Concat(a).Concat(n).Concat(c).Text())
            };

        public static readonly Parser<IReturnValue> LiteralBoolean =
            from val in Parse.String("true").Or(Parse.String("false"))
            select new TokenLiteral
            {
                DataType = ScriptType.Boolean,
                Value = bool.Parse(val.Text())
            };

        /// <summary>
        ///     ex: asfd\"
        /// </summary>
        private static readonly Parser<string> LiteralStringPart = from first in Parse.CharExcept("\\\"").Many()
            from second in Parse.String("\\\"").Then(x => Parse.Return("\"")).Optional()
            select first.Concat(second.GetOrDefault(() => string.Empty)).Text();

        public static readonly Parser<IReturnValue> LiteralString = (from open in Parse.Char('"')
            from content in LiteralStringPart.Many()
            from close in Parse.Char('"')
            select new TokenLiteral
            {
                DataType = ScriptType.String,
                Value = content.Text()
            });

        public static readonly Parser<IReturnValue> Literal =
            LiteralBoolean.Or(LiteralDouble).Or(LiteralInt).Or(LiteralString);

        #endregion

        #region  Judgment
        private static readonly Parser<TokenJudgmentExpression> JudgmentExpressionFactor =
            from first in Literal.Or(VariableRef).Token()
            from sign in Parse.String("==").Or(Parse.String("!=")).Token()
            from second in Literal.Or(VariableRef)
            select new TokenJudgmentExpression()
            {
                Left = first,
                Operator = sign.Text() == "==" ? JudgmentExpressionType.Equal : JudgmentExpressionType.NotEqual,
                Right = second
            };

        private static readonly Parser<IReturnValue> JudgmentExpressionTerm =
        (from lparen in Parse.Char('(')
            from expr in Parse.Ref(() => JudgmentExpression)
            from rparen in Parse.Char(')')
            select expr).Or(JudgmentExpressionFactor).Or(LiteralBoolean)
            .Or(VariableRef);


        /// <summary>
        /// ex: a==b and 1==1 or a
        /// </summary>
        public static readonly Parser<TokenJudgmentExpression> JudgmentExpression = Parse
            .ChainOperator(Parse.String("and").Or(Parse.String("or")).Token().Text(), JudgmentExpressionTerm, BuildTokenJudgmentExpression)
            .ThenCast<IReturnValue,TokenJudgmentExpression>();

        private static  TokenJudgmentExpression  BuildTokenJudgmentExpression(string opt, IReturnValue left, IReturnValue right)
        {
            return new TokenJudgmentExpression()
            {
                Left = left,
                Operator = opt == "and" ? JudgmentExpressionType.And : JudgmentExpressionType.Or,
                Right = right
            };
        }
        #endregion

        #region BinaryExpression

        private static readonly Parser<IReturnValue> Factor =
            (from lparen in Parse.Char('(')
                from expr in Parse.Ref(() => BinaryExpression)
                from rparen in Parse.Char(')')
                select expr);

        private static readonly Parser<IReturnValue> Operand =
            LiteralDouble.Or(LiteralInt).Or(VariableRef).Or(LiteralString)
                .Or(Factor).Token();


        private static readonly Parser<IReturnValue> Term =
            Parse.ChainOperator(Parse.Char('*').Or(Parse.Char('/')).Or(Parse.Char('%')).Token(), Operand,
                BuildExpression);

        /// <summary>
        /// ex:1+1*(2+1)
        /// </summary>
        public static readonly Parser<TokenBinaryExpression> BinaryExpression = Parse
            .ChainOperator(Parse.Char('+').Or(Parse.Char('-')).Token(), Term, BuildExpression)
            .ThenCast<IReturnValue, TokenBinaryExpression>();


        private static TokenBinaryExpression BuildExpression(char opt, IReturnValue left, IReturnValue right)
        {
            return new TokenBinaryExpression()
            {
                Left = left,
                Operator = opt,
                Right = right
            };
        }

        #endregion

        #region  stement: if/else call assingment

        #region if else Stement

        private static readonly Parser<TokenConditionBlock> ElssIf =
        (
            from _else in Parse.String("else")
            from space1 in Parse.WhiteSpace.AtLeastOnce()
            from _if in IFStement
            select _if).Token();

        private static readonly Parser<TokenConditionBlock> ELSE = (
            from _if in Parse.String("else").Token()
            from trueBlock in Block.Token()
            select new TokenConditionBlock()
            {
                Condition = LiteralBoolean.Parse("true"),
                TrueBlock = trueBlock
            }).Token();

        public static readonly Parser<TokenConditionBlock> IFStement = (
            from _if in Parse.String("if")
            from space1 in Parse.WhiteSpace.AtLeastOnce()
            from expr in JudgmentExpression.Or(LiteralBoolean).Or(VariableRef)
            from space2 in Parse.WhiteSpace.AtLeastOnce()
            from then in Parse.String("then")
            from trueBlock in Block.Token()
            from _else in ELSE.Or(ElssIf).Optional()
            select new TokenConditionBlock()
            {
                Condition = expr,
                TrueBlock = trueBlock,
                Else = _else.GetOrDefault()
            }).Token();

        #endregion



        /// <summary>
        /// return 1; return a; return 1+1; return a ==b ; return a and 1==2
        /// </summary>
        public static readonly Parser<TokenReturnStement> ReturnStement =
            from ret in Keyword("return")
            from val in CallMethodStatementPart.Or<IReturnValue>( BinaryExpression).Or(JudgmentExpression).Or(Literal).Or(VariableRef).Token().Optional()
            from last in Parse.Char(';').Token()
            select new TokenReturnStement()
            {
                Value = val.GetOrDefault()
            };
        
        
        /// <summary>
        ///     ex:Console.WriteLine("123");
        /// </summary>
        public static readonly Parser<TokenFunctionCallStement> CallMethodStatementPart =
            (from first in Identifier.Once().Token()
                from other in (from s22 in Parse.Char('.')
                    from s23 in Identifier
                    select s23.Text()).Many()
                from argResult in Tuple
                select new TokenFunctionCallStement
                {
                    CallChain = new List<string>(first.Concat(other)),
                    Parameters = argResult.ToList()
                }
            );

        /// <summary>
        ///     ex:Console.WriteLine("123");
        /// </summary>
        public static readonly Parser<TokenFunctionCallStement> CallMethodStatement =
            (from first in CallMethodStatementPart
                from end in Parse.Char(';').Token()
                select first
            ).Token();
        
        /// <summary>
        ///     ex: int a =1;  /  var a =1 ;   /     a=1;
        /// </summary>
        public static readonly Parser<TokenStement> Assignment =
            (from left in VariableDefine.Or<IReturnValue>(VariableRef)
                from opt in Parse.Char('=').Token()
                from right in CallMethodStatementPart.Or<IReturnValue>(BinaryExpression).Or(JudgmentExpression).Or(Literal).Or(VariableRef)
                from last in Parse.Char(';').Token()
                select new TokenAssignment(left.TokenType == TokenType.VariableDefine
                    ? TokenType.AssignmentDefine
                    : TokenType.Assignment)
                {
                    Left = left,
                    Right = right
                }).Token();

        public static readonly Parser<TokenStement> Statement =ReturnStement.Or<TokenStement>( CallMethodStatement).Or(Assignment).Or(IFStement);

        #endregion

        #region funciton

        /// <summary>
        ///     代码段
        /// </summary>
        public static readonly Parser<TokenBlockStement> Block = (
            from s1 in Parse.Char('{').Token()
            from statements in Statement.Many()
            from s3 in Parse.Char('}').Token()
            select new TokenBlockStement
            {
                Stements = statements.Slice(it => it.TokenType == TokenType.Return)
            }).Token();


        public static readonly Parser<TokenFunctionDefine> FuncParser =
            (from id in Keyword("func").Token()
                from name in Identifier
                from args in TupleDefine.Optional()
                from block in Block
                select new TokenFunctionDefine
                {
                    Name = name.Text(),
                    Parameters = args.GetOrDefault(() => new TokenTupleDefine()),
                    CodeBlock = block
                }).Token();

        #endregion
    }
}