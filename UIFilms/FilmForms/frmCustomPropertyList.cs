using BaseUI;
using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilmForms
{
    public partial class frmCustomPropertyList : BaseListForm<CustomProperty>
    {
        protected override string FormCaption => "Пользовательские свойства";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(CustomProperty), typeof(frmCustomPropertyEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>()
            {
                new FieldForListUI(ReflectionHelper.Nameof<CustomProperty>(a => a.Name), "Название"),
            };


        public frmCustomPropertyList()
            : this(false)
        {
        }

        public frmCustomPropertyList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(CustomProperty x, CustomProperty y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
