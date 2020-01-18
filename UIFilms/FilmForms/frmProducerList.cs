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
    public partial class frmProducerList : BaseListForm<Producer>
    {
        protected override string FormCaption => "Режиссёры";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Producer), typeof(frmProducerEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>() { new FieldForListUI(ReflectionHelper.Nameof<Producer>(a => a.Name), "Имя") };


        public frmProducerList()
            : this(false)
        {
        }

        public frmProducerList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Producer x, Producer y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
