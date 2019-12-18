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
    public partial class frmFilmList : BaseListForm<Film>
    {
        protected override string FormCaption => "Фильмы";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Film), typeof(frmFilmEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>()
            {
                //new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Poster), "Постер"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Name), "Название"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Year), "Год"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Slogan), "Слоган"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Description), "Описание"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Rating), "Рейтинг"),
                new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Link), "Ссылка"),
            };


        public frmFilmList()
            : this(false)
        {
        }

        public frmFilmList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Film x, Film y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
