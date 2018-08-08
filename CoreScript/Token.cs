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

    public class TokenVariable : Token
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
        public object Value { get; set; }

        public override TokenType TokenType => TokenType.Variable;
    }

    public class TokenTuple:Token
    {
        public override TokenType TokenType => TokenType.Tuple;
        public IList<TokenVariable> TokenVariables { get; set; }
    }

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
        public TokenTuple Parameter { get; set; }

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
        public IList<TokenVariable> Parameters { get; set; }
        public TokenBlockStement CodeBlock { get; set; }

        public TokenVariable ReturnValue { get; set; }
    }

    public enum TokenType
    {
        FunctionDefine,
        FunctionCall,
        Tuple,
        Block,
        Variable
    }
}