using System;
using System.Collections.Generic;

namespace laba1_compilator
{
    /// <summary>
    /// Рекурсивно-спусковой парсер с синхронизацией на 'end', чтобы при ошибках в конце всегда выводился 'end'.
    /// </summary>
    public class Recurs
    {
        private readonly List<Form1.Token> _tokens;
        private int _pos;
        private readonly List<string> _trace;
        private int _indent;

        public Recurs(List<Form1.Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
            _trace = new List<string>();
            _indent = 0;
            SkipSeparators();
        }

        public List<string> Parse()
        {
            ParseBeginStmt();
            return _trace;
        }

        private Form1.Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private void SkipSeparators()
        {
            while (Current != null && Current.Code == Form1.TokenCode.Separator)
                _pos++;
        }

        private void Add(string text)
        {
            _trace.Add(new string(' ', _indent * 2) + text);
        }

        private bool MatchLexeme(string lex)
        {
            if (Current != null && Current.Lexeme == lex)
            {
                Add(lex);
                _pos++;
                SkipSeparators();
                return true;
            }
            return false;
        }

        private bool MatchIdentifier()
        {
            if (Current != null && Current.Code == Form1.TokenCode.Identifier)
            {
                Add("VAR");
                _pos++;
                SkipSeparators();
                return true;
            }
            return false;
        }

        private bool MatchNumber()
        {
            if (Current != null && Current.Code == Form1.TokenCode.Integer)
            {
                Add("NUM");
                _pos++;
                SkipSeparators();
                return true;
            }
            return false;
        }

        private void Error(string expected)
        {
            Add($"error(expected {expected})");
            _pos++;
            SkipSeparators();
        }

        private void ParseBeginStmt()
        {
            Add("begin-stmt");
            _indent++;

            if (!MatchLexeme("begin"))
                Error("begin");

            ParseStmtList();

            // Если нет 'end', синхронизируемся: пропускаем до ближайшего 'end'
            if (!MatchLexeme("end"))
            {
                while (Current != null && Current.Lexeme != "end")
                {
                    _pos++;
                    SkipSeparators();
                }
                // теперь должны либо найти, либо завершить
                if (MatchLexeme("end")) { /* добавили 'end' */ }
            }

            _indent--;
        }

        private void ParseStmtList()
        {
            Add("stmt-list");
            _indent++;

            ParseStmt();

            if (MatchLexeme(";"))
            {
                ParseStmtList();
            }
            else
            {
                Add("ε");
            }

            _indent--;
        }

        private void ParseStmt()
        {
            Add("stmt");
            _indent++;

            if (Current != null && Current.Lexeme == "begin")
                ParseBeginStmt();
            else
                ParseAssgStmt();

            _indent--;
        }

        private void ParseAssgStmt()
        {
            Add("assg-stmt");
            _indent++;

            if (!MatchIdentifier())
                Error("VAR");

            if (!MatchLexeme(":="))
                Error(":=");

            ParseArithExpr();

            _indent--;
        }

        private void ParseArithExpr()
        {
            Add("arith-expr");
            _indent++;

            ParseFactor();
            ParseArithRest();

            _indent--;
        }

        private void ParseArithRest()
        {
            if (Current != null && (Current.Lexeme == "+" || Current.Lexeme == "*"))
            {
                Add(Current.Lexeme);
                _pos++;
                SkipSeparators();

                _indent++;
                ParseFactor();
                ParseArithRest();
                _indent--;
            }
            else
            {
                Add("ε");
            }
        }

        private void ParseFactor()
        {
            if (MatchIdentifier()) return;
            if (MatchNumber()) return;

            if (MatchLexeme("("))
            {
                _indent++;
                ParseArithExpr();
                if (!MatchLexeme(")"))
                    Error(")");
                _indent--;
            }
            else
            {
                Error("VAR|NUM|()");
            }
        }
    }
}
