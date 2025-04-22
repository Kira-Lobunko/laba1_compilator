//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static laba1_compilator.Form1;

//namespace laba1_compilator
//{
//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        private HashSet<string> _keywords = new HashSet<string> { "const", "val" };

//        private bool _expectIdentifier = false;

//        public Scanner()
//        {
//            _tokens = new List<Token>();
//        }

//        public List<Token> Scan(string text)
//        {
//            _text = text;
//            _pos = 0;
//            _line = 1;
//            _linePos = 1;
//            _tokens.Clear();

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();

//                switch (ch)
//                {
//                    case '\r':
//                    case '\n':
//                        Advance();
//                        break;

//                    case var c when char.IsWhiteSpace(c):
//                        if (c == ' ')
//                        {
//                            AddToken(TokenCode.Separator, "разделитель", "(пробел)");
//                        }
//                        Advance();
//                        break;

//                    case var c when (char.IsLetter(c) || c == '_'):
//                        ReadMixedIdentifierOrError();
//                        break;

//                    case var c when char.IsDigit(c):
//                        ReadInteger();
//                        break;

//                    case '=':
//                        AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
//                        Advance();
//                        break;

//                    case ';':
//                        AddToken(TokenCode.EndOperator, "конец оператора", ";");
//                        Advance();
//                        break;

//                    case '"':
//                        ReadStringLiteral();
//                        break;

//                    default:
//                        int startErrorPos = _linePos;
//                        StringBuilder errorSb = new StringBuilder();
//                        while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
//                        {
//                            errorSb.Append(CurrentChar());
//                            Advance();
//                        }
//                        AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), startErrorPos, _linePos - 1, _line);
//                        break;
//                }
//            }

//            return _tokens;
//        }

//        private bool IsEnd()
//        {
//            return _pos >= _text.Length;
//        }

//        private char CurrentChar()
//        {
//            return _pos < _text.Length ? _text[_pos] : '\0';
//        }

//        private void Advance()
//        {
//            if (CurrentChar() == '\n')
//            {
//                _line++;
//                _linePos = 0;
//            }
//            _pos++;
//            _linePos++;
//        }

//        private void AddToken(TokenCode code, string type, string lexeme)
//        {
//            AddToken(code, type, lexeme, _linePos, _linePos, _line);
//        }

//        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
//        {
//            _tokens.Add(new Token
//            {
//                Code = code,
//                Type = type,
//                Lexeme = lexeme,
//                StartPos = startPos,
//                EndPos = endPos,
//                Line = line
//            });
//        }

//        private bool IsValidTokenStart(char ch)
//        {
//            return ch == '\r' || ch == '\n' || char.IsWhiteSpace(ch) || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || char.IsDigit(ch) || ch == '_' || ch == '=' || ch == ';' || ch == '"';
//        }

//        private void ReadMixedIdentifierOrError()
//        {
//            int tokenStartPos = _linePos;
//            StringBuilder wordSb = new StringBuilder();

//            while (!IsEnd() && (char.IsLetter(CurrentChar()) || char.IsDigit(CurrentChar()) || CurrentChar() == '_'))
//            {
//                wordSb.Append(CurrentChar());
//                Advance();
//            }

//            string word = wordSb.ToString();
//            int segmentStart = 0;
//            bool currentAllowed = IsAllowedIdentifierChar(word[0]);

//            for (int i = 1; i < word.Length; i++)
//            {
//                bool allowed = IsAllowedIdentifierChar(word[i]);
//                if (allowed != currentAllowed)
//                {
//                    string segment = word.Substring(segmentStart, i - segmentStart);
//                    TokenCode code = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//                    string type = currentAllowed ? "идентификатор" : "недопустимый символ";

//                    if (currentAllowed && _keywords.Contains(segment))
//                    {
//                        code = TokenCode.Keyword;
//                        type = "ключевое слово";
//                        _expectIdentifier = true;
//                    }
//                    else if (_expectIdentifier && currentAllowed)
//                    {
//                        _expectIdentifier = false;
//                    }
//                    AddToken(code, type, segment, tokenStartPos + segmentStart, tokenStartPos + i - 1, _line);

//                    segmentStart = i;
//                    currentAllowed = allowed;
//                }
//            }

//            string lastSegment = word.Substring(segmentStart);
//            TokenCode lastCode = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
//            string lastType = currentAllowed ? "идентификатор" : "недопустимый символ";
//            if (currentAllowed && _keywords.Contains(lastSegment))
//            {
//                lastCode = TokenCode.Keyword;
//                lastType = "ключевое слово";
//                _expectIdentifier = true;
//            }
//            else if (_expectIdentifier && currentAllowed)
//            {
//                _expectIdentifier = false;
//            }
//            AddToken(lastCode, lastType, lastSegment, tokenStartPos + segmentStart, tokenStartPos + word.Length - 1, _line);
//        }

//        private void ReadInteger()
//        {
//            int startPos = _linePos;
//            StringBuilder sb = new StringBuilder();
//            sb.Append(CurrentChar());
//            Advance();

//            while (!IsEnd() && char.IsDigit(CurrentChar()))
//            {
//                sb.Append(CurrentChar());
//                Advance();
//            }

//            string lexeme = sb.ToString();
//            AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
//        }

//        private void ReadStringLiteral()
//        {
//            int startPos = _linePos;
//            Advance();
//            StringBuilder sb = new StringBuilder();
//            bool closed = false;

//            while (!IsEnd())
//            {
//                char ch = CurrentChar();
//                if (ch == '"')
//                {
//                    closed = true;
//                    Advance();
//                    break;
//                }
//                else
//                {
//                    sb.Append(ch);
//                    Advance();
//                }
//            }

//            if (closed)
//            {
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else
//            {
//                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
//                while (!IsEnd())
//                {
//                    AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), _linePos, _linePos, _line);
//                    Advance();
//                }
//            }
//        }

//        private bool IsAllowedIdentifierChar(char ch)
//        {
//            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || (ch == '_');
//        }
//    }
//}
