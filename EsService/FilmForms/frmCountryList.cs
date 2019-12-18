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
    public partial class frmCountryList : BaseListForm<Country>
    {
        protected override string FormCaption => "Страны";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Country), typeof(frmCountryEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>() { new FieldForListUI(ReflectionHelper.Nameof<Country>(c => c.Name), "Название") };


        public frmCountryList()
            : this(false)
        {
        }

        public frmCountryList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Country x, Country y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
