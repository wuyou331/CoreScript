using System.Collections.Generic;

namespace CoreScript.Tokens
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
    public  interface ITokenValue 
    {
         string DateType { get; set; }
    }


    /// <summary>
    /// 变量引用
    /// </summary>
    public class TokenVariableRef : Token, ITokenValue
    {
        public override TokenType TokenType => TokenType.Identifier;
        public string Variable { get; set; }
        public string DateType { get; set; }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    public class TokenLiteral : Token, ITokenValue
    {
        public override TokenType TokenType => TokenType.Literal;
        public object Value { get; set; }
        public string DateType { get; set; }
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
        public ITokenValue Value { get; set; }

        public override TokenType TokenType => TokenType.VariableDefine;
    }

    public class TokenTupleDefine: Token
    {
        public override TokenType TokenType => TokenType.TupleDefine;
        public IList<TokenVariableDefine> Values { get; set; }
    }

    public class TokenTuple : Token
    {
        public override TokenType TokenType => TokenType.Tuple;
        public IList<ITokenValue> Parameters { get; set; }
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
        public IList<ITokenValue> Parameters { get; set; }

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
        public TokenTupleDefine Parameters { get; set; }
        public TokenBlockStement CodeBlock { get; set; }

        public TokenVariableDefine ReturnValue { get; set; }
    }

    public enum TokenType
    {
        FunctionDefine,
        FunctionCall,
        Block,
        VariableDefine,
        TupleDefine,
        Tuple,
        Identifier,
        Literal
    }
}