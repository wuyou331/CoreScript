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
        public void TestFunctionArgs()
        {
            Assert.IsTrue(FactorParser.TupleDefine.TryParse("()").WasSuccessful);
            Assert.IsFalse(FactorParser.TupleDefine.TryParse("(a)").WasSuccessful);
            Assert.IsTrue(FactorParser.TupleDefine.TryParse("(a abc)").WasSuccessful);
            Assert.IsFalse(FactorParser.TupleDefine.TryParse("(a abc,)").WasSuccessful);
            Assert.IsTrue(FactorParser.TupleDefine.TryParse("(a abc,b abd)").WasSuccessful);
        }

        [TestMethod]
        public void TestStatement()
        {
            Assert.IsTrue(FactorParser.CallMethodStatement.TryParse("abc();").WasSuccessful);
            Assert.IsTrue(FactorParser.CallMethodStatement.TryParse("abc(abc);").WasSuccessful);
            Assert.IsTrue(FactorParser.CallMethodStatement.TryParse("abc.abc();").WasSuccessful);
            Assert.IsFalse(FactorParser.CallMethodStatement.TryParse("abc;").WasSuccessful);
        }

        [TestMethod]
        public void TestString()
        {
            Assert.IsTrue(FactorParser.LiteralString.TryParse("\"\"").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralString.TryParse("\"asdf\"").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralString.TryParse("\"as\\\"df\"").WasSuccessful);
        }

        [TestMethod]
        public void TestInt()
        {
            Assert.IsTrue(FactorParser.LiteralInt.TryParse("123").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralInt.TryParse("0123").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralInt.TryParse("1.2").WasSuccessful);
        }
        [TestMethod]
        public void TestDouble()
        {
            Assert.IsTrue(FactorParser.LiteralDouble.TryParse("12.12").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralDouble.TryParse("0.12").WasSuccessful);
            Assert.IsTrue(FactorParser.LiteralDouble.TryParse("-12.12").WasSuccessful);
            Assert.IsFalse(FactorParser.LiteralDouble.TryParse("-12.").WasSuccessful);
            Assert.IsFalse(FactorParser.LiteralDouble.TryParse("-.12").WasSuccessful);
        }
    }
}