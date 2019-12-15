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
    public partial class frmActorList : BaseListForm<Actor>
    {
        protected override string FormCaption => "Актёры";

        protected override Dictionary<Type, Type> EditFormTypes =>
            new Dictionary<Type, Type>() { { typeof(Actor), typeof(frmActorEdit) } };

        protected override List<FieldForListUI> ColumnsNames =>
            new List<FieldForListUI>() { new FieldForListUI(ReflectionHelper.Nameof<Actor>(a => a.Name), "Имя") };


        public frmActorList()
            : this(false)
        {
        }

        public frmActorList(bool forChoosing)
            : base(new DBWork(), forChoosing, InitSort: Sorting)
        {
        }

        protected static int Sorting(Actor x, Actor y)
        {
            return x.Name.CompareTo(y.Name);
        }



    }
}
