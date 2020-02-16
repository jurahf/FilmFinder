using ExpertSystemDb;
using Logic;
using Logic.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilmForms
{
    public partial class frmFillFilmFromUrl : Form
    {
        public frmFillFilmFromUrl()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                ЗагрузкаФильмаИзКинопоиска кинопоиск = new ЗагрузкаФильмаИзКинопоиска();

                try
                {
                    var фильм = кинопоиск.ЗагрузитьПоСсылке(textBox1.Text);

                    if (фильм != null)
                    {
                        frmFilmEdit frm = new frmFilmEdit(фильм, true);
                        frm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Фильм не загрузился");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"При загрузке произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in openFileDialog1.FileNames)
                {
                    try
                    {
                        var фильм = LoadFilm(fileName);
                        if (фильм != null)
                        {
                            listView1.Items.Add(фильм.Name);
                            listView1.Items[listView1.Items.Count - 1].BackColor = Color.LightGreen;
                            listView1.Items[listView1.Items.Count - 1].Tag = фильм;
                        }
                        else
                        {
                            listView1.Items.Add($"НЕ ЗАГРУЗИЛСЯ: {fileName}");
                            listView1.Items[listView1.Items.Count - 1].BackColor = Color.OrangeRed;
                        }
                    }
                    catch (FilmAlreadyExistsException)
                    {
                        listView1.Items.Add($"УЖЕ СУЩЕСТВУЕТ: {fileName}");
                        listView1.Items[listView1.Items.Count - 1].BackColor = Color.OrangeRed;
                    }
                    catch (Exception ex)
                    {
                        listView1.Items.Add($"ФАТАЛЬНАЯ ОШИБКА: {fileName} - {ex.Message}");
                        listView1.Items[listView1.Items.Count - 1].BackColor = Color.OrangeRed;
                    }
                }
            }
        }

        private Film LoadFilm(string fileName)
        {
            string html;
            using (StreamReader sr = new StreamReader(fileName))
            {
                html = sr.ReadToEnd();
            }

            ЗагрузкаФильмаИзКинопоиска кинопоиск = new ЗагрузкаФильмаИзКинопоиска();
            var фильм = кинопоиск.ЗагрузитьИзСтроки(html);

            return фильм;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    Film film = listView1.SelectedItems[0].Tag as Film;

                    if (film != null)
                    {
                        frmFilmEdit frm = new frmFilmEdit(film, true);
                        frm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
