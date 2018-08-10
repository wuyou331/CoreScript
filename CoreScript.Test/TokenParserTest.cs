using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sprache;

namespace CoreScript.Test
{
    [TestClass]
    public class TokenParserTest
    {
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