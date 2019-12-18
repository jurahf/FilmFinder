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
    public partial class frmFilmEdit : BaseEditForm<Film>
    {
        protected override string FormCaption => "Фильм";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                //new FieldForListUI(ReflectionHelper.Nameof<Film>(f => f.Poster), "Постер"),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Name), "Название"),
                
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Year), "Год") { MinUpDown = 1895, MaxUpDown = DateTime.Now.Year, UpDownDefaultValue = DateTime.Now.Year },
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Slogan), "Слоган"),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Description), "Описание"),
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Rating), "Рейтинг") { MinUpDown = 0, MaxUpDown = 10, UpDownDefaultValue = 5 },
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(f => f.Link), "Ссылка"),

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(p => p.ActorFilm), "Актёры", new Dictionary<Type, Type>() { { typeof(ActorFilm), typeof(frmActorFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<ActorFilm>(s => s.Film)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<ActorFilm>(s => s.Actor.Name), "Имя")
                    }
                },

            };
        }


        public frmFilmEdit()
            : base(new DBWork())
        {
        }

        public frmFilmEdit(Film objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
        }
    }
}
