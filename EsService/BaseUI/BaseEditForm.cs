using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaseUI
{
    /// <summary>
    /// Чтобы на формах списков при открытии формы редактирования
    /// можно было редактировать не только базовый тип, но и типы-наследники
    /// (и приводить тип формы к типу формы редактирования базового типа)
    /// </summary>
    public interface IBaseEditForm<out T>
    {
        event EventHandler Saved;
        T EditedObject { get; }
        DialogResult ShowDialog();
        DialogResult DialogResult { get; set; }
    }

    public abstract partial class BaseEditForm<T> : Form, IBaseEditForm<T> where T : class, new()
    {
        protected IDataWork DataWork { get; set; }
        private const int formHeightAdd = 38;
        protected List<FieldPresentation> fieldControls = new List<FieldPresentation>();
        protected bool isNewObject = false;
        protected T objectToEdit = null;
        public event EventHandler Saving;
        public event EventHandler Saved;

        protected abstract List<FieldForEditUI> FieldsNames();
        /*{
            return new List<FieldForUI>()
            {
                new FieldForUI(objectToEdit, "example", "Example", "example.example")
            };
        }*/

        /// <summary>
        /// Заголовок формы для отображения
        /// </summary>
        protected abstract string FormCaption { get; }

        /// <summary>
        /// Контрол с закладками - для отображения списков
        /// </summary>
        protected virtual TabControl Tab { get; set; }

        /// <summary>
        /// Валидация объекта перед сохранением (если валидация не пройдена - вызывать исключения)
        /// </summary>
        protected virtual void ValidateObject()
        {
        }

        public T EditedObject
        {
            get { return objectToEdit; }
        }

        public BaseEditForm(IDataWork dataWork)
            : this(dataWork, null, false)
        {
        }

        public BaseEditForm(IDataWork dataWork, T objectToEdit, bool isNewObject)
        {
            this.DataWork = dataWork;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.objectToEdit = objectToEdit;
            this.isNewObject = isNewObject;

            if (this.objectToEdit == null)
            {
                this.objectToEdit = new T();
                this.isNewObject = true;
            }            

            InitializeComponent();
        }

        private void OnSaving()
        {
            if (Saving != null)
            {
                Saving(this, new EventArgs());
            }
        }

        private void OnSaved()
        {
            if (Saved != null)
            {
                Saved(this, new EventArgs());
            }
        }

        private void BaseEditForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                this.Text = FormCaption;

                // по каждому полю объекта создадим контрол, в соответствии с типом
                int i = 0;
                foreach (var field in FieldsNames())
                {
                    FieldPresentation fp = new FieldPresentation(field);

                    // если это поле - список - размещаем на таб-контроле
                    Type type = ReflectionHelper.GetTypeOfObject(field.Object, field.Field);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>) && Tab != null)
                    {
                        Tab.TabPages.Add(field.Field, field.NameForUser);
                        Tab.TabPages[field.Field].Controls.Add(fp);
                        fp.Dock = DockStyle.Fill;
                    }
                    else
                    {
                        // если обычное поле - удлинняем форму
                        pnlBody.Controls.Add(fp);
                        fp.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        fp.Top = i;
                        i += fp.Height + 1;
                    }
                    
                    fieldControls.Add(fp);
                }

                // вычислить размер формы
                this.Height = i + pnlFooter.Height + formHeightAdd + (Tab?.Height ?? 0);
                this.MinimumSize = this.Size;

                this.Width = 500;
            }
        }

        /// <summary>
        /// Одна из кнопок, вызывающих закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                // при сохранении - запишем данные из контролов в объект
                foreach (var fp in fieldControls)
                {
                    if (fp.IsInit)
                    {
                        try
                        {
                            objectToEdit.GetType().GetProperty(fp.FieldForUi.Field).SetValue(objectToEdit, fp.GetValue(), null);
                        }
                        catch
                        {
                            MessageBox.Show($"Ошибка при сохранении: не удалось распознать значение поля {objectToEdit.GetType().GetProperty(fp.FieldForUi.Field).Name}.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }
            else
            {
                // при отмене - запишем первоначальные данные из контролов в объект
                foreach (var fp in fieldControls)
                {
                    if (fp.IsInit)
                    {
                        objectToEdit.GetType().GetProperty(fp.FieldForUi.Field).SetValue(objectToEdit, fp.OriginalValue(), null);
                    }
                }
            }

            // валидация
            try
            {
                ValidateObject();
                OnSaving();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"При сохранении произошла ошибка:{Environment.NewLine}{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }

            // отправление данных в базу
            try
            {
                if (isNewObject)
                {
                    if (DialogResult == DialogResult.OK)
                    {
                        DataWork.Insert(objectToEdit);
                    }
                }
                else
                {
                    DataWork.Update(objectToEdit);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"При сохранении произошла ошибка:{Environment.NewLine}{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }

            if (!e.Cancel)
            {
                OnSaved();
            }
        }

        public void RefreshData()
        {
            foreach (var fp in fieldControls)
            {
                fp.RefreshData();
            }
        }

    }
}
