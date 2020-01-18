using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseUI
{
    /// <summary>
    /// Представляет описание мастерового объекта для объекта данного класса
    /// </summary>
    public class MasterDefinition
    {
        /// <summary>
        /// Поле, которым объект ссылается на мастера
        /// </summary>
        public string MasterFieldName { get; set; }

        /// <summary>
        /// Мастеровой объект
        /// </summary>
        public object Master { get; set; }

        public MasterDefinition(object Master, string MasterFieldName)
        {
            this.Master = Master;
            this.MasterFieldName = MasterFieldName;
        }

    }
}
