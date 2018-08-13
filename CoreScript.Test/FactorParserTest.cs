using System.Linq;
using CoreScript.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprache;

namespace CoreScript.Test
{
    [TestClass]
    public class FactorParserTest
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
        public void TestFunctionArgs()
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
        public void TestCondition()
        {
            var stement = TokenParser.IFStement.TryParse("if true then {}");
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
    }
}