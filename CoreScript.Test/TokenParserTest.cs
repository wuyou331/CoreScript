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
            Assert.IsFalse(FactorParser.Keyword("func").TryParse("func").WasSuccessful);
            Assert.IsFalse(FactorParser.Keyword("func").TryParse(" func").WasSuccessful);
            Assert.IsTrue(FactorParser.Keyword("func").TryParse("func ").WasSuccessful);
            Assert.IsTrue(FactorParser.Keyword("func").TryParse(" func ").WasSuccessful);
            Assert.IsFalse(FactorParser.Keyword("func").TryParse("func1").WasSuccessful);
        }
        [TestMethod]
        public void TestIdentifier()
        {
            Assert.IsTrue(FactorParser.Identifier.TryParse("abc").WasSuccessful);
            Assert.IsTrue(FactorParser.Identifier.TryParse("abc123").WasSuccessful);
            Assert.IsTrue(FactorParser.Identifier.TryParse("abc_123").WasSuccessful);
            Assert.IsTrue(FactorParser.Identifier.TryParse("_abc_123").WasSuccessful);
            Assert.IsFalse(FactorParser.Identifier.TryParse("1_abc_123").WasSuccessful);
        }

        [TestMethod]
        public void TestStatement()
        {
            Assert.IsTrue(FactorParser.Statement.TryParse("abc();").WasSuccessful);
            Assert.IsTrue(FactorParser.Statement.TryParse("abc.abc();").WasSuccessful);
            Assert.IsFalse(FactorParser.Statement.TryParse("abc;").WasSuccessful);
        }

        [TestMethod]
        public void TestFuncParser()
        {
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc(){}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc (){}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func abc ( ){}").WasSuccessful);
            Assert.IsTrue(TokenParser.FuncParser.TryParse("func _abc ( ){}").WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func 1_abc(){}").WasSuccessful);
            Assert.IsFalse(TokenParser.FuncParser.TryParse("func func abc(){}").WasSuccessful);
        }
    }
}