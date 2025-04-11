using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace laba1_compilator
{
    public partial class Form1 : Form
    {
        private bool isTextChanged = false;
        private string currentFilePath = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        public enum TokenCode
        {
            Integer = 1,          // целое число
            Identifier = 2,       // идентификатор
            StringLiteral = 3,    // строковый литерал
            AssignOp = 10,        // знак "="
            Separator = 11,       // разделитель (пробел)
            Keyword = 14,         // ключевые слова: const, val
            EndOperator = 16,     // конец оператора ";"
            Error = 99            // ошибка
        }

        public class Token
        {
            public TokenCode Code { get; set; }
            public string Type { get; set; }
            public string Lexeme { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public int Line { get; set; }

            public override string ToString()
            {
                return $"[{Line}:{StartPos}-{EndPos}] ({Code}) {Type} : '{Lexeme}'";
            }
        }

        public class Scanner
        {
            private string _text;
            private int _pos;
            private int _line;
            private int _linePos;
            private List<Token> _tokens;
            private HashSet<string> _keywords = new HashSet<string> { "const", "val" };

            // Флаг, чтобы после ключевого слова ожидать идентификатор
            private bool _expectIdentifier = false;

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

                while (!IsEnd())
                {
                    char ch = CurrentChar();

                    switch (ch)
                    {
                        case '\r':
                        case '\n':
                            Advance(); // Просто пропускаем переход на новую строку
                            break;

                        case var c when char.IsWhiteSpace(c):
                            // Если пробел – добавляем токен разделителя и идём дальше
                            if (c == ' ')
                            {
                                AddToken(TokenCode.Separator, "разделитель", "(пробел)");
                            }
                            Advance();
                            break;

                        // Если символ – буква или символ подчёркивания,
                        // считываем непрерывную последовательность (которая может быть смешанной)
                        case var c when (char.IsLetter(c) || c == '_'):
                            ReadMixedIdentifierOrError();
                            break;

                        // Если цифра – обрабатываем как целое число (идентификаторы не могут начинаться с цифры)
                        case var c when char.IsDigit(c):
                            ReadInteger();
                            break;

                        case '=':
                            AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
                            Advance();
                            break;

                        case ';':
                            AddToken(TokenCode.EndOperator, "конец оператора", ";");
                            Advance();
                            break;

                        case '"':
                            ReadStringLiteral();
                            break;

                        default:
                            // Для остальных символов группируем подряд идущие недопустимые
                            int startErrorPos = _linePos;
                            StringBuilder errorSb = new StringBuilder();
                            while (!IsEnd() && !IsValidTokenStart(CurrentChar()))
                            {
                                errorSb.Append(CurrentChar());
                                Advance();
                            }
                            AddToken(TokenCode.Error, "недопустимый символ", errorSb.ToString(), startErrorPos, _linePos - 1, _line);
                            break;
                    }
                }

                return _tokens;
            }

            // Вспомогательный метод: является ли символ допустимым началом токена (для остальных случаев)
            private bool IsValidTokenStart(char ch)
            {
                if (ch == '\r' || ch == '\n')
                    return true;
                if (char.IsWhiteSpace(ch))
                    return true;
                // Допустимыми считаем английские буквы (в диапазоне A-Z, a-z), цифры, подчёркивание,
                // а также символы, начинающие строку, оператор присваивания и конец оператора.
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                    return true;
                if (char.IsDigit(ch))
                    return true;
                if (ch == '_' || ch == '=' || ch == ';' || ch == '"')
                    return true;
                return false;
            }

            // Вспомогательный метод, проверяющий, относится ли символ к «словообразующим»
            private bool IsWordChar(char ch)
            {
                return char.IsLetter(ch) || char.IsDigit(ch) || ch == '_';
            }

            // Вспомогательный метод, определяющий, является ли символ допустимым для идентификатора
            // (т.е. английская буква, цифра или '_')
            private bool IsAllowedIdentifierChar(char ch)
            {
                return (ch >= 'A' && ch <= 'Z') ||
                       (ch >= 'a' && ch <= 'z') ||
                       (ch >= '0' && ch <= '9') ||
                       (ch == '_');
            }

            // Новый метод для считывания последовательности, состоящей из букв, цифр и '_'
            // При этом последовательность делится на сегменты, где каждая группа либо состоит
            // из допустимых (английских) символов, либо из недопустимых (например, русских)
            private void ReadMixedIdentifierOrError()
            {
                // Запоминаем позицию начала всей последовательности
                int tokenStartPos = _linePos;
                StringBuilder wordSb = new StringBuilder();

                // Считываем всю последовательность из буквы, цифры или '_'
                while (!IsEnd() && IsWordChar(CurrentChar()))
                {
                    wordSb.Append(CurrentChar());
                    Advance();
                }

                string word = wordSb.ToString();
                int segmentStart = 0;
                // Определяем тип первого символа
                bool currentAllowed = IsAllowedIdentifierChar(word[0]);

                for (int i = 1; i < word.Length; i++)
                {
                    bool allowed = IsAllowedIdentifierChar(word[i]);
                    if (allowed != currentAllowed)
                    {
                        // Завершаем сегмент от segmentStart до i-1
                        string segment = word.Substring(segmentStart, i - segmentStart);
                        TokenCode code = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
                        string type = currentAllowed ? "идентификатор" : "недопустимый символ";
                        // Если сегмент полностью совпадает с ключевым словом, меняем тип
                        if (currentAllowed && _keywords.Contains(segment))
                        {
                            code = TokenCode.Keyword;
                            type = "ключевое слово";
                            _expectIdentifier = true; // после ключевого слова ждём идентификатор
                        }
                        else if (_expectIdentifier && currentAllowed)
                        {
                            // Если ожидался идентификатор, а получен корректный сегмент – сбрасываем флаг
                            _expectIdentifier = false;
                        }
                        AddToken(code, type, segment, tokenStartPos + segmentStart, tokenStartPos + i - 1, _line);
                        // Начинаем новый сегмент с текущего символа
                        segmentStart = i;
                        currentAllowed = allowed;
                    }
                }
                // Завершаем последний сегмент
                string lastSegment = word.Substring(segmentStart);
                TokenCode lastCode = currentAllowed ? TokenCode.Identifier : TokenCode.Error;
                string lastType = currentAllowed ? "идентификатор" : "недопустимый символ";
                if (currentAllowed && _keywords.Contains(lastSegment))
                {
                    lastCode = TokenCode.Keyword;
                    lastType = "ключевое слово";
                    _expectIdentifier = true;
                }
                else if (_expectIdentifier && currentAllowed)
                {
                    _expectIdentifier = false;
                }
                AddToken(lastCode, lastType, lastSegment, tokenStartPos + segmentStart, tokenStartPos + word.Length - 1, _line);
            }

            private void ReadInteger()
            {
                int startPos = _linePos;
                StringBuilder sb = new StringBuilder();
                sb.Append(CurrentChar());
                Advance();

                while (!IsEnd() && char.IsDigit(CurrentChar()))
                {
                    sb.Append(CurrentChar());
                    Advance();
                }

                string lexeme = sb.ToString();
                AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
            }

            private void ReadStringLiteral()
            {
                int startPos = _linePos;
                Advance(); // Пропускаем открывающую кавычку
                StringBuilder sb = new StringBuilder();
                bool closed = false;

                while (!IsEnd())
                {
                    char ch = CurrentChar();
                    if (ch == '"')
                    {
                        closed = true;
                        Advance();
                        break;
                    }
                    else
                    {
                        sb.Append(ch);
                        Advance();
                    }
                }

                if (closed)
                {
                    AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
                }
                else
                {
                    AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
                    while (!IsEnd())
                    {
                        AddToken(TokenCode.Error, "недопустимый символ", CurrentChar().ToString(), _linePos, _linePos, _line);
                        Advance();
                    }
                }
            }

            private bool IsEnd()
            {
                return _pos >= _text.Length;
            }

            private char CurrentChar()
            {
                return _pos < _text.Length ? _text[_pos] : '\0';
            }

            private void Advance()
            {
                if (CurrentChar() == '\n')
                {
                    _line++;
                    _linePos = 0;
                }
                _pos++;
                _linePos++;
            }

            private void AddToken(TokenCode code, string type, string lexeme)
            {
                AddToken(code, type, lexeme, _linePos, _linePos, _line);
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
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            string input = richTextBox1.Text;

            // Создаем сканер
            var scanner = new Scanner();

            // Сканируем текст
            List<Token> tokens = scanner.Scan(input);

            // Выводим результаты в richTextBox2
            richTextBox2.Clear();
            foreach (var token in tokens)
            {
                richTextBox2.AppendText(
                    $"Строка: {token.Line}, с позиции {token.StartPos} по {token.EndPos} — {token.Type}: \"{token.Lexeme}\" (код {(int)token.Code})\n"
                );
            }
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            isTextChanged = true;
        }

        // 🔹 Функция проверки несохранённых изменений перед важными действиями
        private bool CheckForUnsavedChanges()
        {
            if (!isTextChanged) return true; // Если изменений нет – выходим

            DialogResult result = MessageBox.Show(
                "Сохранить изменения перед продолжением?",
                "Несохранённые изменения",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                return SaveFile();
            }
            else if (result == DialogResult.No)
            {
                return true;
            }

            return false; // "Отмена" – прерываем действие
        }

        // 🔹 Функция сохранения файла
        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return SaveFileAs(); // Если нет пути, вызываем "Сохранить как"
            }

            try
            {
                File.WriteAllText(currentFilePath, richTextBox1.Text);
                isTextChanged = false;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // 🔹 Функция "Сохранить как"
        private bool SaveFileAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Сохранить файл"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = saveFileDialog.FileName;
                return SaveFile();
            }

            return false; // Пользователь отменил сохранение
        }

        // 🔹 Открытие файла с проверкой изменений
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Открыть текстовый файл"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    currentFilePath = openFileDialog.FileName;
                    richTextBox1.Text = File.ReadAllText(currentFilePath);
                    isTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🔹 Создание нового файла с проверкой изменений
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Создать новый текстовый файл",
                FileName = "Новый файл.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Create(saveFileDialog.FileName).Close();
                    currentFilePath = saveFileDialog.FileName;
                    richTextBox1.Clear();
                    isTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🔹 Сохранение изменений перед выходом из программы
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 🔹 Проверка перед закрытием формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckForUnsavedChanges())
            {
                e.Cancel = true; // Отменяем выход, если пользователь передумал
            }
        }

        // 🔹 Сохранение файла
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("Текстовое поле пусто. Нечего сохранять.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFile();
        }

        // 🔹 Сохранение файла как
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("Текстовое поле пусто. Нечего сохранять.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFileAs();
        }

        // 🔹 Функции редактирования текста
        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Undo();
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Redo();
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Cut();
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Copy();
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Paste();
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectedText = string.Empty;
        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectAll();

        // 🔹 Открытие справки и информации о программе
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var AboutProgramm = new AboutProgramm();
            AboutProgramm.Show();
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Help = new Help();
            Help.Show();
        }
    }
}