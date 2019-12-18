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

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(p => p.ProducerFilm), "Режиссёры", new Dictionary<Type, Type>() { { typeof(ProducerFilm), typeof(frmProducerFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<ProducerFilm>(s => s.Film)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<ProducerFilm>(s => s.Producer.Name), "Имя")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(p => p.GenreFilm), "Жанры", new Dictionary<Type, Type>() { { typeof(GenreFilm), typeof(frmGenreFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<GenreFilm>(s => s.Film)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<GenreFilm>(s => s.Genre.Name), "Название")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(p => p.CountryFilm), "Страны", new Dictionary<Type, Type>() { { typeof(CountryFilm), typeof(frmCountryFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<CountryFilm>(s => s.Film)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<CountryFilm>(s => s.Country.Name), "Название")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Film>(p => p.FilmCustomProperty), "Дополнительные свойства", new Dictionary<Type, Type>() { { typeof(FilmCustomProperty), typeof(frmFilmCustomPropertyEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<FilmCustomProperty>(s => s.Film)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<FilmCustomProperty>(s => s.CustomProperty.Name), "Название"),
                        new FieldForListUI( ReflectionHelper.Nameof<FilmCustomProperty>(s => s.CustomProperty.Value), "Значение")
                    }
                },

            };
        }


        public frmFilmEdit()
            : this(null, false)
        {
        }

        public frmFilmEdit(Film objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
            Tab = new TabControl();
            Tab.Height = 300;
            Tab.Width = 300;
            pnlBody.Controls.Add(Tab);
            this.Load += FrmFilmEdit_Load;
        }

        private void FrmFilmEdit_Load(object sender, EventArgs e)
        {
            Tab.Location = new Point(1, pnlBody.Height - Tab.Height);
            Tab.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            this.Width = 900;
            this.Height += 200;
        }
    }
}
