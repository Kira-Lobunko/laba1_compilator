//using System;
//using System.Text;
//using System.Collections.Generic;

//namespace laba1_compilator
//{
//    public enum TokenType
//    {
//        CONST,
//        VAL,
//        IDENTIFIER,
//        STRING_LITERAL,
//        EQUAL,
//        SEMICOLON,
//        UNKNOWN
//    }

//    public struct Token
//    {
//        public TokenType Type;
//        public string Value;
//        public int Position;
//        public bool IsStringClosed;

//        public Token(TokenType type, string value, int position)
//        {
//            Type = type;
//            Value = value;
//            Position = position;
//            IsStringClosed = true;
//        }
//    }

//    public struct ErrorInfo
//    {
//        public int Position;
//        public string Message;
//        public string Suggestion;

//        public ErrorInfo(int position, string message, string suggestion)
//        {
//            Position = position;
//            Message = message;
//            Suggestion = suggestion;
//        }
//    }

//    public class Parser
//    {
//        public string ParseLines(string[] lines)
//        {
//            var output = new StringBuilder();
//            for (int li = 0; li < lines.Length; li++)
//            {
//                var line = lines[li];
//                if (string.IsNullOrWhiteSpace(line))
//                    continue;

//                output.AppendLine($"Строка {li + 1}:");
//                var errors = new List<ErrorInfo>();
//                var tree = ParseLine(line, errors);

//                foreach (var ln in tree)
//                    output.AppendLine(ln);

//                output.AppendLine($"Ошибок: {errors.Count}");
//                for (int i = 0; i < errors.Count; i++)
//                {
//                    var e = errors[i];
//                    int posHuman = e.Position + 1;
//                    output.AppendLine($"{i + 1}. Позиция {posHuman}: {e.Message}. Рекомендуется: {e.Suggestion}");
//                }
//                output.AppendLine();
//            }
//            return output.ToString();
//        }

//        private List<string> ParseLine(string line, List<ErrorInfo> errors)
//        {
//            var output = new List<string>();
//            var tokens = Tokenize(line, errors);

//            // Если токенов нет и ровно одна ошибка — возвращаем только её
//            if (tokens.Count == 0 && errors.Count == 1)
//                return output;

//            int idx = 0;
//            bool suppressFurtherErrors = false;

//            // === S0: CONST ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.CONST)
//            {
//                output.Add("S0: const -> S1");
//                idx++;
//            }
//            else
//            {
//                int pos = idx < tokens.Count ? tokens[idx].Position : line.Length;
//                string found = idx < tokens.Count ? tokens[idx].Value : "";
//                output.Add($"S0: **Ошибка**: ожидалось ключевое слово 'const', встречено '{found}'");
//                errors.Add(new ErrorInfo(pos,
//                    "отсутствует ключевое слово 'const'",
//                    "Добавьте ключевое слово 'const'."));
//                // синхронизация — прыгаем к первому VAL
//                while (idx < tokens.Count && tokens[idx].Type != TokenType.VAL)
//                    idx++;
//            }

//            // === S1: VAL ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.VAL)
//            {
//                output.Add("  S1: val -> S2");
//                idx++;
//            }
//            else
//            {
//                int pos = idx < tokens.Count ? tokens[idx].Position : line.Length;
//                // извлекаем фрагмент «встречено»
//                int start = pos;
//                int end = line.IndexOfAny(new[] { ' ', '\t', '=', ';' }, start);
//                if (end == -1) end = line.Length;
//                string badVal = line.Substring(start, end - start);

//                output.Add($"  S1: **Ошибка**: ожидалось ключевое слово 'val', встречено '{badVal}'");
//                errors.Clear();
//                errors.Add(new ErrorInfo(pos,
//                    "отсутствует ключевое слово 'val'",
//                    "Добавьте ключевое слово 'val'."));
//                suppressFurtherErrors = true;
//                // пропускаем токены, входящие в этот фрагмент
//                while (idx < tokens.Count && tokens[idx].Position < end)
//                    idx++;
//                // эмулируем корректный переход
//                output.Add("  S1: val -> S2");
//            }

//            // === S2: IDENTIFIER ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.IDENTIFIER)
//            {
//                var tok = tokens[idx];
//                // если внутри идентификатора есть кириллица — это ошибка
//                if (ContainsCyrillic(tok.Value))
//                {
//                    output.Add($"    S2: **Ошибка**: ожидался идентификатор, встречено '{tok.Value}'");
//                    if (!suppressFurtherErrors)
//                        errors.Add(new ErrorInfo(tok.Position,
//                            "ожидался идентификатор",
//                            "Добавьте корректный идентификатор."));
//                    idx++;
//                }
//                else
//                {
//                    output.Add($"    S2: {tok.Value} -> S3");
//                    idx++;
//                }
//            }
//            else
//            {
//                int pos = idx < tokens.Count ? tokens[idx].Position : line.Length;
//                string found = idx < tokens.Count ? tokens[idx].Value : "";
//                output.Add($"    S2: **Ошибка**: ожидался идентификатор, встречено '{found}'");
//                if (!suppressFurtherErrors)
//                    errors.Add(new ErrorInfo(pos,
//                        "отсутствует идентификатор",
//                        "Добавьте имя идентификатора."));
//                // синхронизация — к '='
//                while (idx < tokens.Count && tokens[idx].Type != TokenType.EQUAL)
//                    idx++;
//            }

//            // === S3: '=' ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.EQUAL)
//            {
//                output.Add($"{Indent(3)}S3: = -> S4");
//                idx++;
//            }
//            else
//            {
//                int pos = idx < tokens.Count ? tokens[idx].Position : line.Length;
//                string found = idx < tokens.Count ? tokens[idx].Value : "";
//                output.Add($"{Indent(3)}S3: **Ошибка**: ожидалось символ '=', встречено '{found}'");
//                if (!suppressFurtherErrors)
//                    errors.Add(new ErrorInfo(pos,
//                        "отсутствует символ '='",
//                        "Добавьте символ '='."));
//                while (idx < tokens.Count && tokens[idx].Type != TokenType.STRING_LITERAL)
//                    idx++;
//            }

//            // === S4: STRING_LITERAL ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.STRING_LITERAL)
//            {
//                output.Add($"{Indent(4)}S4: {tokens[idx].Value} -> S5");
//                idx++;
//            }
//            else
//            {
//                int pos = idx < tokens.Count ? tokens[idx].Position : line.Length;
//                string found = idx < tokens.Count ? tokens[idx].Value : "";
//                output.Add($"{Indent(4)}S4: **Ошибка**: ожидался строковый литерал, встречено '{found}'");
//                if (!suppressFurtherErrors)
//                    errors.Add(new ErrorInfo(pos,
//                        "отсутствует строковый литерал",
//                        "Добавьте строковый литерал в кавычках."));
//                while (idx < tokens.Count && tokens[idx].Type != TokenType.SEMICOLON)
//                    idx++;
//            }

//            // === S5: ';' ===
//            if (idx < tokens.Count && tokens[idx].Type == TokenType.SEMICOLON)
//            {
//                output.Add($"{Indent(5)}S5: ; -> S6");
//                idx++;
//            }
//            else
//            {
//                string found = idx < tokens.Count ? tokens[idx].Value : "";
//                output.Add($"{Indent(5)}S5: **Ошибка**: ожидалось символ ';', встречено '{found}'");
//                if (!suppressFurtherErrors)
//                    errors.Add(new ErrorInfo(line.Length,
//                        "отсутствует символ ';'",
//                        "Добавьте символ ';'."));
//            }

//            // === S6: финальное состояние ===
//            output.Add($"{Indent(6)}S6 (конечное состояние, принято)");

//            return output;
//        }

//        private List<Token> Tokenize(string line, List<ErrorInfo> errors)
//        {
//            var tokens = new List<Token>();
//            int i = 0;

//            while (i < line.Length)
//            {
//                char c = line[i];

//                // 1) Любые цифры вне кавычек — лишний токен
//                if (char.IsDigit(c))
//                {
//                    int start = i;
//                    var sb = new StringBuilder();
//                    while (i < line.Length && char.IsDigit(line[i]))
//                        sb.Append(line[i++]);
//                    errors.Add(new ErrorInfo(start,
//                        $"лишний токен '{sb}'",
//                        "Удалите лишний токен."));
//                    continue;
//                }

//                // 2) Подряд идущая кириллица вне кавычек — одна ошибка
//                if (IsCyrillic(c))
//                {
//                    int start = i;
//                    var sb = new StringBuilder();
//                    while (i < line.Length && IsCyrillic(line[i]))
//                        sb.Append(line[i++]);
//                    errors.Add(new ErrorInfo(start,
//                        $"неподдерживаемый символ '{sb}'",
//                        "Удалите неподдерживаемый символ."));
//                    continue;
//                }

//                // 3) Пробельные символы
//                if (char.IsWhiteSpace(c))
//                {
//                    i++;
//                    continue;
//                }

//                // 4) const
//                if (i + 5 <= line.Length && line.Substring(i, 5) == "const"
//                    && (i + 5 == line.Length || !IsIdentifierChar(line[i + 5])))
//                {
//                    tokens.Add(new Token(TokenType.CONST, "const", i));
//                    i += 5;
//                    continue;
//                }

//                // 5) val
//                if (i + 3 <= line.Length && line.Substring(i, 3) == "val"
//                    && (i + 3 == line.Length || !IsIdentifierChar(line[i + 3])))
//                {
//                    tokens.Add(new Token(TokenType.VAL, "val", i));
//                    i += 3;
//                    continue;
//                }

//                // 6) идентификатор (латиница и '_')
//                if (char.IsLetter(c) && !IsCyrillic(c) || c == '_')
//                {
//                    int start = i;
//                    var sb = new StringBuilder();
//                    sb.Append(c);
//                    i++;
//                    while (i < line.Length && IsIdentifierChar(line[i]) && !IsCyrillic(line[i]))
//                        sb.Append(line[i++]);
//                    tokens.Add(new Token(TokenType.IDENTIFIER, sb.ToString(), start));
//                    continue;
//                }

//                // 7) '='
//                if (c == '=')
//                {
//                    tokens.Add(new Token(TokenType.EQUAL, "=", i));
//                    i++;
//                    continue;
//                }

//                // 8) ';'
//                if (c == ';')
//                {
//                    tokens.Add(new Token(TokenType.SEMICOLON, ";", i));
//                    i++;
//                    continue;
//                }

//                // 9) строковый литерал
//                if (c == '"')
//                {
//                    int start = i;
//                    var sb = new StringBuilder();
//                    sb.Append(c);
//                    i++;
//                    bool closed = false;
//                    while (i < line.Length)
//                    {
//                        sb.Append(line[i]);
//                        if (line[i] == '"')
//                        {
//                            closed = true;
//                            i++;
//                            break;
//                        }
//                        i++;
//                    }
//                    tokens.Add(new Token(TokenType.STRING_LITERAL, sb.ToString(), start)
//                    { IsStringClosed = closed });
//                    continue;
//                }

//                // 10) всё остальное — ошибка
//                errors.Add(new ErrorInfo(i,
//                    $"неподдерживаемый символ '{c}'",
//                    "Удалите неподдерживаемый символ."));
//                i++;
//            }

//            return tokens;
//        }

//        private static bool ContainsCyrillic(string s)
//        {
//            foreach (char c in s)
//                if (IsCyrillic(c))
//                    return true;
//            return false;
//        }

//        private static bool IsCyrillic(char c) =>
//            c >= '\u0400' && c <= '\u04FF';

//        private bool IsIdentifierChar(char ch) =>
//            char.IsLetter(ch) || ch == '_';

//        private string Indent(int level) => new string(' ', level * 2);
//    }
//}