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
    public partial class frmProducerEdit : BaseEditForm<Producer>
    {
        protected override string FormCaption => "Режиссёр";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Producer>(r => r.Name), "Имя")
            };
        }


        public frmProducerEdit()
            : base(new DBWork())
        {
        }

        public frmProducerEdit(Producer objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
