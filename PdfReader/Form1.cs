﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Windows.Forms;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;

namespace PdfReader
{
    public partial class Form1 : Form
    {
        List<String> pages = new List<String>();
        StringBuilder pageText=new StringBuilder();
        int page = 1;
        string currentFile;
        int pageCount=0;
        public Form1()
        {
            InitializeComponent();
            UpdateButtonSizeAndPosition();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
               
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    currentFile= filePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Что-то пошло не так", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            pageCount= GetPagesCount(currentFile);
            richTextBox1.Text = GetTextFromPage(currentFile, page);
            label2.Text+=pageCount.ToString();
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
            if (page < 2)
            {
                return;
            }

            richTextBox1.Text = GetTextFromPage(currentFile, --page);
            textBox1.Text = page.ToString();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (page + 1 > pageCount - 1)
            {
                return;
            }

            richTextBox1.Text = GetTextFromPage(currentFile, ++page);
            textBox1.Text=page.ToString();
        }


        private void richTextBox1_FontChanged(object sender, EventArgs e)
        {
            pageCount = GetPagesCount(currentFile);
        }


        private int CalculateCharsPerLine(RichTextBox richTextBox)
        {
            using (Graphics g = richTextBox.CreateGraphics())
            {
                float charSize = richTextBox.Font.Size;
                int charsPerLine = (int)(richTextBox.Width / (charSize/1.33));
                return charsPerLine;
            }
        }

        private long GetFileLenght(string filePath)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(filePath);
            long size = file.Length;
            return size;

        }
        private string GetTextFromPage(string filePath, int targetPageNumber)
        {
            StreamReader sr = new StreamReader(filePath);

            int charSizeInBytes, pageSizeInBytes;
            GetBytesPerPage(richTextBox1, out charSizeInBytes, out pageSizeInBytes);

            int offset = (targetPageNumber - 1) * pageSizeInBytes;

            sr.BaseStream.Seek(offset, SeekOrigin.Begin);


            char[] buffer = new char[pageSizeInBytes / charSizeInBytes];

            int bytesRead = sr.ReadBlock(buffer, 0, buffer.Length);

            StringBuilder pageText = new StringBuilder();
            
            pageText.Append(buffer);
            pageText.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
            return pageText.ToString();
        }

        private void GetBytesPerPage(RichTextBox richTextBox, out int charSizeInBytes, out int pageSizeInBytes)
        {
            int charsPerLine = CalculateCharsPerLine(richTextBox);
            int linesPerPage = CalculateLinesPerPage(richTextBox);

            charSizeInBytes = Encoding.UTF8.GetByteCount("a");
            pageSizeInBytes = charsPerLine * linesPerPage * charSizeInBytes;
        }
        private int GetPagesCount(string filePath)
        {
            int charSizeInBytes, pageSizeInBytes;
            GetBytesPerPage(richTextBox1, out charSizeInBytes, out pageSizeInBytes);
            return (int)(GetFileLenght(filePath) / pageSizeInBytes);

        }

        private int CalculateLinesPerPage(RichTextBox richTextBox)
        {
            using (Graphics g = richTextBox.CreateGraphics())
            {
                SizeF charSize = g.MeasureString("a", richTextBox.Font);
                int linesPerPage = (int)(richTextBox.Height / charSize.Height);
                return linesPerPage;
            }
        }


       

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            UpdateButtonSizeAndPosition();
            if (richTextBox1.Text!="")
            {
                pageCount = GetPagesCount(currentFile);
                richTextBox1.Text = GetTextFromPage(currentFile, page);

                label2.Text ='/'+ pageCount.ToString();
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
            if(textBox1.Text.Length==0) {
                return;

            }
            
            try
            {
                Int32.TryParse(textBox1.Text,out page);
                if(page>pageCount)
                {
                    throw new Exception("Нет такой страницы");
                }
                richTextBox1.Text = GetTextFromPage(currentFile, page);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка" , MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}