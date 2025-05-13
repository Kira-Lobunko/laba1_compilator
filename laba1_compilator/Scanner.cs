//using System;
//using System.Collections.Generic;
//using System.Text;
//using static laba1_compilator.Form1;  // Для доступа к TokenCode и Token

//namespace laba1_compilator
//{
//    public class Scanner
//    {
//        private string _text;
//        private int _pos;
//        private int _line;
//        private int _linePos;
//        private List<Token> _tokens;
//        private bool _errorOccurred;
//        private string _errorMessage;

//        public List<Token> Tokens => _tokens;
//        public string ErrorMessage => _errorMessage;

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
//            _errorOccurred = false;
//            _errorMessage = null;

//            // 1) Первое ключевое слово: const
//            ReadKeyword("const");
//            if (_errorOccurred) return _tokens;

//            SkipAndAddSeparators();

//            // 2) Второе ключевое слово: val
//            ReadKeyword("val");
//            if (_errorOccurred) return _tokens;

//            SkipAndAddSeparators();

//            // 3) Идентификатор
//            ReadIdentifier();
//            if (_errorOccurred) return _tokens;

//            SkipAndAddSeparators();

//            // 4) Оператор присваивания =
//            ReadAssignOp();
//            if (_errorOccurred) return _tokens;

//            SkipAndAddSeparators();

//            // 5) Значение
//            ReadValue();
//            if (_errorOccurred) return _tokens;

//            SkipAndAddSeparators();

//            // 6) Конец оператора ;
//            ReadEndOperator();
//            return _tokens;
//        }

//        private void ReadKeyword(string expected)
//        {
//            int startPos = _linePos;
//            var sb = new StringBuilder();
//            while (!IsEnd() && char.IsLetter(CurrentChar))
//            {
//                sb.Append(CurrentChar);
//                Advance();
//            }
//            string word = sb.ToString();
//            if (word == expected)
//            {
//                AddToken(TokenCode.Keyword, "ключевое слово", word, startPos, _linePos - 1, _line);
//            }
//            else
//            {
//                SetError($"Ожидалось ключевое слово {expected}");
//            }
//        }

//        private void ReadIdentifier()
//        {
//            // Первый символ должен быть латинской буквой или '_'
//            if (IsEnd() || !(IsLatinLetter(CurrentChar) || CurrentChar == '_'))
//            {
//                SetError("Ожидалось идентификатор");
//                return;
//            }

//            int startPos = _linePos;
//            var sb = new StringBuilder();

//            // Считываем все корректные символы идентификатора
//            while (!IsEnd() && IsAllowedIdentifierChar(CurrentChar))
//            {
//                sb.Append(CurrentChar);
//                Advance();
//            }

//            // Добавляем токен из корректной части
//            AddToken(TokenCode.Identifier, "идентификатор", sb.ToString(), startPos, _linePos - 1, _line);

//            // Если следующий символ — любая буква или прочий недопустимый знак,
//            // сразу сообщаем об ошибке без упоминания "в идентификаторе"
//            if (!IsEnd() && !IsDelimiter(CurrentChar))
//            {
//                char bad = CurrentChar;
//                SetError($"Недопустимый символ '{bad}'");
//            }
//        }

//        private void ReadAssignOp()
//        {
//            if (IsEnd() || CurrentChar != '=')
//            {
//                SetError("Ожидался знак присваивания '='");
//                return;
//            }
//            int pos = _linePos;
//            AddToken(TokenCode.AssignOp, "оператор присваивания", "=", pos, pos, _line);
//            Advance();
//        }

//        private void ReadValue()
//        {
//            if (IsEnd())
//            {
//                SetError("Ожидалось значение");
//                return;
//            }

//            if (CurrentChar == '"')
//            {
//                int startPos = _linePos;
//                Advance(); // пропустить кавычку
//                var sb = new StringBuilder();
//                while (!IsEnd() && CurrentChar != '"')
//                {
//                    sb.Append(CurrentChar);
//                    Advance();
//                }
//                if (IsEnd())
//                {
//                    SetError("Ожидалась закрывающая кавычка '\"'");
//                    return;
//                }
//                Advance(); // закрывающая кавычка
//                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else if (char.IsDigit(CurrentChar))
//            {
//                int startPos = _linePos;
//                var sb = new StringBuilder();
//                while (!IsEnd() && char.IsDigit(CurrentChar))
//                {
//                    sb.Append(CurrentChar);
//                    Advance();
//                }
//                AddToken(TokenCode.Integer, "целое число", sb.ToString(), startPos, _linePos - 1, _line);
//            }
//            else if (char.IsLetter(CurrentChar))
//            {
//                SetError("Ожидалась открывающаяся кавычка");
//            }
//            else
//            {
//                SetError("Недопустимый символ внутри значения");
//            }
//        }

//        private void ReadEndOperator()
//        {
//            if (IsEnd() || CurrentChar != ';')
//            {
//                SetError("Ожидался конец оператора ';'");
//                return;
//            }
//            int pos = _linePos;
//            AddToken(TokenCode.EndOperator, "конец оператора", ";", pos, pos, _line);
//            Advance();
//        }

//        private void SkipAndAddSeparators()
//        {
//            while (!IsEnd() && char.IsWhiteSpace(CurrentChar))
//            {
//                if (CurrentChar == ' ')
//                    AddToken(TokenCode.Separator, "разделитель", "(пробел)", _linePos, _linePos, _line);
//                Advance();
//            }
//        }

//        private bool IsLatinLetter(char ch)
//            => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');

//        private bool IsAllowedIdentifierChar(char ch)
//            => IsLatinLetter(ch) || char.IsDigit(ch) || ch == '_';

//        private bool IsDelimiter(char ch)
//            => char.IsWhiteSpace(ch) || ch == '=' || ch == ';' || ch == '\0';

//        private void SetError(string message)
//        {
//            if (!_errorOccurred)
//            {
//                _errorOccurred = true;
//                _errorMessage = message;
//            }
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

//        private char CurrentChar => _pos < _text.Length ? _text[_pos] : '\0';
//        private bool IsEnd() => _pos >= _text.Length;

//        private void Advance()
//        {
//            if (CurrentChar == '\n')
//            {
//                _line++;
//                _linePos = 0;
//            }
//            _pos++;
//            _linePos++;
//        }
//    }
//}
