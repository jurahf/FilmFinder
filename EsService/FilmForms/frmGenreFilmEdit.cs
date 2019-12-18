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
    public partial class frmGenreFilmEdit : BaseEditForm<GenreFilm>
    {
        protected override string FormCaption => "Жанры фильма";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<GenreFilm>(s => s.Genre), ReflectionHelper.Nameof<GenreFilm>(s => s.Genre.Name), "Название", new Dictionary<Type, Type>() { { typeof(Genre), typeof(frmGenreList) } }),
            };
        }


        public frmGenreFilmEdit()
            : base(new DBWork())
        {
        }

        public frmGenreFilmEdit(GenreFilm objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
