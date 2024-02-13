﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace PdfReader
{
    public partial class Form1 : Form
    {
        List<String> pages = new List<String>();
        string pageText = "";
        int page = 0;

        public Form1()
        {
            InitializeComponent();
            UpdateButtonSizeAndPosition();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH", MessageId = "type: System.String; size: 46881MB")]
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
                pageText = "";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
    
                    try
                    {
                        // Открываем файл для чтения
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            // Читаем и выводим содержимое файла построчно
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                pageText += line;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка: ",ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Что-то пошло не так", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size, FontStyle.Regular);
            FillPages(richTextBox1, pageText);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size - 1, FontStyle.Regular);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size + 1, FontStyle.Regular);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (page < 1)
            {
                return;
            }

            richTextBox1.Text = pages[--page];
            textBox1.Text = page.ToString();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (page + 1 > pages.Count - 1)
            {
                return;
            }

            richTextBox1.Text = pages[++page];
            textBox1.Text = page.ToString();
        }


        private void richTextBox1_FontChanged(object sender, EventArgs e)
        {
            FillPages(richTextBox1, pageText);
        }


        private int CalculateCharsPerLine(RichTextBox richTextBox)
        {
            using (Graphics g = richTextBox.CreateGraphics())
            {
                float charSize = richTextBox.Font.Size;
                int charsPerLine = (int)(richTextBox.Width / (charSize / 1.3333));
                return charsPerLine;
            }
        }

        private int CalculateLinesPerPage(RichTextBox richTextBox)
        {
            using (Graphics g = richTextBox.CreateGraphics())
            {
                SizeF charSize = g.MeasureString("a", richTextBox.Font);
                int charsPerLine = (int)(richTextBox.Width / charSize.Width);
                int linesPerPage = (int)(richTextBox.Height / charSize.Height);
                return linesPerPage;
            }
        }


        private void FillPages(RichTextBox richTextBox, string text)
        {
            pages.Clear();
            int linesPerPage = CalculateLinesPerPage(richTextBox);
            int charactersPerLine = CalculateCharsPerLine(richTextBox);

            List<string> lines = new List<string>();
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder currentLine = new StringBuilder();

            foreach (string word in words)
            {
                if (currentLine.Length + word.Length + 1 <= charactersPerLine) 
                {
                    currentLine.Append(word);
                    currentLine.Append(" ");
                }
                else
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                    currentLine.Append(word);
                    currentLine.Append(" ");
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            int currentPage = 1;
            int currentLines = 0;
            StringBuilder currentPageText = new StringBuilder();
            foreach (string line in lines)
            {
                if (currentLines < linesPerPage)
                {
                    currentPageText.AppendLine(line);
                    currentLines++;
                }
                else
                {
                    pages.Add($"{Environment.NewLine}{currentPageText.ToString()}{Environment.NewLine}");
                    currentPage++;
                    currentPageText.Clear();
                    currentPageText.AppendLine(line);
                    currentLines = 1;
                }
            }


            pages.Add($"{Environment.NewLine}{currentPageText.ToString()}{Environment.NewLine}");
            if (pages.Count <= page)
            {
                page -= 1;
            }

            richTextBox1.Text = pages[page];
            textBox1.Text = page.ToString();
            label2.Text = "/";
            label2.Text += currentPage - 1;
            UpdateButtonSizeAndPosition();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateButtonSizeAndPosition();
            if (pageText.Length > 0)
            {
                FillPages(richTextBox1, pageText);
            }
            else
            {
                return;
            }
        }

        private void UpdateButtonSizeAndPosition()
        {
            int buttonWidth = ClientSize.Width / 10; 
            int buttonHeight = 25; 
            int margin = 5; 
            
            button1.Size = new Size(buttonWidth, buttonHeight);
            button1.Location = new Point(0, 0);

            button2.Size = new Size(buttonWidth, buttonHeight);
            button2.Location = new Point(button1.Right + margin, 0);

            button3.Size = new Size(buttonWidth, buttonHeight);
            button3.Location = new Point(button2.Right + margin, 0);

            button4.Size = new Size(buttonWidth, buttonHeight);
            button4.Location = new Point(button3.Right + margin, 0);

            textBox1.Location = new Point(button4.Right + margin, 4);
            label2.Location = new Point(textBox1.Right + 2, 4);

            button5.Size = new Size(buttonWidth, buttonHeight);
            button5.Location = new Point(label2.Right + margin, 0);

            richTextBox1.Location = new Point(0, button1.Bottom + margin);
            richTextBox1.Size = new Size(ClientSize.Width, ClientSize.Height - richTextBox1.Top);
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (pages.Count == 0)
                {
                    throw new Exception("Необходимо открыть файл");
                }
                if (textBox1.Text == String.Empty)
                {
                    throw new Exception("Введите номер страницы");
                }

                Regex regex = new Regex("^(-?\\d+){1}$");
                MatchCollection matchCollection = regex.Matches(textBox1.Text);
                if (matchCollection.Count == 0)
                {
                    throw new Exception("Введен некорректный номер страницы");
                }

                int num = int.Parse(textBox1.Text);
                if (num > pages.Count)
                {
                    throw new Exception("Такой страницы не существует");
                }

                if (num < 0)
                {
                    throw new Exception("Номер страницы не может быть отрицательным");
                }
                richTextBox1.Text = pages[num];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка" , MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}