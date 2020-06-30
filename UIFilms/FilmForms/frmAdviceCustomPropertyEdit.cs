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
    public partial class frmAdviceCustomPropertyEdit : BaseEditForm<AdviceCustomProperty>
    {
        protected override string FormCaption => "Пользовательское свойство в совете";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty), ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty.Name), "Название", new Dictionary<Type, Type>() { { typeof(CustomProperty), typeof(frmCustomPropertyList) } }),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.Value), "Степень соответствия")
                    { MinUpDown = -100, MaxUpDown = 100, UpDownDefaultValue = 100 },
            };
        }


        public frmAdviceCustomPropertyEdit()
            : base(new DBWork())
        {
        }

        public frmAdviceCustomPropertyEdit(AdviceCustomProperty objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
