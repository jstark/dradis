using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.frontend
{
    public enum TokenType
    {
        INVALID,
        // Reserved
        BEGIN_RESERVED,
        AND = BEGIN_RESERVED, ARRAY, BEGIN, CASE, CONST,
        DIV, DO, DOWNTO, ELSE, END,
        FILE, FOR, FUNCTION, GOTO, IF, IN,
        LABEL, MOD, NIL, NOT, OF, OR,
        PACKED, PROCEDURE, PROGRAM, RECORD, REPEAT,
        SET, THEN, TO, TYPE, UNTIL, VAR, WHILE, WITH,
        END_RESERVED = WITH,
        // Special
        PLUS, MINUS, STAR, SLASH, COLON_EQUALS, DOT, 
        COMMA, SEMICOLON, COLON, QUOTE, 
        EQUALS, NOT_EQUALS, LESS_THAN, LESS_EQUALS,
        GREATER_EQUALS, GREATER_THAN, LEFT_PAREN, RIGHT_PAREN,
        LEFT_BRACKET, RIGHT_BRACKET, LEFT_BRACE, RIGHT_BRACE,
        UP_ARROW, DOT_DOT, 
        // Other
        IDENTIFIER, INTEGER, REAL, STRING, ERROR, END_OF_FILE
    }

    public static class TokenTypeInfo
    {
        private static Dictionary<string, TokenType> special;

        static TokenTypeInfo()
        {
            special = new Dictionary<string, TokenType>()
            {
                {"+",   TokenType.PLUS            },
                {"-",   TokenType.MINUS           }, 
                {"*",   TokenType.STAR            },
                {"/",   TokenType.SLASH           },
                {":=",  TokenType.COLON_EQUALS    },
                {".",   TokenType.DOT             },
                {",",   TokenType.COMMA           },
                {";",   TokenType.SEMICOLON       }, 
                {":",   TokenType.COLON           },
                {"'",   TokenType.QUOTE           }, 
                {"=",   TokenType.EQUALS          },
                {"<>",  TokenType.NOT_EQUALS      },
                {"<",   TokenType.LESS_THAN       }, 
                {"<=",  TokenType.LESS_EQUALS     }, 
                {">=",  TokenType.GREATER_EQUALS  },
                {">",   TokenType.GREATER_THAN    },
                {"(",   TokenType.LEFT_PAREN      }, 
                {")",   TokenType.RIGHT_PAREN     },
                {"[",   TokenType.LEFT_BRACKET    },
                {"]",   TokenType.RIGHT_BRACKET   }, 
                {"{",   TokenType.LEFT_BRACE      },
                {"}",   TokenType.RIGHT_BRACE     },
                {"^",   TokenType.UP_ARROW        }, 
                {"..",  TokenType.DOT_DOT         },
            };
        }

        public static string GetName(TokenType tt)
        {
            return tt.ToString();
        }

        public static bool IsSpecialSymbol(string str)
        {
            return special.ContainsKey(str);
        }

        public static TokenType ReservedTypeFromString(string str)
        {   
            TokenType reserved = TokenType.INVALID;
            if (Enum.TryParse(str, true, out reserved))
            {
                if (reserved > TokenType.END_RESERVED)
                {
                    reserved = TokenType.INVALID;
                }
            }
            return reserved;
        }

        public static TokenType SpecialTypeFromString(string str)
        {
            TokenType type = TokenType.INVALID;
            special.TryGetValue(str, out type);
            return type;
        }
    }

    public class Token
    {
        internal Token() { }

        public string Lexeme { get; internal set; }

        public int LineNumber { get; internal set; }

        public int Position { get; internal set; }

        public TokenType TokenType { get; internal set; }

        public object Value { get; internal set; }

        public bool IsEof { get { return TokenType == TokenType.END_OF_FILE; } }
    }

    public sealed class TokenBuilder
    {
        private string lexeme = "";
        private int line;
        private int pos;
        private TokenType type = TokenType.INVALID;
        private object val;

        public TokenBuilder CreateTokenWithType(TokenType tp)
        {
            Contract.Requires(type == TokenType.INVALID);
            type = tp;
            return this;
        }

        public TokenBuilder WithLexeme(string text)
        {
            Contract.Requires(lexeme.Equals(""));
            lexeme = text;
            return this;
        }

        public TokenBuilder WithValue(object value)
        {
            val = value;
            return this;
        }

        public TokenBuilder AtLine(int num)
        {
            Contract.Requires(num > 0);
            line = num;
            return this;
        }

        public TokenBuilder AtPosition(int p)
        {
            Contract.Requires(p > 0);
            pos = p;
            return this;
        }

        public Token Build()
        {
            return new Token()
            {
                Lexeme = lexeme,
                LineNumber = line,
                Position = pos,
                Value = val,
                TokenType = type
            };
        }
    }
}
