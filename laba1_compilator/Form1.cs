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

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            Parser parser = new Parser();
            string[] lines = richTextBox1.Lines;
            string parseResult = parser.ParseLines(lines);
            richTextBox2.Text = parseResult;
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