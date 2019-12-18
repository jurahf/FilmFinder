using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaseUI
{
    public partial class OpenListFormButton<T> : Button, IReturnValueControl where T : class, new()
    {
        private Dictionary<Type, Type> listFormType;
        private object value = null;

        public event EventHandler Selected;

        /// <summary>
        /// Выбранный объект
        /// </summary>
        [Browsable(false)]
        public object Value
        {
            get
            {
                return value;
            }
        }

        public OpenListFormButton(Dictionary<Type, Type> ListFormType)
        {
            /*
            if (ListFormType == null || !ListFormType.IsSubclassOf(typeof(BaseListForm<T>)))
            {
                throw new Exception("Кнопке открытия формы не предоставлен тип формы.");
            }                
            */

            this.listFormType = ListFormType;
            this.Click += button1_Click;
            this.Text = "...";

            InitializeComponent();
        }


        private void OnSelected()
        {
            if (Selected != null)
            {
                Selected(this, new EventArgs());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BaseListForm<T> form = (BaseListForm<T>)Activator.CreateInstance(listFormType.First().Value, new object[] { true });
            form.FormClosing += Form_FormClosing;
            form.ShowDialog();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // вернем значение из формы
            BaseListForm<T> form = (BaseListForm<T>)sender;

            if (form.DialogResult == DialogResult.OK)
            {
                value = form.SelectedValue;
                OnSelected();
            }
        }


    }
}
