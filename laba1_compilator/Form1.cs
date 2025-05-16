using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using static laba1_compilator.Form1;


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
            Integer = 1,
            Identifier = 2,
            StringLiteral = 3,
            AssignOp = 10,
            Separator = 11,
            PlusOp = 12,
            MulOp = 13,
            LParen = 14,
            RParen = 15,
            Keyword = 16,
            EndOperator = 17,
            Error = 99
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
            // 1) Сканируем входной текст
            string input = richTextBox1.Text;
            var scanner = new Scanner();
            List<Token> tokens = scanner.Scan(input);

            // 2) Парсим токены рекурсивным спуском
            var parser = new Recurs(tokens);
            List<string> trace = parser.Parse();

            // 3) Выводим результат разбора по строкам
            richTextBox2.Clear();
            foreach (var step in trace)
            {
                richTextBox2.AppendText(step);
                richTextBox2.AppendText(Environment.NewLine);
            }
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            isTextChanged = true;
        }

        // 🔹 Функция проверки несохранённых изменений перед важными действиями
        private bool CheckForUnsavedChanges()
        {
            if (!isTextChanged) return true;

            var result = MessageBox.Show(
                "Сохранить изменения перед продолжением?",
                "Несохранённые изменения",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                return SaveFile();
            if (result == DialogResult.No)
                return true;

            return false; // Cancel
        }

        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
                return SaveFileAs();

            try
            {
                File.WriteAllText(currentFilePath, richTextBox1.Text);
                isTextChanged = false;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool SaveFileAs()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Сохранить файл"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = dlg.FileName;
                    return SaveFile();
                }
            }
            return false;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            using (var dlg = new OpenFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Открыть текстовый файл"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        currentFilePath = dlg.FileName;
                        richTextBox1.Text = File.ReadAllText(currentFilePath);
                        isTextChanged = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            using (var dlg = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Создать новый текстовый файл",
                FileName = "Новый файл.txt"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Create(dlg.FileName).Close();
                        currentFilePath = dlg.FileName;
                        richTextBox1.Clear();
                        isTextChanged = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckForUnsavedChanges())
                e.Cancel = true;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("Текстовое поле пусто. Нечего сохранять.", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFile();
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                MessageBox.Show("Текстовое поле пусто. Нечего сохранять.", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFileAs();
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Undo();
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Redo();
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Cut();
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Copy();
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Paste();
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectedText = string.Empty;
        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectAll();

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutProgramm().Show();
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Help().Show();
        }

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void грамматикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void диагностикаИНейтрализацияОшибокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Text().Show();
        }
    }
}
