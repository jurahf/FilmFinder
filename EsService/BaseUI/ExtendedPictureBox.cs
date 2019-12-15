using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BaseUI
{
    public partial class ExtendedPictureBox : UserControl
    {
        public Image Picture
        {
            get
            {
                return pictureBox1.Image;
            }
            set
            {
                pictureBox1.Image = value;
            }
        }

        public string PictureBase64
        {
            get
            {
                if (Picture == null)
                    return null;

                using (MemoryStream ms = new MemoryStream())
                {
                    Picture.Save(ms, Picture.RawFormat);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public ExtendedPictureBox()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Picture = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (Picture != null
                && MessageBox.Show("Текущее изображение будет удалено. Продолжить?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Picture = null;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {

        }
    }
}
