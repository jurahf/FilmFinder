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
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        private string MessageText
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        private string ResultText
        {
            get { return textBox1.Text; }
        }

        public static string GetValue(string text, string caption)
        {
            InputBox frm = new InputBox();
            frm.Text = caption;
            frm.MessageText = text;

            if (frm.ShowDialog() == DialogResult.OK)
                return frm.ResultText;
            else
                return null;
        }

    }
}
