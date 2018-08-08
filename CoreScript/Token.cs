using System;
using System.Collections.Generic;
using System.Text;

namespace CoreScript
{
    public class Token
    {
        public string TokenType { get; set; }

        public string Value { get; set; }

        public int Postion { get; set; }
    }

    public enum TokenType
    {
        Keyword,
        Identifier,
        Tuple,
        Dot,
        Comma,
        ParenBegin,
        ParenEnd,
        CurlyBegin,
        CurlyClose,
        String,
        Int,
        Double
    }
}