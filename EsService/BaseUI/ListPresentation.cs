using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using ExpertSystemDb;

namespace BaseUI
{
    public partial class ListPresentation<T> : UserControl, IToolBarControl, IRefreshedControl where T : class, new()
    {
        protected IDataWork DataWork { get; set; }
        private List<FieldForListUI> columnsNames;    // путь до поля в объекте и заголовок для поля
        /// <summary>
        /// Тип данных, и тип формы для его редактирования
        /// </summary>
        private Dictionary<Type, Type> editFormTypes = new Dictionary<Type, Type>();
        List<Type> usedTypes = new List<Type>();
        private InheritMode inheritMode;
        private MasterDefinition master = null; // если мы редактируем не весь список, а только список, связанный с объектом
        private List<ToolStripItem> customtoolStripItems = new List<ToolStripItem>();

        public event DataGridViewCellEventHandler CellDoubleClick;

        [Browsable(false)]
        public T SelectedValue
        {
            get
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    return null;
                }
                else
                {
                    return (T)dataGridView1.SelectedRows.Cast<DataGridViewRow>().First().Tag;
                }
            }
        }


        [Browsable(true), Category("View"), DefaultValue(true), Description("Минимальная ширина формы с учетом колонок")]
        public int MinAutoWidth
        {
            get;
            set;
        } = 270;


        [Browsable(true), Category("View"), DefaultValue(true), Description("Устанавливаемая ширина колонок")]
        public int ColumnWidth
        {
            get;
            set;
        } = 100;



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
        public int NeededWidth
        {
            get;
            protected set;
        }

        [Browsable(false)]
        public Comparison<T> InitSort
        {
            get;
            set;
        }

        public ListPresentation(IDataWork dataWork, List<FieldForListUI> ColumnsNames, Dictionary<Type, Type> EditFormType, Comparison<T> InitSort = null, InheritMode InheritMode = InheritMode.BaseOnly)
        {
            this.DataWork = dataWork;
            this.columnsNames = ColumnsNames;
            this.editFormTypes = EditFormType;
            this.inheritMode = InheritMode;
            this.InitSort = InitSort;

            switch (inheritMode)
            {
                case InheritMode.BaseOnly:
                    usedTypes.Add(typeof(T));
                    break;
                case InheritMode.DerivedOnly:
                    usedTypes.AddRange(Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsSubclassOf(typeof(T))));
                    break;
            }            

            InitializeComponent();
        }

        public ListPresentation(IDataWork dataWork, List<FieldForListUI> ColumnsNames, Dictionary<Type, Type> EditFormType, MasterDefinition master, Comparison<T> InitSort = null, InheritMode InheritMode = InheritMode.BaseOnly)
            : this(dataWork, ColumnsNames, EditFormType, InitSort, InheritMode)
        {
            this.master = master;
        }

        /// <summary>
        /// Инициатор события двойного щелчка
        /// </summary>
        private void OnCellDoubleClick(DataGridViewCellEventArgs e)
        {
            if (CellDoubleClick != null)
            {
                CellDoubleClick(this, e);
            }
        }

        /// <summary>
        /// Добавляет строку с данными в таблицу
        /// </summary>
        /// <param name="data">Объект, из которого берутся данные</param>
        /// <returns></returns>
        private DataGridViewRow AddRow(T data)
        {
            int strNumber = dataGridView1.Rows.Count + 1;

            DataGridViewRow row = new DataGridViewRow();
            row.Tag = data;

            if (Numeration)
            {
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                cell.Value = strNumber.ToString();
                row.Cells.Add(cell);
            }

            DataGridViewCell cellType = new DataGridViewTextBoxCell();            
            cellType.Value = ReflectionHelper.GetTypeName(data.GetType());
            row.Cells.Add(cellType);

            foreach (var field in columnsNames)
            {
                // из d надо взять поле с именем field.Field
                DataGridViewCell cell = new DataGridViewTextBoxCell();

                FillCellValue(cell, data, field);
                
                cell.Tag = field.Field;
                row.Cells.Add(cell);
            }

            dataGridView1.Rows.Add(row);

            return row;
        }

        /// <summary>
        /// Заполняет ячейку таблицы значением поля объекта
        /// </summary>
        /// <param name="cell">Ячейка</param>
        /// <param name="data">Объект</param>
        /// <param name="key">Поле объекта</param>
        private void FillCellValue(DataGridViewCell cell, T data, FieldForListUI field)
        {
            object value = ReflectionHelper.GetTypedValueOfObject(data, field.Field);

            if (value != null)
            {
                Type type = value.GetType();
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (type == typeof(bool))
                {
                    cell.Value = (bool)value ? "+" : "-";
                }
                else if (type == typeof(DateTime))
                {
                    cell.Value = ((DateTime)value).ToString(field.DateTimeFormat);
                }
                else
                {
                    cell.Value = value.ToString();
                }
            }
            else
            {
                cell.Value = "";
            }
        }

        /// <summary>
        /// Возвращает тип формы редактирования, заданный для типа
        /// </summary>
        /// <param name="editedType">Тип данных для редактирования</param>
        private Type GetFormType(Type editedType)
        {
            if (editFormTypes.ContainsKey(editedType))
            {
                return editFormTypes[editedType];
            }
            else
            {
                // TODO: Если на одной форме будут редактироваться и класс и его потомки, надо будет добавить проверку,
                // чтобы доставать форму более конкретного типа
                return editFormTypes.First(p => editedType.IsSubclassOf(p.Key)).Value;
            }
        }

        /// <summary>
        /// Кнопка "Добавить"
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            var tag = (sender as ToolStripItem).Tag;
            if (tag != null && tag is Type)
            {
                StartAddData(tag as Type);
            }
            else
            {
                StartAddData();
            }            
        }

        /// <summary>
        /// Кнопка "Изменить"
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows.Cast<DataGridViewRow>().First();

                EditObject(row);
            }
        }

        private void EditObject(DataGridViewRow row)
        {
            Type editedType = row.Tag.GetType();
            Type editFormType = GetFormType(editedType);

            IBaseEditForm<T> form = (IBaseEditForm<T>)Activator.CreateInstance(editFormType, row.Tag, false);
            form.Saved += Form_Saved_Edit;
            form.ShowDialog();
        }

        public void TryEditObject(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                EditObject(dataGridView1.Rows[e.RowIndex]);
            }
        }

        /// <summary>
        /// Кнопка "Удалить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Действительно удалить выбранный объект?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DataGridViewRow row = dataGridView1.SelectedRows.Cast<DataGridViewRow>().First();
                    T obj = (T)row.Tag;

                    try
                    {
                        DataWork.DeleteObject(obj);
                        dataGridView1.Rows.Remove(row);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить объект: {Environment.NewLine}{ex.Message}", "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Закрытие формы редактирования при изменении объекта
        /// </summary>
        private void Form_Saved_Edit(object sender, EventArgs e)
        {
            IBaseEditForm<T> form = (IBaseEditForm<T>)sender;

            if (form.DialogResult == DialogResult.OK)
            {
                // изменим строку в таблице
                var row = dataGridView1.Rows.Cast<DataGridViewRow>().First(r => r.Tag == form.EditedObject);

                foreach (var field in columnsNames)
                {
                    // из d надо взять поле с именем pair.Key
                    DataGridViewCell cell = row.Cells.Cast<DataGridViewCell>().First(c => (string)c.Tag == field.Field);
                    FillCellValue(cell, form.EditedObject, field); 
                }

                row.Selected = true;
            }
        }

        /// <summary>
        /// Закрытие формы редактирования при добавлении объекта
        /// </summary>
        private void Form_Saved_Add(object sender, EventArgs e)
        {
            IBaseEditForm<T> form = (IBaseEditForm<T>)sender;

            if (form.DialogResult == DialogResult.OK)
            {
                // добавим строку в таблицу
                var row = AddRow(form.EditedObject);
                row.Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;
            }
        }

        /// <summary>
        /// Щелчок по ячейкам - пробрасываем событие дальше
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OnCellDoubleClick(e);
        }

        private void ListPresentation_Load(object sender, EventArgs e)
        {
            int width = dataGridView1.RowHeadersWidth + 5;

            if (inheritMode == InheritMode.BaseOnly)
            {
                toolStripButton1.Click += btnAdd_Click;
            }
            else
            {
                // надо создать кнопки для добавления строк каждого типа
                toolStripButton1.Visible = false;
                toolStripSplitButton1.Visible = true;
                foreach (var type in usedTypes)
                {
                    var typeItem = new System.Windows.Forms.ToolStripMenuItem();
                    typeItem.Text = ReflectionHelper.GetTypeName(type);
                    typeItem.Tag = type;
                    typeItem.Click += btnAdd_Click;
                    
                    toolStripSplitButton1.DropDownItems.Add(typeItem);
                }
            }

            // создадим колонки
            if (Numeration)
            {
                int index = dataGridView1.Columns.Add("col_number", "№");
                dataGridView1.Columns[index].Width = 35;
                width += dataGridView1.Columns[index].Width;
            }

            int indexColType = dataGridView1.Columns.Add("col_type", "Тип");
            dataGridView1.Columns[indexColType].Width = 80;
            width += dataGridView1.Columns[indexColType].Width;
            dataGridView1.Columns[indexColType].Visible = ShowType;

            foreach (var field in columnsNames)
            {
                int index = dataGridView1.Columns.Add(field.Field, field.FieldHeader);
                dataGridView1.Columns[index].Width = ColumnWidth;
                dataGridView1.Columns[index].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                width += dataGridView1.Columns[index].Width;
            }

            this.NeededWidth = Math.Max(width, MinAutoWidth);

            RefreshData();
        }


        public void AddToToolBar(ToolStripItem item)
        {
            if (!customtoolStripItems.Any())
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                toolStrip1.Items.Add(separator);
                customtoolStripItems.Add(separator);
            }

            customtoolStripItems.Add(item);
            toolStrip1.Items.Add(item);
        }

        public void RefreshData()
        {
            dataGridView1.Rows.Clear();

            // вычитаем данные типа T из БД
            List<T> data = new List<T>();

            // если редактируем связанные объекты - возьмем только мастера
            if (master == null)
            {
                foreach (var t in usedTypes)
                {
                    data.AddRange(DataWork.GetFromDatabase(t).OfType<T>());
                }
            }
            else
            {
                foreach (var t in usedTypes)
                {
                    data.AddRange(DataWork.GetFromDatabase(t).OfType<T>());
                }

                data = data.Where(d => typeof(T).GetProperty(master.MasterFieldName).GetValue(d, null) == master.Master).ToList();
            }

            if (InitSort != null)
            {
                data.Sort(InitSort);
            }

            // и создадим строки
            foreach (var d in data)
            {
                AddRow(d);
            }
        }


        public void StartAddData()
        {
            BaseEditForm<T> form;
            Type editFormType = editFormTypes.Single().Value;

            if (master == null)
            {
                // добавление обычного объекта
                form = (BaseEditForm<T>)Activator.CreateInstance(editFormType);
            }
            else
            {
                // добавление объекта, связанного с мастеровым
                T editedObject = new T();
                typeof(T).GetProperty(master.MasterFieldName).SetValue(editedObject, master.Master, null);

                form = (BaseEditForm<T>)Activator.CreateInstance(editFormType, editedObject, true);
            }

            form.Saved += Form_Saved_Add;
            form.ShowDialog();
        }


        public void StartAddData(Type type)
        {
            IBaseEditForm<T> form;
            Type editFormType = GetFormType(type);

            if (master == null)
            {
                // добавление обычного объекта
                form = (IBaseEditForm<T>)Activator.CreateInstance(editFormType);
            }
            else
            {
                // TODO: Этот код не проверен, и, возможно, работать не будет

                // добавление объекта, связанного с мастеровым
                T editedObject = new T();
                typeof(T).GetProperty(master.MasterFieldName).SetValue(editedObject, master.Master, null);

                form = (IBaseEditForm<T>)Activator.CreateInstance(editFormType, editedObject, true);
            }

            form.Saved += Form_Saved_Add;
            form.ShowDialog();
        }

    }
}
