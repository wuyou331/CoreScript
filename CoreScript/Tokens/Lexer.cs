using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sprache;

namespace CoreScript.Tokens
{
    public class Lexer
    {
        public static IList<Token> Analyzer(string script)
        {
            var index = 0;
            var result = new List<Token>();
            var tokens = TokenParser.FuncParser.Or<Token>(TokenParser.Assignment).Token();
            while (index<script.Length)
            {
                var source = script.Substring(index);
                var rs = tokens.TryParse(source);
                if (rs.WasSuccessful)
                {
                    rs.Value.Postion = rs.Remainder.Position;
                    rs.Value.Start = index;
                    index = rs.Value.Postion;
                    result.Add(rs.Value);
                }
                else
                {
                    throw new Exception($"期望：{rs.Expectations.First()}");
                }
            }
           
            return result;
        }
    }
}
