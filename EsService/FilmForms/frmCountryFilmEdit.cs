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
    public partial class frmCountryFilmEdit : BaseEditForm<CountryFilm>
    {
        protected override string FormCaption => "Страна фильма";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<CountryFilm>(s => s.Country), ReflectionHelper.Nameof<CountryFilm>(s => s.Country.Name), "Название", new Dictionary<Type, Type>() { { typeof(Country), typeof(frmCountryList) } }),
            };
        }


        public frmCountryFilmEdit()
            : base(new DBWork())
        {
        }

        public frmCountryFilmEdit(CountryFilm objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
