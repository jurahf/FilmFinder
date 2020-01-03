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
    public partial class frmAdviceFilmEdit : BaseEditForm<AdviceFilm>
    {
        protected override string FormCaption => "Фильм в совете";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<AdviceFilm>(s => s.Film), ReflectionHelper.Nameof<AdviceFilm>(s => s.Film.Name), "Название", new Dictionary<Type, Type>() { { typeof(Film), typeof(frmFilmList) } }),
            };
        }


        public frmAdviceFilmEdit()
            : base(new DBWork())
        {
        }

        public frmAdviceFilmEdit(AdviceFilm objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
