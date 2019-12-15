using DatabaseOperations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class DerivedListForm : BaseListForm<СтудентУчебнойГруппы>
    {
        protected override Dictionary<string, string> ColumnsNames()
        {
            return new Dictionary<string, string>
            {
                { "PrimaryKey", "Ключ" },
                { "УчебнаяГруппа1.Наименование", "Группа" }
            };
        }


        public DerivedListForm()
        {
            Numeration = false;

            InitializeComponent();
        }
    }
}
