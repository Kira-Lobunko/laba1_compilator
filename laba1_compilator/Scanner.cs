using System;
using System.Collections.Generic;
using System.Text;
using static laba1_compilator.Form1;  // Чтобы видеть Token и TokenCode

namespace laba1_compilator
{
    public class Scanner
    {
        private string _text;
        private int _pos;
        private int _line;
        private int _linePos;
        private List<Token> _tokens;
        private bool _errorOccurred;
        private string _errorMessage;

        public List<Token> Tokens => _tokens;
        public string ErrorMessage => _errorMessage;

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        public List<Token> Scan(string text)
        {
            _text = text;
            _pos = 0;
            _line = 1;
            _linePos = 1;
            _tokens.Clear();
            _errorOccurred = false;
            _errorMessage = null;

            while (!IsEnd())
            {
                char c = CurrentChar;

                // 1) Пробелы → Separator
                if (char.IsWhiteSpace(c))
                {
                    if (c == ' ')
                        AddToken(TokenCode.Separator, "разделитель", "(пробел)", _linePos, _linePos, _line);
                    Advance();
                }
                // 2) Идентификатор или ключевое слово
                else if (char.IsLetter(c) || c == '_')
                {
                    int start = _linePos;
                    var sb = new StringBuilder();
                    while (!IsEnd() && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
                    {
                        sb.Append(CurrentChar);
                        Advance();
                    }
                    string word = sb.ToString();
                    if (word == "begin" || word == "end")
                        AddToken(TokenCode.Keyword, "ключевое слово", word, start, _linePos - 1, _line);
                    else
                        AddToken(TokenCode.Identifier, "идентификатор", word, start, _linePos - 1, _line);
                }
                // 3) Число
                else if (char.IsDigit(c))
                {
                    int start = _linePos;
                    var sb = new StringBuilder();
                    while (!IsEnd() && char.IsDigit(CurrentChar))
                    {
                        sb.Append(CurrentChar);
                        Advance();
                    }
                    AddToken(TokenCode.Integer, "число", sb.ToString(), start, _linePos - 1, _line);
                }
                // 4) Операторы и разделители
                else
                {
                    int start = _linePos;

                    // 4.1) оператор ":="
                    if (c == ':' && PeekChar() == '=')
                    {
                        Advance(); // ':'
                        Advance(); // '='
                        AddToken(TokenCode.AssignOp, "оператор присваивания", ":=", start, _linePos - 1, _line);
                    }
                    else
                    {
                        switch (c)
                        {
                            case '=':
                                // одиночный '=' — ошибка
                                SetError("Одиночный '=' не разрешён, ожидался оператор ':='");
                                Advance();
                                break;

                            case ';':
                                Advance();
                                AddToken(TokenCode.EndOperator, "конец оператора", ";", start, start, _line);
                                break;

                            case '+':
                                Advance();
                                AddToken(TokenCode.PlusOp, "оператор \"+\"", "+", start, start, _line);
                                break;

                            case '*':
                                Advance();
                                AddToken(TokenCode.MulOp, "оператор \"*\"", "*", start, start, _line);
                                break;

                            case '(':
                                Advance();
                                AddToken(TokenCode.LParen, "скобка \"(\"", "(", start, start, _line);
                                break;

                            case ')':
                                Advance();
                                AddToken(TokenCode.RParen, "скобка \")\"", ")", start, start, _line);
                                break;

                            default:
                                SetError($"Недопустимый символ '{c}'");
                                Advance();
                                break;
                        }
                    }
                }
            }

            return _tokens;
        }

        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
        {
            _tokens.Add(new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                StartPos = startPos,
                EndPos = endPos,
                Line = line
            });
        }

        private void SetError(string message)
        {
            if (!_errorOccurred)
            {
                _errorOccurred = true;
                _errorMessage = message;
            }
        }

        private char CurrentChar => _pos < _text.Length ? _text[_pos] : '\0';
        private bool IsEnd() => _pos >= _text.Length;

        private void Advance()
        {
            if (CurrentChar == '\n')
            {
                _line++;
                _linePos = 0;
            }
            _pos++;
            _linePos++;
        }

        private char PeekChar()
            => (_pos + 1 < _text.Length) ? _text[_pos + 1] : '\0';
    }
}
