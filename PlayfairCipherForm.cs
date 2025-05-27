using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Playfair
{
    public partial class PlayfairCipherForm : Form
    {
        public PlayfairCipherForm()
        {
            InitializeComponent();
        }
        private char[,] matrixToDraw;
        private char[,] GenerateMatrix(string keyword)
        {
            string result = "";
            HashSet<char> used = new HashSet<char>();
            keyword = keyword.ToUpper().Replace("J", "I");

            foreach (char c in keyword)
            {
                if (char.IsLetter(c) && !used.Contains(c))
                {
                    result += c;
                    used.Add(c);
                }
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c == 'J') continue;
                if (!used.Contains(c))
                {
                    result += c;
                    used.Add(c);
                }
            }
            char[,] matrix = new char[5, 5];
            for (int i = 0; i < 25; i++)
            {
                matrix[i / 5, i % 5] = result[i];
            }
            return matrix;
        }
        private List<(char, char)> PrepareTextEncrypt(string plaintext)
        {
            plaintext = plaintext.ToUpper().Replace("J", "I").Replace(" ", "");
            List<(char, char)> pairs = new List<(char, char)>();
            int i = 0;
            while (i < plaintext.Length)
            {
                char a = plaintext[i];
                char b = (i + 1) < plaintext.Length ? plaintext[i + 1] : 'X';

                if (a == b)
                {
                    pairs.Add((a, 'X'));
                    i++;
                }
                else
                {
                    pairs.Add((a, b));
                    i += 2;
                }
            }
            return pairs;
        }
        private List<(char, char)> PrepareTextDecrypt(string ciphertext)
        {
            ciphertext = ciphertext.ToUpper().Replace("J", "I").Replace(" ", "");
            List<(char, char)> pairs = new List<(char, char)>();
            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                char a = ciphertext[i];
                char b = (i + 1) < ciphertext.Length ? ciphertext[i + 1] : 'X';
                pairs.Add((a, b));
            }
            return pairs;
        }
        private (int, int) FindPosition(char[,] matrix, char ch)
        {
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (matrix[row, col] == ch)
                        return (row, col);
                }
            }
            return (-1, -1);
        }
        private string EncryptPlayfair(string keyword, string plaintext)
        {
            char[,] matrix = GenerateMatrix(keyword);
            matrixToDraw = matrix;
            dgvMatrix.Invalidate();
            DisplayMatrix(matrixToDraw); 
            var pairs = PrepareTextEncrypt(plaintext);
            string ciphertext = "";

            foreach (var pair in pairs)
            {
                var (row1, col1) = FindPosition(matrix, pair.Item1);
                var (row2, col2) = FindPosition(matrix, pair.Item2);

                if (row1 == row2)
                {
                    ciphertext += matrix[row1, (col1 + 1) % 5];
                    ciphertext += matrix[row2, (col2 + 1) % 5];
                }
                else if (col1 == col2)
                {
                    ciphertext += matrix[(row1 + 1) % 5, col1];
                    ciphertext += matrix[(row2 + 1) % 5, col2];
                }
                else
                {
                    ciphertext += matrix[row1, col2];
                    ciphertext += matrix[row2, col1];
                }
            }
            return ciphertext;
        }
        private string DecryptPlayfair(string keyword, string ciphertext)
        {
            char[,] matrix = GenerateMatrix(keyword);
            matrixToDraw = matrix;
            dgvMatrix.Invalidate();
            DisplayMatrix(matrixToDraw); 

            var pairs = PrepareTextDecrypt(ciphertext);
            string plaintext = "";

            foreach (var pair in pairs)
            {
                var (row1, col1) = FindPosition(matrix, pair.Item1);
                var (row2, col2) = FindPosition(matrix, pair.Item2);

                if (row1 == row2)
                {
                    plaintext += matrix[row1, (col1 + 4) % 5];
                    plaintext += matrix[row2, (col2 + 4) % 5];
                }
                else if (col1 == col2)
                {
                    plaintext += matrix[(row1 + 4) % 5, col1];
                    plaintext += matrix[(row2 + 4) % 5, col2];
                }
                else
                {
                    plaintext += matrix[row1, col2];
                    plaintext += matrix[row2, col1];
                }
            }
            return plaintext;
        }
        private void DisplayMatrix(char[,] matrix)
        {
            dgvMatrix.Rows.Clear();
            dgvMatrix.Columns.Clear();

            dgvMatrix.AllowUserToResizeColumns = false;
            dgvMatrix.AllowUserToResizeRows = false;
            dgvMatrix.ScrollBars = ScrollBars.None;
            dgvMatrix.BackgroundColor = Color.White;
            dgvMatrix.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            for (int i = 0; i < 5; i++)
            {
                dgvMatrix.Columns.Add($"Col{i}", "");
                dgvMatrix.Columns[i].Width = 30;
            }
            for (int i = 0; i < 5; i++)
            {
                dgvMatrix.Rows.Add();
                dgvMatrix.Rows[i].Height = 30;
                for (int j = 0; j < 5; j++)
                {
                    dgvMatrix.Rows[i].Cells[j].Value = matrix[i, j];
                }
            }
            dgvMatrix.Width = dgvMatrix.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + dgvMatrix.RowHeadersWidth + 2;
            dgvMatrix.Height = dgvMatrix.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + dgvMatrix.ColumnHeadersHeight + 2;
        }
        private void cmbAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKeyword.Text))
            {
                MessageBox.Show("Please enter a keyword.");
                return;
            }
            string keyword = txtKeyword.Text;
            string inputText = txtPlainText.Text;

            if (cmbAction.SelectedItem.ToString() == "Encrypt")
            {
                txtCipherText.Text = EncryptPlayfair(keyword, inputText);
            }
            else if (cmbAction.SelectedItem.ToString() == "Decrypt")
            {
                txtCipherText.Text = DecryptPlayfair(keyword, inputText);
            }
        }
        private void btnClear_Click_1(object sender, EventArgs e)
        {
            txtKeyword.Clear();
            txtPlainText.Clear();
            txtCipherText.Clear();
            matrixToDraw = null;
            dgvMatrix.Rows.Clear();
            dgvMatrix.Columns.Clear();
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
