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
    public partial class frmGenreList : BaseListForm<Genre>
    {
        protected override string FormCaption => "Жанры";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Genre), typeof(frmGenreEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>() { new FieldForListUI(ReflectionHelper.Nameof<Genre>(a => a.Name), "Название") };


        public frmGenreList()
            : this(false)
        {
        }

        public frmGenreList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Genre x, Genre y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
