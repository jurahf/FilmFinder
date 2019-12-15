using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaseUI
{
    /// <summary>
    /// Описывает отображение поля для редактирования
    /// </summary>
    public class FieldForEditUI
    {
        // TODO: Проверять и другие символы


        private string field;

        /// <summary>
        /// Само поле (без пути)
        /// </summary>
        public string Field
        {
            get
            {
                return field;
            }

            set
            {
                if (value.Contains('.'))
                {
                    throw new Exception("Нельзя использовать путь в поле, предназначенном для редактирования объекта");
                }
                else
                {
                    field = value;
                }
            }
        }

        /// <summary>
        /// Поле (можно с путем), значение которого будет выводиться вместо значения Field (только если поле - объект БД)
        /// </summary>
        public string FieldForUser { get; set; }

        /// <summary>
        /// Название поля для отображения
        /// </summary>
        public string NameForUser { get; set; }

        /// <summary>
        /// Объект, из которого берется поле
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        /// Перечисление полей и их названий (только если поле - список объектов)
        /// </summary>
        public List<FieldForListUI> ListFields { get; set; }

        /// <summary>
        /// Мастеровой объект (только если поле - список объектов)
        /// </summary>
        public MasterDefinition Master { get; set; }

        /// <summary>
        /// Кнопки и другие элементы тул-бара для добавления в элемент редактирования списка (только если поле - список объектов)
        /// </summary>
        public List<ToolStripItem> ToolBarItems { get; set; }

        /// <summary>
        /// Минимальное значение, исупользуемое в случае отображения в NumericUpDown
        /// </summary>
        public decimal MinUpDown { get; set; } = 0;

        /// <summary>
        /// Максимальное значение, исупользуемое в случае отображения в NumericUpDown
        /// </summary>
        public decimal MaxUpDown { get; set; } = 100;

        public decimal UpDownDefaultValue;

        /// <summary>
        /// Формат даты и времени, используемый в случае отображения в DateTimePicker
        /// </summary>
        public DateTimePickerFormat DateTimeFormat { get; set; } = DateTimePickerFormat.Short;

        /// <summary>
        /// Тип формы для выбора значения поля из списка (только если поле - объект БД) или формы редактирования (если поле - список объектов)
        /// </summary>
        public Type FormType
        {
            get; set;
        }

        /// <summary>
        /// Создание контрола для редактирования поля объекта
        /// </summary>
        /// <param name="Object">Объект, из которого берется поле</param>
        /// <param name="Field">Само поле (без пути)</param>
        /// <param name="FieldForUser">Поле (можно с путем), значение которого будет выводиться вместо значения Field (только если поле - объект БД)</param>
        /// <param name="NameForUser">Название поля для отображения</param>
        /// <param name="ListFormType">Тип формы для выбора значения поля из списка (только если поле - объект БД)</param>
        public FieldForEditUI(object Object, string Field, string FieldForUser, string NameForUser, Type ListFormType = null)
        {
            this.Object = Object;
            this.Field = Field;
            this.FieldForUser = FieldForUser;
            this.NameForUser = NameForUser;
            this.FormType = ListFormType;
        }

        /// <summary>
        /// Создание контрола для редактирования поля объекта
        /// </summary>
        /// <param name="Object">Объект, из которого берется поле</param>
        /// <param name="Field">Само поле (без пути)</param>
        /// <param name="NameForUser">Название поля для отображения</param>
        /// <param name="ListFormType">Тип формы для выбора значения поля из списка (только если поле - объект БД)</param>
        public FieldForEditUI(object Object, string Field, string NameForUser, Type ListFormType = null)
            : this(Object, Field, null, NameForUser, ListFormType)
        {
        }


        public override string ToString()
        {
            return this.Field;
        }

    }
}
