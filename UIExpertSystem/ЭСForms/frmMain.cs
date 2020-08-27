using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;
using ClassicClasses;
using EsStorageAdapter;
using Logic;

namespace ЭС
{
    public partial class frmMain : Form
    {
        private IExpertSystemStorageAdapter storageAdapter = new ExpertSystemStorageAdapter();
        private ExpertSystem tempES = null;
        private string tempFile = "";



        /// <summary>
        /// Сохранена ли текущая система
        /// </summary>
        bool saved = true;

        public frmMain()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }


        /// <summary>
        /// Создание новой ЭС
        /// </summary>
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string esName = InputBox.GetValue("Введите имя для новой экспертной системы", "Создание экспертной системы");

                if (string.IsNullOrEmpty(esName))
                    return;

                tempES = storageAdapter.CreateNewES(esName);
                tempFile = esName;

                StartES();
                saved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FinishES();
            }
        }


        /// <summary>
        /// Скрывает на форме возможность доступа к закрытой ЭС
        /// </summary>
        private void FinishES()
        {
            this.Text = "Экспертная система";
            редактированиеToolStripMenuItem.Enabled = false;
            консультацияToolStripMenuItem.Enabled = false;
            сохранитьToolStripMenuItem.Enabled = false;
            сохранитьКакToolStripMenuItem.Enabled = false;
            закрытьToolStripMenuItem.Enabled = false;
            доменыToolStripMenuItem.Enabled = false;
            переменныеToolStripMenuItem.Enabled = false;
            правилаToolStripMenuItem.Enabled = false;
            пускToolStripMenuItem1.Enabled = false;
            цельToolStripMenuItem1.Enabled = false;
            объяснениеToolStripMenuItem.Enabled = false;
        }


        /// <summary>
        /// Готовит форму к работе с загруженной ЭС
        /// </summary>
        private void StartES()
        {
            this.Text = "Экспертная система - " + tempFile;
            редактированиеToolStripMenuItem.Enabled = true;
            консультацияToolStripMenuItem.Enabled = true;
            сохранитьToolStripMenuItem.Enabled = true;
            сохранитьКакToolStripMenuItem.Enabled = true;
            закрытьToolStripMenuItem.Enabled = true;
            доменыToolStripMenuItem.Enabled = true;
            переменныеToolStripMenuItem.Enabled = true;
            правилаToolStripMenuItem.Enabled = true;
            пускToolStripMenuItem1.Enabled = true;
            цельToolStripMenuItem1.Enabled = true;
            объяснениеToolStripMenuItem.Enabled = true;
        }


        /// <summary>
        /// Сохраняет экспертную систему в указанный файл
        /// </summary>
        /// <param name="name">Имя для сохранения</param>
        private void SaveES(string name)
        {
            try
            {
                storageAdapter.SaveES(tempES, name);
                MessageBox.Show("Экспертная система успешно сохранена!", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Кнопка "Выход"
        /// </summary>
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Кнопка "Сохранить"
        /// </summary>
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveES(tempFile);
        }


        /// <summary>
        /// Кнопка "Открыть из БД"
        /// </summary>
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> avaliableNames = storageAdapter.GetAvaliableESNames();
                string esName = ChooseListBox.ShowChoose("Выберите экспертную систему для открытия", "Выбор экспертной системы", avaliableNames);

                if (string.IsNullOrEmpty(esName))
                    return;

                tempFile = esName;
                tempES = storageAdapter.LoadES(esName);

                StartES();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FinishES();
            }
        }


        /// <summary>
        /// Кнопка "Редактировать домены"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void доменыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmDomains f = new frmDomains(tempES);
            if (f.ShowDialog() == DialogResult.OK)
            {
                saved = false;
            }
        }


        /// <summary>
        /// Кнопка "Редактировать переменные"
        /// </summary>
        private void цельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmVariables f = new frmVariables(tempES);
            if (f.ShowDialog() == DialogResult.OK)
            {
                saved = false;
            }
        }


        /// <summary>
        /// Кнопка "Редактировать правила"
        /// </summary>
        private void пускToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRules f = new frmRules(tempES);
            if (f.ShowDialog() == DialogResult.OK)
            {
                saved = false;
            }
        }

        private void цельToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmGoal fg = new frmGoal(tempES, this);
            fg.ShowDialog();
        }


        private void пускToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmConsultation frm = new frmConsultation();
            frm.Show();
        }

        private void объяснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tempES.WorkedRules == null || tempES.WorkedRules.Count <= 0)
            {
                MessageBox.Show("Нет данных");
                return;
            }
            else
            {
                frmExplain fe = new frmExplain(tempES);
                fe.ShowDialog();
            }
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tempES = null;
            FinishES();
            saved = true;   
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved)
            {
                DialogResult dr = MessageBox.Show("Сохранить перед выходом?", "Выход", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (dr)
                {
                    case DialogResult.Yes:
                        SaveES(tempFile);
                        goto case DialogResult.No;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break; ;
                }
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tempFile = saveFileDialog1.FileName;
                SaveES(saveFileDialog1.FileName);
                StartES();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
        }

        private void синхронизироватьДоменToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var s = new СинхронизацияБдФильмовСБзЭс();
                s.СоветыВСпецДомен();

                MessageBox.Show("Успешно синхронизировали");
            }
            catch
            {
                MessageBox.Show("При синхронизации произошла ошибка");
            }
        }
    }
}
