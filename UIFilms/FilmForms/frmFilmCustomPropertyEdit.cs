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
    public partial class frmFilmCustomPropertyEdit : BaseEditForm<FilmCustomProperty>
    {
        protected override string FormCaption => "Дополнительные свойства фильма";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<FilmCustomProperty>(s => s.CustomProperty), ReflectionHelper.Nameof<FilmCustomProperty>(s => s.CustomProperty.Name), "Название", new Dictionary<Type, Type>() { { typeof(CustomProperty), typeof(frmCustomPropertyList) } }),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<FilmCustomProperty>(s => s.Percent), "Значение")
            };
        }


        public frmFilmCustomPropertyEdit()
            : base(new DBWork())
        {
        }

        public frmFilmCustomPropertyEdit(FilmCustomProperty objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
