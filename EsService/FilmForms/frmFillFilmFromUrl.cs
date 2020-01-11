using Logic;
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string html;
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                    {
                        html = sr.ReadToEnd();
                    }

                    ЗагрузкаФильмаИзКинопоиска кинопоиск = new ЗагрузкаФильмаИзКинопоиска();
                    var фильм = кинопоиск.ЗагрузитьИзСтроки(html);

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





    }
}
