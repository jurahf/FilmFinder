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
    public partial class frmCustomPropertyEdit : BaseEditForm<CustomProperty>
    {
        protected override string FormCaption => "Пользовательское свойство";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<CustomProperty>(p => p.Name), "Название"),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<CustomProperty>(p => p.Value), "Значение")
            };
        }


        public frmCustomPropertyEdit()
            : base(new DBWork())
        {
        }

        public frmCustomPropertyEdit(CustomProperty objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
