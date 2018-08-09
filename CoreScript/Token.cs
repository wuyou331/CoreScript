using System;
using System.Collections.Generic;
using System.Text;

namespace CoreScript
{
    public abstract class Token
    {
        public  abstract TokenType TokenType { get;  }
        public override string ToString()
        {
            return TokenType.ToString();
        }
    }

    /// <summary>
    /// 字面量或者变量引用
    /// </summary>
    public abstract class TokenValue : Token
    {
    }


    /// <summary>
    /// 变量引用
    /// </summary>
    public class TokenVariableRef : TokenValue
    {
        public override TokenType TokenType => TokenType.Identifier;
        public string Variable { get; set; }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    public class TokenLiteral : TokenValue
    {
        public override TokenType TokenType => TokenType.Literal;
        public string DateType { get; set; }
        public string Value { get; set; }
    }

    public class TokenVariableDefine : Token
    {
        /// <summary>
        /// 变量类型
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 变量名
        /// </summary>
        public string Variable { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public TokenValue Value { get; set; }

        public override TokenType TokenType => TokenType.VariableDefine;
    }


    /// <summary>
    /// 语句
    /// </summary>
    public abstract class TokenStement : Token
    {
      

    }

    public class TokenFunctionCallStement : TokenStement
    {
        public override TokenType TokenType => TokenType.FunctionCall;
        /// <summary>
        /// 方法调用链
        /// </summary>
        public IList<string> CallChain { get; set; }
        public IList<TokenValue> Parameters { get; set; }

    }

    public class TokenBlockStement : TokenStement
    {
        public override TokenType TokenType => TokenType.FunctionCall;
        public IList<TokenStement> Stements { get; set; }
    }

    public class TokenFunctionDefine : Token
    {
        public override TokenType TokenType => TokenType.FunctionDefine;
        public string Name { get; set; }
        public IList<TokenVariableDefine> Parameters { get; set; }
        public TokenBlockStement CodeBlock { get; set; }

        public TokenVariableDefine ReturnValue { get; set; }
    }

    public enum TokenType
    {
        FunctionDefine,
        FunctionCall,
        Block,
        VariableDefine,

        Identifier,
        Literal
    }
}