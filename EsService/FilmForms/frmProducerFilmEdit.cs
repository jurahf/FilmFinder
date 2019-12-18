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
    public partial class frmProducerFilmEdit : BaseEditForm<ProducerFilm>
    {
        protected override string FormCaption => "Режиссёр фильма";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<ProducerFilm>(s => s.Producer), ReflectionHelper.Nameof<ProducerFilm>(s => s.Producer.Name), "Имя", new Dictionary<Type, Type>() { { typeof(Producer), typeof(frmProducerList) } }),
            };
        }


        public frmProducerFilmEdit()
            : base(new DBWork())
        {
        }

        public frmProducerFilmEdit(ProducerFilm objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
