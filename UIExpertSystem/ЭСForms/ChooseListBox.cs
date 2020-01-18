using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ЭС
{
    public partial class ChooseListBox : Form
    {
        public ChooseListBox()
        {
            InitializeComponent();
        }

        private string Selected
        {
            get { return (string)listBox1.SelectedItem; }
        }

        private void SetItems(List<string> items)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(items.ToArray());
        }

        private string MessageText
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }


        public static string ShowChoose(string text, string caption, List<string> items)
        {
            ChooseListBox frm = new ChooseListBox();
            frm.MessageText = text;
            frm.Text = caption;
            frm.SetItems(items);

            if (frm.ShowDialog() == DialogResult.OK)
                return frm.Selected;
            else
                return null;
        }

    }
}
