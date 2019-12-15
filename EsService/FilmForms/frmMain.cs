using ExpertSystemDb;
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
    public partial class frmMain : Form
    {
        private Dictionary<string, Form> openedForms = new Dictionary<string, Form>();

        public frmMain()
        {
            InitializeComponent();
        }

        private void OpenForm(Type formType)
        {
            if (openedForms.ContainsKey(formType.Name))
            {
                Form f = openedForms[formType.Name];
                f.Activate();
            }
            else
            {
                Form form = (Form)Activator.CreateInstance(formType);
                openedForms.Add(formType.Name, form);
                form.FormClosing += Form_FormClosing;
                form.Show();
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            string key = sender.GetType().Name;
            openedForms.Remove(key);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                // TODO: DI
                IDataWork data = new DBWork();
                var фильмы = data.GetFromDatabase<Film>().ToList();

                foreach (var f in фильмы)
                {
                    dataGridView1.Rows.Add(f.Name, f.Year, f.Description);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не найдена база данных. Приложение будет закрыто.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Exception tempEx = ex;
                string error = "";
                do
                {
                    error += $"{tempEx.Message}{Environment.NewLine}";
                    tempEx = tempEx.InnerException;
                } while (tempEx != null);

                using (StreamWriter sw = new StreamWriter("log.txt", true))
                {
                    sw.Write(error);
                }

                this.Close();
            }
        }

        private void актёрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForm(typeof(frmActorList));
        }




    }
}
