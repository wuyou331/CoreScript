using System.Linq;
using CoreScript.Script;
using CoreScript.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprache;

namespace CoreScript.Test
{
    [TestClass]
    public class TokenParserTest
    {
        [TestMethod]
        public void TestKeyword()
        {
            Assert.IsFalse(TokenParser.Keyword("func").TryParse("func").WasSuccessful);
            Assert.IsFalse(TokenParser.Keyword("func").TryParse(" func").WasSuccessful);
            Assert.IsTrue(TokenParser.Keyword("func").TryParse("func ").WasSuccessful);
            Assert.IsTrue(TokenParser.Keyword("func").TryParse(" func ").WasSuccessful);
            Assert.IsFalse(TokenParser.Keyword("func").TryParse("func1").WasSuccessful);
        }

        [TestMethod]
        public void TestIdentifier()
        {
            Assert.IsTrue(TokenParser.Identifier.TryParse("abc").WasSuccessful);
            Assert.IsTrue(TokenParser.Identifier.TryParse("abc123").WasSuccessful);
            Assert.IsTrue(TokenParser.Identifier.TryParse("abc_123").WasSuccessful);
            Assert.IsTrue(TokenParser.Identifier.TryParse("_abc_123").WasSuccessful);
            Assert.IsFalse(TokenParser.Identifier.TryParse("1_abc_123").WasSuccessful);
        }
        
        [TestMethod]
        public void TestTuple()
        {
            Assert.IsTrue(TokenParser.Tuple.TryParse("()").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(a)").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(a,b)").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(1,2)").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(get())").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(true)").WasSuccessful);
            Assert.IsTrue(TokenParser.Tuple.TryParse("(true,false)").WasSuccessful);
            Assert.IsFalse(TokenParser.Tuple.TryParse("(a abc)").WasSuccessful);
            Assert.IsFalse(TokenParser.Tuple.TryParse("(a,)").WasSuccessful);
        }


        [TestMethod]
        public void TestTupleDefine()
        {
            Assert.IsTrue(TokenParser.TupleDefine.TryParse("()").WasSuccessful);
            Assert.IsFalse(TokenParser.TupleDefine.TryParse("(a)").WasSuccessful);
            Assert.IsTrue(TokenParser.TupleDefine.TryParse("(a abc)").WasSuccessful);
            Assert.IsFalse(TokenParser.TupleDefine.TryParse("(a abc,)").WasSuccessful);
            Assert.IsTrue(TokenParser.TupleDefine.TryParse("(a abc,b abd)").WasSuccessful);
        }

        [TestMethod]
        public void TestStatement()
        {
            Assert.IsTrue(TokenParser.CallMethodStatement.TryParse("abc();").WasSuccessful);
            Assert.IsTrue(TokenParser.CallMethodStatement.TryParse("abc(abc);").WasSuccessful);
            Assert.IsTrue(TokenParser.CallMethodStatement.TryParse("abc.abc();").WasSuccessful);
            Assert.IsFalse(TokenParser.CallMethodStatement.TryParse("abc;").WasSuccessful);
        }

        [TestMethod]
        public void TestString()
        {
            Assert.IsTrue(TokenParser.LiteralString.TryParse("\"\"").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralString.TryParse("\"asdf\"").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralString.TryParse("\"as\\\"df\"").WasSuccessful);
        }

        [TestMethod]
        public void TestInt()
        {
            Assert.IsTrue(TokenParser.LiteralInt.TryParse("123").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralInt.TryParse("0123").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralInt.TryParse("1.2").WasSuccessful);
        }
        [TestMethod]
        public void TestDouble()
        {
            Assert.IsTrue(TokenParser.LiteralDouble.TryParse("12.12").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralDouble.TryParse("0.12").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralDouble.TryParse("-12.12").WasSuccessful);
            Assert.IsFalse(TokenParser.LiteralDouble.TryParse("-12.").WasSuccessful);
            Assert.IsFalse(TokenParser.LiteralDouble.TryParse("-.12").WasSuccessful);
        }
        [TestMethod]
        public void TestBoolean()
        {
            Assert.IsTrue(TokenParser.LiteralBoolean.TryParse("true").WasSuccessful);
            Assert.IsTrue(TokenParser.LiteralBoolean.TryParse("false").WasSuccessful);

            Assert.IsFalse(TokenParser.LiteralBoolean.TryParse("True").WasSuccessful);
            Assert.IsFalse(TokenParser.LiteralBoolean.TryParse("False").WasSuccessful);
        }
        [TestMethod]
        public void TestJudgmentExpression()
        {
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("1==1").WasSuccessful);
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("1!=1").WasSuccessful);
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("a==b").WasSuccessful);
            Assert.IsFalse(TokenParser.JudgmentExpression.TryParse("a=b").WasSuccessful);
   
            Assert.IsFalse(TokenParser.JudgmentExpression.TryParse("a").WasSuccessful);

            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("1==1").WasSuccessful);

            var rs = TokenParser.JudgmentExpression.TryParse("a==b and c==d");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenJudgmentExpression);
            Assert.IsTrue(rs.Value.Right is TokenJudgmentExpression);

            rs = TokenParser.JudgmentExpression.TryParse("(a==b) and (c==d)");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenJudgmentExpression);
            Assert.IsTrue(rs.Value.Right is TokenJudgmentExpression);


            rs = TokenParser.JudgmentExpression.TryParse("a==b or 1==1");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenJudgmentExpression);
            Assert.IsTrue(rs.Value.Right is TokenJudgmentExpression);

            rs = TokenParser.JudgmentExpression.TryParse("a and b");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenVariableRef);
            Assert.IsTrue(rs.Value.Right is TokenVariableRef);

            rs = TokenParser.JudgmentExpression.TryParse("true or false");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenLiteral);
            Assert.IsTrue(rs.Value.Right is TokenLiteral);

            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("a==b and b").WasSuccessful);
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("(a==b) and b").WasSuccessful);
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("a and b==c").WasSuccessful);
            Assert.IsTrue(TokenParser.JudgmentExpression.TryParse("a and (b==c)").WasSuccessful);

            rs = TokenParser.JudgmentExpression.TryParse("a==b and c==d and e==f");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenJudgmentExpression);
            Assert.IsTrue(rs.Value.Right is TokenJudgmentExpression);

            rs = TokenParser.JudgmentExpression.TryParse("a==b and (c==d and e==f)");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsTrue(rs.Value.Left is TokenJudgmentExpression );
            Assert.IsNotNull(rs.Value);
            var value = rs.Value.Right as TokenJudgmentExpression;
            Assert.IsTrue(value.Left is TokenJudgmentExpression);
            Assert.IsTrue(value.Right is TokenJudgmentExpression);

        }
        [TestMethod]

        public void TestBinaryExpression()
        {

            Assert.IsFalse(TokenParser.BinaryExpression.TryParse("1").WasSuccessful);
            Assert.IsTrue(TokenParser.BinaryExpression.TryParse("1+1").WasSuccessful);
            var rs = TokenParser.BinaryExpression.TryParse("1+2+3");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsNotNull(rs.Value.Left is TokenBinaryExpression);
            rs = TokenParser.BinaryExpression.TryParse("5+(1+2)*3");
            Assert.IsTrue(rs.WasSuccessful);
           // Assert.AreEqual(ScriptEngine.SumBinaryExpression(rs.Value, new VariableStack()).Value, 5 + (1 + 2) * 3);

            rs = TokenParser.BinaryExpression.TryParse("5+(1-1*2+(1+3)+2)*3");
            Assert.IsTrue(rs.WasSuccessful);
          //  Assert.AreEqual(ScriptEngine.SumBinaryExpression(rs.Value,new VariableStack()).Value, 5 + (1 - 1 * 2 + (1 + 3) + 2) * 3);
        }

        [TestMethod]
        public void TestCondition()
        {
            var stement = TokenParser.IFStement.TryParse("if true then {}");
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsNull(stement.Value.Else);

             stement = TokenParser.IFStement.TryParse("if 1==1 then {}");
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsNull(stement.Value.Else);

            stement = TokenParser.IFStement.TryParse("if true then {} else {}");
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsNotNull(stement.Value.Else);

            stement = TokenParser.IFStement.TryParse("if true then {} else if false then {}");
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsNotNull(stement.Value.Else);
            Assert.IsNull(stement.Value.Else.Else);

            stement = TokenParser.IFStement.TryParse("if true then { } else if true then { } else { }");
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsTrue(stement.WasSuccessful);
            Assert.IsNotNull(stement.Value.Else);
            Assert.IsNotNull(stement.Value.Else.Else);
            Assert.IsNull(stement.Value.Else.Else.Else);
        }
        [TestMethod]
        public void TestReturnStement()
        {
            Assert.IsTrue(TokenParser.ReturnStement.TryParse("return a;").WasSuccessful);
            Assert.IsTrue(TokenParser.ReturnStement.TryParse("return a and b;").WasSuccessful);
            Assert.IsTrue(TokenParser.ReturnStement.TryParse("return 1+1;").WasSuccessful);
            Assert.IsTrue(TokenParser.ReturnStement.TryParse("return  ;").WasSuccessful);
        }
        
        [TestMethod]
        public void TestAssignmentStement()
        {
            Assert.IsTrue(TokenParser.Assignment.TryParse(" a=1;").WasSuccessful);
            Assert.IsTrue(TokenParser.Assignment.TryParse("var a=1;").WasSuccessful);
            Assert.IsTrue(TokenParser.Assignment.TryParse("var a=abc;").WasSuccessful);
            Assert.IsTrue(TokenParser.Assignment.TryParse("var a=true;").WasSuccessful);
            Assert.IsTrue(TokenParser.Assignment.TryParse("var a=abc();").WasSuccessful);
        }

        [TestMethod]
        public void TestBlock()
        {
            var script = "{var i=0;" +
                         "return i;" +
                         "var a=1;" +
                         "}";
            var result = TokenParser.Block.TryParse(script);
            Assert.IsTrue(result.WasSuccessful);
            Assert.AreEqual(result.Value.Stements.Count(),2);
        }
        
        [TestMethod]
        public void TestFuncParser()
        {
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc(){}").WasSuccessful);
            var rs = TokenParser.FuncParser.TryParse("func abc(){" +
                                                     "Console.WriteLine(abc);" +
                                                     "Console.WriteLine(123);" +
                                                     "Console.WriteLine(123.12);" +
                                                     "}");
            Assert.IsTrue(rs.WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func abc(){" +
                                                           "Console.WriteLine()" +
                                                           "}").WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func abc(){" +
                                                           "1Console.WriteLine();" +
                                                           "}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc (){}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc ( ){}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func _abc ( ){}").WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func 1_abc(){}").WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func func abc(){}").WasSuccessful);
        }
    }
}