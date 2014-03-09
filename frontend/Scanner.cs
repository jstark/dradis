using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.frontend
{
    public class Scanner
    {
        private Source source;

        public Scanner(Source s)
        {
            Contract.Requires(s != null);
            source = s;
        }

        public Token CurrentToken { get; private set; }

        public Token GetNextToken()
        {
            CurrentToken = ExtractToken();
            return CurrentToken;
        }

        private Token ExtractWordToken()
        {
            char current = source.GetCurrentChar();
    
            int line = source.LineNumber;
            int pos = source.Position;
    
            // Get the word characters (letter or digit). The scanner has already
            // determined that the first character is a letter.
            StringBuilder text = new StringBuilder();
            while (Char.IsLetterOrDigit(current))
            {
                text.Append(current);
                current = source.GetNextChar();
            }
    
            // is it a reserved word or an identifier ?
            string lexeme = text.ToString();
            TokenType type = TokenTypeInfo.ReservedTypeFromString(lexeme);
            if (type == TokenType.INVALID)
            {
                type = TokenType.IDENTIFIER;
            }
            
            return new TokenBuilder()
                        .CreateTokenWithType(type)
                        .AtLine(line)
                        .AtPosition(pos)
                        .WithLexeme(lexeme)
                        .Build();
        }

        private Token ExtractStringToken()
        {
            int line = source.LineNumber;
            int pos = source.Position;
    
            StringBuilder lexeme = new StringBuilder();
            StringBuilder value = new StringBuilder();
    
            char current = source.GetNextChar(); // consume initial quote
            lexeme.Append('\'');
    
            // deal with string characters
            do
            {
                // replace any whitespace character with a blank
                if (Char.IsWhiteSpace(current))
                {
                    current = ' ';
                }
                
                if (current != '\'' && current != Source.END_OF_FILE)
                {
                    lexeme.Append(current);
                    value.Append(current);
                    current = source.GetNextChar(); // again consume
                }
                
                // quote ? each pair of adjacent quotes represent a single quote.
                if (current == '\'')
                {
                    while (current == '\'' && source.PeekNextChar() == '\'')
                    {
                        lexeme.Append("''");
                        value.Append('\'');
                        current = source.GetNextChar();
                        current = source.GetNextChar(); // consume both quotes
                    }
                }
            } while(current != '\'' && current != Source.END_OF_FILE);
            
            TokenBuilder builder = new TokenBuilder();
            if (current == '\'')
            {
                source.GetNextChar(); // consume final quote
                lexeme.Append('\'');
                builder.CreateTokenWithType(TokenType.STRING).WithValue(value.ToString()).WithLexeme(lexeme.ToString());
            } else
            {
                builder.CreateTokenWithType(TokenType.ERROR).WithValue(ErrorCode.UNEXPECTED_EOF);
            }
            return builder.AtPosition(pos).AtLine(line).Build();
        }

        private Token ExtractSpecialToken()
        {
            char current = source.GetCurrentChar();
            
            StringBuilder lexeme = new StringBuilder();
            lexeme.Append(current);
            
            int pos = source.Position;
            int line= source.LineNumber;

            string value = null;
            TokenType type = TokenType.INVALID;
            
            switch (current) {
                case '+': case '-': case '*': case '/': case ',':
                case ';': case '\'': case '=': case '(': case ')':
                case '[': case ']': case '{': case '}': case '^':
                    source.GetNextChar();
                    break;
                case ':':
                    current = source.GetNextChar(); // consume ':'
                    if (current == '=')
                    {
                        lexeme.Append(current);
                        source.GetNextChar(); // consume '='
                    }
                    break;
                case '<':
                    current = source.GetNextChar();
                    if (current == '=')
                    {
                        lexeme.Append(current);
                        source.GetNextChar();
                    } else if (current == '>')
                    {
                        lexeme.Append(current);
                        source.GetNextChar();
                    }                   
                    break;
                case '>':
                    current = source.GetNextChar();
                    if (current == '=')
                    {
                        lexeme.Append(current);
                        source.GetNextChar();
                    }
                    break;
                case '.':
                    current = source.GetNextChar();
                    if (current == '.')
                    {
                        lexeme.Append(current);
                        source.GetNextChar();
                    }
                    break;
                default:
                    source.GetNextChar();
                    type = TokenType.ERROR;
                    value = ErrorCode.INVALID_CHARACTER.Message;
                    break;
            }
            
            if (type == TokenType.INVALID)
            {
                type = TokenTypeInfo.SpecialTypeFromString(lexeme.ToString());
            }
            
            TokenBuilder builder = new TokenBuilder();
            return builder.CreateTokenWithType(type).WithLexeme(lexeme.ToString())
                          .WithValue(value)
                          .AtPosition(pos)
                          .AtLine(line).Build();
        }

        #region numbers

        private Tuple<TokenType, string, object, string> UnsignedDigits()
        {
            char current = source.GetCurrentChar();
    
            StringBuilder text = new StringBuilder();
            object val = null;
            TokenType type = TokenType.INVALID;
            StringBuilder digits = new StringBuilder();

            // must have at least one digit
            if (!Char.IsDigit(current))
            {
                type = TokenType.ERROR;
                val = ErrorCode.INVALID_NUMBER;
            } else
            {
                while (Char.IsDigit(current))
                {
                    text.Append(current);
                    digits.Append(current);
                    current = source.GetNextChar(); // consume digit
                }
            }
            
            return Tuple.Create(type, text.ToString(), val, digits.ToString());
        }

        private Tuple<TokenType, int, object> ComputeIntVal(string digits)
        {
            int val = 0;
            if (digits.Length != 0)
            {
                int prev = -1;
                int index= 0;
                
                // loop over the digits to compute the integer value
                // as long as there is no overflow.
                while (index < digits.Length && val >= prev)
                {
                    prev = val;
                    val = 10 * val + (digits[index++] - '0');
                }
                
                // no overflow: return the int value
                if (val >= prev)
                {
                    return Tuple.Create<TokenType, int, object>(TokenType.INTEGER, val, null);
                } else
                {
                    return Tuple.Create<TokenType, int, object>(TokenType.ERROR, 0, ErrorCode.RANGE_INTEGER);
                }
            }
            return Tuple.Create<TokenType, int, object>(TokenType.INTEGER, 0, null);
        }

        private Tuple<TokenType, double, object> ComputeDoubleVal(string whole, string frac, string expd, char sign)
        {
            var ival = ComputeIntVal(expd);
            TokenType err = ival.Item1;

            if (err == TokenType.ERROR)
            {
                return Tuple.Create(err, 0.0, (object)ErrorCode.INVALID_NUMBER);
            }

            int exp_val = ival.Item2;
            if (sign == '-')
            {
                exp_val = -exp_val;
            }
            
            StringBuilder digits = new StringBuilder(whole);
            if (frac.Length != 0)
            {
                exp_val -= frac.Length;
                digits.Append(frac);
            }
            
            // out of range check
            int x = Math.Abs(exp_val + (int)whole.Length);
            if (x > 308)
            {
                return Tuple.Create(TokenType.ERROR, 0.0, (object)ErrorCode.RANGE_REAL);
            }
            
            // loop over the digits to compute the double value
            int index = 0;
            double dval = 0;
            while (index < digits.Length)
            {
                dval = 10 * dval + (digits[index++] - '0');
            }
            
            // adjust due to exponent
            if (exp_val != 0)
            {
                dval *= Math.Pow(10.0, exp_val);
            }
            
            return Tuple.Create(TokenType.REAL, dval, (object)0);
        }

        private Token ExtractNumberToken()
        {
            int line = source.LineNumber;
            int pos = source.Position;

            var uval = UnsignedDigits();
            
            TokenType err = uval.Item1;
            string lexeme = uval.Item2;
            object value  = uval.Item3;
            string whole_digits = uval.Item4;

            if (err == TokenType.ERROR)
            {
                TokenBuilder builder = new TokenBuilder();
                return builder.CreateTokenWithType(err)
                              .WithLexeme(lexeme)
                              .AtPosition(pos)
                              .AtLine(line)
                              .WithValue(value)
                              .Build();
            }
            
            // assume int
            TokenType type = TokenType.INTEGER;
            
            // Is there a . ?
            // It could be a decimal point or the start of a .. token
            char current = source.GetCurrentChar();
            bool saw_dot_dot = false;
            string frac_digits = "";
            if (current == '.')
            {
                if (source.PeekNextChar() == '.')
                {
                    saw_dot_dot = true; // it's a ".." token, so don't consume it
                } else
                {
                    type = TokenType.REAL; // decimal point, so token type is REAL.
                    lexeme += current;
                    current = source.GetNextChar(); // consume decimal point
                    
                    // collect fraction part of the number
                    var uval2 = UnsignedDigits();
                    err = uval2.Item1;
                    string frac_lexeme = uval2.Item2;
                    lexeme += frac_lexeme;
                    value = uval2.Item3;
                    frac_digits = uval2.Item4;

                    if (err == TokenType.ERROR)
                    {
                        TokenBuilder builder = new TokenBuilder();
                        return builder.CreateTokenWithType(err)
                                      .WithLexeme(lexeme)
                                      .AtPosition(pos)
                                      .AtLine(line)
                                      .WithValue(value)
                                      .Build();
                    }
                }
            }
            
            // is there an exponent part ?
            // There cannot be an exponent if we already saw a ".." token.
            current = source.GetCurrentChar();
            char exponent_sign = '+';
            string exp_digits = "";
            if (!saw_dot_dot && (current == 'E' || current == 'e'))
            {
                type = TokenType.REAL;
                lexeme += current;
                current = source.GetNextChar(); // consume 'E' or 'e'
                
                // exponent sign ?
                if (current == '+' || current == '-')
                {
                    lexeme += current;
                    exponent_sign = current;
                    current = source.GetNextChar();
                    
                }
                
                // extract the digits of the exponent.
                err = TokenType.INVALID;
                var uval3 = UnsignedDigits();
                err = uval3.Item1;
                exp_digits = uval3.Item2;
                value = uval3.Item3;

                lexeme += exp_digits;
                if (err == TokenType.ERROR)
                {
                     TokenBuilder builder = new TokenBuilder();
                     return builder.CreateTokenWithType(err)
                                   .WithLexeme(lexeme)
                                   .AtPosition(pos)
                                   .AtLine(line)
                                   .WithValue(value)
                                   .Build();
                }
            }
            
            // compute the value of an integer number token
            if (type == TokenType.INTEGER)
            {
                var uval4 = ComputeIntVal(whole_digits);
                err = uval4.Item1;
                int val = uval4.Item2;
                value = uval4.Item3;
                if (err == TokenType.ERROR)
                {
                     TokenBuilder builder = new TokenBuilder();
                     return builder.CreateTokenWithType(err)
                                   .WithLexeme(lexeme)
                                   .AtPosition(pos)
                                   .AtLine(line)
                                   .WithValue(value)
                                   .Build();
                } else
                {
                    value = val;
                }
            } else if (type == TokenType.REAL)
            {
                var dval1 = ComputeDoubleVal(whole_digits, frac_digits,
                                                exp_digits, exponent_sign);
                err = dval1.Item1;
                double dval = dval1.Item2;
                value = dval1.Item3;
                if (err == TokenType.ERROR)
                {
                    TokenBuilder builder = new TokenBuilder();
                    return builder.CreateTokenWithType(err)
                                  .WithLexeme(lexeme)
                                  .AtPosition(pos)
                                  .AtLine(line)
                                  .WithValue(value)
                                  .Build();
                } else
                {
                    value = dval;
                }
            }
            
            TokenBuilder builder0 = new TokenBuilder();
            return builder0.CreateTokenWithType(err)
                          .WithLexeme(lexeme)
                          .AtPosition(pos)
                          .AtLine(line)
                          .WithValue(value)
                          .Build();
        }
        
        #endregion numbers

        private Token ExtractToken()
        {
            SkipWhitespace();

            char current = source.GetCurrentChar();
            Token token;
            if (current == Source.END_OF_FILE)
            {
                TokenBuilder builder = new TokenBuilder();
                token = builder.CreateTokenWithType(TokenType.END_OF_FILE)
                              .AtLine(source.LineNumber)
                              .AtPosition(source.Position)
                              .Build();
            } else if (Char.IsLetter(current))
            {
                token = ExtractWordToken();
            } else if (Char.IsDigit(current))
            {
                token = ExtractNumberToken();
            } else if (current == '\'')
            {
                token = ExtractStringToken();
            } else if (TokenTypeInfo.IsSpecialSymbol(current.ToString()))
            {
                token = ExtractSpecialToken();
            } else
            {
                TokenBuilder builder = new TokenBuilder();
                token = builder.CreateTokenWithType(TokenType.ERROR) 
                               .WithLexeme(current.ToString())
                               .WithValue(ErrorCode.INVALID_CHARACTER)
                               .AtLine(source.LineNumber)
                               .AtPosition(source.Position)
                               .Build();
                source.GetNextChar();
            }
            return token;
        }

        private void SkipWhitespace()
        {
            char current = source.GetCurrentChar();
            while (Char.IsWhiteSpace(current) || current == '{')
            {
                if (current == '{')
                {
                    do
                    {
                        current = source.GetNextChar();
                    } while (current != '}' && current != Source.END_OF_FILE);

                    // found closing '}' ?
                    if (current == '}')
                    {
                        current = source.GetNextChar();
                    }
                } 
                else
                {
                    current = source.GetNextChar();
                }
            }
        }
    }
}
