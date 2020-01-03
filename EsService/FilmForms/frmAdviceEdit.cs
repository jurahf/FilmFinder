﻿using BaseUI;
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
    public partial class frmAdviceEdit : BaseEditForm<Advice>
    {
        protected override string FormCaption => "Совет";

        protected override List<FieldForEditUI> FieldsNames()
        {
            return new List<FieldForEditUI>()
            {
                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Advice>(p => p.AdviceFilmPositive), "Включить фильмы", new Dictionary<Type, Type>() { { typeof(AdviceFilm), typeof(frmAdviceFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<AdviceFilm>(s => s.AdvicePositive)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<AdviceFilm>(s => s.Film.Name), "Название")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Advice>(p => p.AdviceFilmNegative), "Исключить фильмы", new Dictionary<Type, Type>() { { typeof(AdviceFilm), typeof(frmAdviceFilmEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<AdviceFilm>(s => s.AdviceNegative)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI( ReflectionHelper.Nameof<AdviceFilm>(s => s.Film.Name), "Название")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Advice>(p => p.AdviceCustomPropertyPositive), "Включить свойства", new Dictionary<Type, Type>() { { typeof(AdviceCustomProperty), typeof(frmAdviceCustomPropertyEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.AdvicePositive)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI(ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty.Name), "Название"),
                        new FieldForListUI(ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty.Value), "Значение")
                    }
                },

                new FieldForEditUI(EditedObject, ReflectionHelper.Nameof<Advice>(p => p.AdviceCustomPropertyNegative), "Исключить свойства", new Dictionary<Type, Type>() { { typeof(AdviceCustomProperty), typeof(frmAdviceCustomPropertyEdit) } })
                {
                    Master = new MasterDefinition(EditedObject, ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.AdviceNegative)),
                    ListFields = new List<FieldForListUI> ()
                    {
                        new FieldForListUI(ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty.Name), "Название"),
                        new FieldForListUI(ReflectionHelper.Nameof<AdviceCustomProperty>(s => s.CustomProperty.Value), "Значение")
                    }
                },

            };
        }


        public frmAdviceEdit()
            : this(null, false)
        {
        }

        public frmAdviceEdit(Advice objToEdit, bool isNewObject)
            : base(new DBWork(), objToEdit, isNewObject)
        {
            Tab = new TabControl();
            Tab.Height = 300;
            Tab.Width = 300;
            pnlBody.Controls.Add(Tab);
            this.Load += FrmAdviceEdit_Load;
        }

        private void FrmAdviceEdit_Load(object sender, EventArgs e)
        {
            Tab.Location = new Point(1, pnlBody.Height - Tab.Height);
            Tab.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            this.Width = 900;
            this.Height += 200;
        }
    }
}
