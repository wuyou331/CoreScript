using System.Collections.Generic;
using System.Linq;
using CoreScript.Script;

namespace CoreScript.Tokens
{
    public abstract class Token
    {
        public abstract TokenType TokenType { get; }

        public override string ToString()
        {
            return TokenType.ToString();
        }

        public int Start { get; set; }
        public int Postion { get; set; }


    }

    /// <summary>
    /// 有返回值的语句或表达式
    /// </summary>
    public interface IReturnValue
    {
        TokenType TokenType { get; }
        string DataType { get; }
    }


    
    /// <summary>
    /// 变量引用
    /// </summary>
    public class TokenVariableRef : Token, IReturnValue
    {
        public override TokenType TokenType => TokenType.VariableRef;
        public string Variable { get; set; }
        public string DataType { get; set; }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    public class TokenLiteral : Token, IReturnValue
    {
        public override TokenType TokenType => TokenType.Literal;
        public object Value { get; set; }
        public string DataType { get; set; }
    }

    /// <summary>
    /// 变量定义
    /// </summary>
    public class TokenVariableDefine : Token, IReturnValue
    {
        /// <summary>
        /// 变量类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 变量名
        /// </summary>
        public string Variable { get; set; }


        public override TokenType TokenType => TokenType.VariableDefine;
    }

    /// <summary>
    /// 元祖定义
    /// </summary>
    public class TokenTupleDefine : Token
    {
        public override TokenType TokenType => TokenType.TupleDefine;
        public IList<TokenVariableDefine> Variables { get; set; }
    }

    public class TokenTuple : Token
    {
        public override TokenType TokenType => TokenType.Tuple;
        public IList<IReturnValue> Parameters { get; set; }
    }

    /// <summary>
    /// 二元运算表达式
    /// </summary>
    public class TokenBinaryExpression : Token, IReturnValue
    {
        public IReturnValue Left { get; set; }
        public char Operator { get; set; }
        public IReturnValue Right { get; set; }
        public override TokenType TokenType => TokenType.BinaryExpression;
        public string DataType { get; }
    }

    /// <summary>
    /// 语句
    /// </summary>
    public abstract class TokenStement : Token
    {
    }

    /// <summary>
    /// 返回语句
    /// </summary>
    public class TokenReturnStement : TokenStement, IReturnValue
    {
        public override TokenType TokenType { get; } = TokenType.Return;
        public string DataType { get; }
        public IReturnValue Value{ get; set; }
    }

    public class TokenFunctionCallStement : TokenStement, IReturnValue
    {
        public override TokenType TokenType => TokenType.FunctionCall;

        /// <summary>
        /// 方法调用链
        /// </summary>
        public IList<string> CallChain { get; set; }

        public IList<IReturnValue> Parameters { get; set; }

        public string DataType { get; set; }
    }

    /// <summary>
    /// 代码块
    /// </summary>
    public class TokenBlockStement : TokenStement,IReturnValue
    {
        public override TokenType TokenType => TokenType.Block;
        public IList<TokenStement> Stements { get; set; }
        
        public bool HasReturn() => Stements?.Last().GetType() == typeof(TokenReturnStement);
        
        public string DataType { get; } = ScriptType.Void;
        public TokenVariableDefine ReturnValue { get; set; }
    }

    /// <summary>
    /// 判断表达式
    /// </summary>
    public class TokenJudgmentExpression : IReturnValue
    {
        public TokenType TokenType => TokenType.JudgmentExpression;
        public string DataType { get; } = ScriptType.Boolean;
        public IReturnValue Left { get; set; }
        public JudgmentExpressionType Operator { get; set; }
        public IReturnValue Right { get; set; }
    }

    /// <summary>
    /// if语句块
    /// </summary>
    public class TokenConditionBlock : TokenStement, IReturnValue
    {
        public override TokenType TokenType => TokenType.Condition;
        public string DataType { get; }

        public IReturnValue Condition { get; set; }
        public TokenBlockStement TrueBlock { get; set; }
        public TokenConditionBlock Else { get; set; }
    }

    /// <summary>
    /// 函数定义
    /// </summary>
    public class TokenFunctionDefine : Token
    {
        public override TokenType TokenType => TokenType.FunctionDefine;
        public string Name { get; set; }
        public TokenTupleDefine Parameters { get; set; }
        public TokenBlockStement CodeBlock { get; set; }
        public TokenVariableDefine ReturnValue { get; set; }
    }

    /// <summary>
    /// 变量初始化赋值
    /// </summary>
    public class TokenAssignment : TokenStement
    {
        public TokenAssignment(TokenType tokenType)
        {
            TokenType = tokenType;
        }

        public override TokenType TokenType { get; }
        public IReturnValue Left { get; set; }
        public IReturnValue Right { get; set; }
    }

    public enum TokenType
    {
        FunctionDefine,
        FunctionCall,

        /// <summary>
        /// 变量初始化
        /// </summary>
        AssignmentDefine,

        /// <summary>
        /// 变量赋值
        /// </summary>
        Assignment,
        Block,
        VariableDefine,
        VariableRef,
        TupleDefine,
        Tuple,
        Literal,
        Condition,

        /// <summary>
        /// 判断表达式
        /// </summary>
        JudgmentExpression,
        BinaryExpression,
        /// <summary>
        /// 返回语句
        /// </summary>
        Return
    }

    public enum JudgmentExpressionType
    {
        Equal,
        NotEqual,
        And,
        Or
    }
}