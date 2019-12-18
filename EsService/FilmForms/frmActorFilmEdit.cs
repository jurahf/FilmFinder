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
    public partial class frmActorFilmEdit : BaseEditForm<ActorFilm>
    {
        protected override string FormCaption => "Актёр в фильме";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<ActorFilm>(s => s.Actor), ReflectionHelper.Nameof<ActorFilm>(s => s.Actor.Name), "Имя", new Dictionary<Type, Type>() { { typeof(Actor), typeof(frmActorList) } }),
            };
        }


        public frmActorFilmEdit()
            : base(new DBWork())
        {
        }

        public frmActorFilmEdit(ActorFilm objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
