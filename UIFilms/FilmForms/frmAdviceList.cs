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
    public partial class frmAdviceList : BaseListForm<Advice>
    {
        protected override string FormCaption => "Советы";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Advice), typeof(frmAdviceEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>()
            {
                new FieldForListUI(ReflectionHelper.Nameof<Advice>(f => f.Key), "Ключ"),
                new FieldForListUI(ReflectionHelper.Nameof<Advice>(f => f.FilmsStr), "Фильмы"),
                new FieldForListUI(ReflectionHelper.Nameof<Advice>(f => f.TagStr), "Теги"),
            };


        public frmAdviceList()
            : this(false)
        {
        }

        public frmAdviceList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Advice x, Advice y)
        {
            return x.Key.CompareTo(y.Key);
        }
    }
}
