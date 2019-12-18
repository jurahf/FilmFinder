using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using ExpertSystemDb;

namespace BaseUI
{
    public abstract partial class BaseListForm<T> : Form where T : class, new()
    {
        private const int paddingWidth = 20;
        private const int maxAutoWidth = 1000;
        private bool forChoosing = false;
        protected ListPresentation<T> ctrlList;

        /// <summary>
        /// Имя поля и имя колонки для его отображения
        /// </summary>
        /// <returns></returns>
        protected abstract List<FieldForListUI> ColumnsNames { get; }
        /*{
            return new Dictionary<string, string>() { { "example", "example" } };
        }*/

        /// <summary>
        /// Тип формы для редактирования объекта
        /// </summary>
        protected abstract Dictionary<Type, Type> EditFormTypes { get; }
        /*{
            get { return typeof(BaseEditForm<>); }
        }*/

        /// <summary>
        /// Заголовок формы для отображения
        /// </summary>
        protected abstract string FormCaption { get; }

        [Browsable(true), Category("View"), DefaultValue(true), Description("Показывать ли столбец с номерами строк")]
        public bool Numeration
        {
            get;
            set;
        } = true;


        [Browsable(true), Category("View"), DefaultValue(true), Description("Показывать ли столбец с типом содержимого")]
        public bool ShowType
        {
            get;
            set;
        } = false;


        [Browsable(false)]
        public Comparison<T> InitSort
        {
            get;
            set;
        }


        public T SelectedValue
        {
            get
            {
                return ctrlList.SelectedValue;
            }
        }

        public BaseListForm(IDataWork dataWork, bool forChoosing = false, InheritMode mode = InheritMode.BaseOnly, Comparison<T> InitSort = null)
        {
            this.MinimumSize = new Size(400, 600);
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            DialogResult = DialogResult.Cancel;
            this.forChoosing = forChoosing;
            this.InitSort = InitSort;

            ctrlList = new ListPresentation<T>(dataWork, ColumnsNames, EditFormTypes, InitSort, mode);

            InitializeComponent();
        }

        private void BaseListForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                this.Text = FormCaption;

                // покажем кнопку "выбрать", если надо
                btnChoose.Visible = forChoosing;


                // инициализация контрола-списка
                ctrlList.Numeration = Numeration;
                ctrlList.ShowType = ShowType;
                this.Controls.Add(ctrlList);
                ctrlList.CellDoubleClick += dataGridView1_DoubleClick;
                this.Width = Math.Min(maxAutoWidth, ctrlList.NeededWidth + panel1.Width + paddingWidth);
                ctrlList.Width = Math.Min(maxAutoWidth - panel1.Width - paddingWidth, ctrlList.NeededWidth);
                ctrlList.Height = panel1.Height;
                ctrlList.Location = new Point(0, 0);
                ctrlList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
        }


        /// <summary>
        /// Кнопка "Закрыть"
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// Кнопка "Выбрать"
        /// </summary>
        private void BtnChoose_Click(object sender, System.EventArgs e)
        {
            this.dataGridView1_DoubleClick(sender, new DataGridViewCellEventArgs(-1, -1));
        }


        /// <summary>
        /// Двойной щелчок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_DoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (forChoosing && ctrlList.SelectedValue != null)
            {
                // или выбрано значение
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // или его редактируют
                ctrlList.TryEditObject(e);
            }
        }



    }
}
