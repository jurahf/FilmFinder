using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseUI
{
    /// <summary>
    /// Описывает отображение поля для списка
    /// </summary>
    public class FieldForListUI
    {
        /// <summary>
        /// Поле (можно с путем) из которого берутся данные
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Заголовок колонки в списке
        /// </summary>
        public string FieldHeader { get; set; }

        /// <summary>
        /// Формат отображения для поля с датой
        /// </summary>
        public string DateTimeFormat { get; set; } = "dd.MM.yyyy";

        /// <summary>
        /// Создает новый экземпляр описания поля списка
        /// </summary>
        /// <param name="field">Поле (можно с путем) из которого берутся данные</param>
        /// <param name="fieldHeader">Заголовок колонки в списке</param>
        public FieldForListUI(string field, string fieldHeader)
        {
            this.Field = field;
            this.FieldHeader = fieldHeader;
        }

        public override string ToString()
        {
            return this.Field;
        }


    }
}
