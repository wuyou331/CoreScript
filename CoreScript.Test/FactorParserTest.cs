using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprache;

namespace CoreScript.Test
{
    [TestClass]
    public class FactorParserTest
    {


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