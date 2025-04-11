using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static laba1_compilator.Form1;

namespace laba1_compilator
{
    internal class Parser
    {
        private List<Token> _tokens;
        private int _currentTokenIndex;
        private List<ErrorInfo> _errors;

        public class ErrorInfo
        {
            public string Fragment { get; set; }
            public int Line { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public string Message { get; set; }
        }

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentTokenIndex = 0;
            _errors = new List<ErrorInfo>();
        }

        public (bool IsValid, List<ErrorInfo> Errors) Parse()
        {
            while (_currentTokenIndex < _tokens.Count)
            {
                ParseConstValDeclaration();
            }

            return (_errors.Count == 0, _errors);
        }

        private void ParseConstValDeclaration()
        {
            // Пропускаем пробелы перед каждым ожидаемым токеном
            SkipSeparators();
            ExpectToken(TokenCode.Keyword, "const");

            SkipSeparators();
            ExpectToken(TokenCode.Keyword, "val");

            SkipSeparators();
            ExpectToken(TokenCode.Identifier);

            SkipSeparators();
            ExpectToken(TokenCode.AssignOp, "=");

            SkipSeparators();
            ExpectToken(TokenCode.StringLiteral);

            SkipSeparators();
            ExpectToken(TokenCode.EndOperator, ";");
        }

        private void SkipSeparators()
        {
            while (_currentTokenIndex < _tokens.Count &&
                   _tokens[_currentTokenIndex].Code == TokenCode.Separator)
            {
                _currentTokenIndex++;
            }
        }

        private Token ExpectToken(TokenCode expectedCode, string expectedLexeme = null)
        {
            if (_currentTokenIndex >= _tokens.Count)
            {
                _errors.Add(new ErrorInfo { /* ... */ });
                return null;
            }

            var token = _tokens[_currentTokenIndex];
            if (token.Code == expectedCode && (expectedLexeme == null || token.Lexeme == expectedLexeme))
            {
                _currentTokenIndex++;
                return token;
            }
            else
            {
                _errors.Add(new ErrorInfo { /* ... */ });
                _currentTokenIndex++; // Все равно двигаемся дальше
                return token;
            }

            //if (_currentTokenIndex >= _tokens.Count)
            //{
            //    _errors.Add(new ErrorInfo
            //    {
            //        Fragment = "<EOF>",
            //        Message = $"Ожидался {expectedCode}, но достигнут конец файла",
            //        Line = _tokens.Last().Line,
            //        StartPos = _tokens.Last().EndPos + 1
            //    });
            //    return null;
            //}

            //var token = _tokens[_currentTokenIndex];

            //if (token.Code != expectedCode || (expectedLexeme != null && token.Lexeme != expectedLexeme))
            //{
            //    _errors.Add(new ErrorInfo
            //    {
            //        Fragment = token.Lexeme,
            //        Line = token.Line,
            //        StartPos = token.StartPos,
            //        EndPos = token.EndPos,
            //        Message = $"Ошибка: ожидался {expectedCode} '{expectedLexeme}', но получен {token.Code} '{token.Lexeme}'"
            //    });
            //    return token;
            //}

            //_currentTokenIndex++;
            //return token;
        }

        private void SkipToNextLine()
        {
            while (_currentTokenIndex < _tokens.Count && _tokens[_currentTokenIndex].Line == _tokens[_currentTokenIndex - 1].Line)
            {
                _currentTokenIndex++;
            }
        }
    }
}
