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
    public partial class frmActorEdit : BaseEditForm<Actor>
    {
        protected override string FormCaption => "Актёр";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Actor>(a => a.Name), "Имя")
            };
        }


        public frmActorEdit()
            : base(new DBWork())
        {
        }

        public frmActorEdit(Actor objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }


    }
}
