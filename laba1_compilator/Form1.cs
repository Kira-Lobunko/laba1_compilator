using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;


namespace laba1_compilator
{
    public partial class Form1 : Form
    {
        private bool isTextChanged = false;
        private string currentFilePath = string.Empty;

        public Form1()
        {
            InitializeComponent();

            // Настраиваем колонки
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Тип", "Тип");
            dataGridView1.Columns.Add("Строка", "Строка");
            dataGridView1.Columns.Add("Позиция", "Позиция");
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
        }

        public enum TokenCode
        {
            Integer = 1,          // целое число
            Identifier = 3,       // идентификатор
            StringLiteral = 7,    // строковый литерал
            AssignOp = 6,        // знак "="
            Separator = 4,       // разделитель (пробел)
            Keyword = 2,         // ключевые слова: const, val
            EndOperator = 8,     // конец оператора ";"
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

        private void HighlightAndFill(string pattern, string type)
        {
            // 1) очищаем грид
            dataGridView1.Rows.Clear();

            // 2) сбрасываем предыдущие подсветки
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = Color.White;
            richTextBox1.DeselectAll();

            // 3) ищем совпадения
            var matches = Regex.Matches(richTextBox1.Text, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                // 4) добавляем строку в таблицу
                dataGridView1.Rows.Add(type, m.Value, m.Index);

                // 5) подсвечиваем в тексте
                richTextBox1.Select(m.Index, m.Length);
                richTextBox1.SelectionBackColor = Color.Yellow;
            }
            richTextBox1.DeselectAll();
        }


        // 1. HTML-теги <p>, <li>, <h3>
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            // ищем открывающие теги p, li, h3 (с любыми атрибутами)
            string htmlPattern = @"<\s*(p|li|h3)(\s+[^>]*?)?>";
            HighlightAndFill(htmlPattern, "HTML-тег");
        }

        // 2. ISBN-13
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            // ISBN-13: начинается с 978 или 979, дальше группы цифр, разделённых либо дефисом, либо пробелом, либо без
            string isbnPattern = @"\b97[89][-\s]?(?:\d[-\s]?){10}\b";
            HighlightAndFill(isbnPattern, "ISBN-13");
        }

        // 3. Российские автомобильные номера
        private void pictureBox13_Click(object sender, EventArgs e)
        {
            // Формат: 1 буква, 3 цифры, 2 буквы, 2–3 цифры (код региона). 
            // Буквы: А, В, Е, К, М, Н, О, Р, С, Т, У, Х
            string carPattern = @"\b[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}\b";
            HighlightAndFill(carPattern, "Росс. номер");
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
