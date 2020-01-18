using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;
using ExpertSystemDb;

namespace BaseUI
{
    /// <summary>
    /// Контрол для отображения поля объекта для редактирования
    /// </summary>
    public partial class FieldPresentation : UserControl, IRefreshedControl
    {
        private const int innerPadding = 5;
        private const int controlsTop = 3;
        private object originalValue;
        private FieldForEditUI field;
        private Control ctrlForEdit;
        private Control ctrlForWatch;

        [Browsable(true), Category("Размер"), Description("Высота контрола в случае, если он отображает таблицу")]
        public int TableHeight
        {
            get;
            set;
        } = 300;

        [Browsable(true), Category("Размер"), Description("Ширина контрола в случае, если он отображает таблицу")]
        public int TableWidth
        {
            get;
            set;
        } = 300;

        public FieldForEditUI FieldForUi
        {
            get { return field; }
        }
        
        /// <summary>
        /// Возвращает, было ли поле проинициализировано, а данные загружены для отображения и редактирования
        /// </summary>
        [Browsable(false)]
        public bool IsInit
        {
            get;
            set;
        } = false;

        public FieldPresentation(FieldForEditUI field)
        {
            this.field = field;

            InitializeComponent();
        }

        private NumericUpDown AddNumericUpDown(decimal val)
        {
            decimal toAssign = GetAssignedValue(val, field);

            NumericUpDown nud = new NumericUpDown()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2)),
                Minimum = field.MinUpDown,
                Maximum = field.MaxUpDown,
                Value = toAssign,
            };

            /*
            nud.Accelerations.Add(new NumericUpDownAcceleration(2, 2));
            nud.Accelerations.Add(new NumericUpDownAcceleration(5, 5));
            nud.Accelerations.Add(new NumericUpDownAcceleration(8, 10));
            */

            this.Controls.Add(nud);

            return nud;
        }

        private decimal GetAssignedValue(decimal val, FieldForEditUI field)
        {
            decimal toAssign = val < field.MinUpDown || val > field.MaxUpDown
                            ? field.UpDownDefaultValue
                            : val;
            if (toAssign < field.MinUpDown)
                toAssign = field.MinUpDown;
            if (toAssign > field.MaxUpDown)
                toAssign = field.MaxUpDown;

            return toAssign;
        }

        private DateTimePicker AddDateTimePicker(DateTime val)
        {
            DateTimePicker dtp = new DateTimePicker()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2)),
                Value = val,
                Format = field.DateTimeFormat
            };
            
            this.Controls.Add(dtp);

            return dtp;
        }


        private ComboBox AddComboBox(Type enumType, string val)
        {
            ComboBox cb = new ComboBox()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var item in Enum.GetNames(enumType))
            {
                int num = cb.Items.Add(item);
                if (val == item)
                {
                    cb.SelectedIndex = num;
                }
            }

            this.Controls.Add(cb);

            return cb;
        }

        private ExtendedPictureBox AddPictureBox(Image pic)
        {
            ExtendedPictureBox pb = new ExtendedPictureBox()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2)),
                Picture = pic
            };

            this.Height = pb.Height;
            this.Controls.Add(pb);

            return pb;
        }

        private CheckBox AddCheckBox(bool val)
        {
            CheckBox chb = new CheckBox()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2)),
                Checked = val
            };

            this.Controls.Add(chb);

            return chb;
    }


        private TextBox AddTextBox()
        {
            TextBox tb = new TextBox()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Left = label1.Left + label1.Width + innerPadding,
                Top = controlsTop,
                Width = this.Width - (label1.Left + label1.Width + (innerPadding * 2))
            };

            this.Controls.Add(tb);

            return tb;
        }


        private void FieldPresentation_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                // добавим лейбл с пользовательским именем
                label1.Text = field.NameForUser;

                // возьмем из объекта поле
                originalValue = ReflectionHelper.GetTypedValueOfObject(field.Object, field.Field);

                // в зависимости от его вида добавим контрол для редактирования
                Type type = ReflectionHelper.GetTypeOfObject(field.Object, field.Field);
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    label1.Visible = false;
                    Type tableType = typeof(ListPresentation<>).MakeGenericType(type.GetGenericArguments()[0]);

                    // TODO: DBWORK тут это жестко
                    ctrlForEdit = (Control)Activator.CreateInstance(tableType, new DBWork(), field.ListFields, field.FormType, field.Master, null, InheritMode.BaseOnly);
                    this.Controls.Add(ctrlForEdit);
                    ctrlForEdit.Dock = DockStyle.Fill;
                    this.Height = TableHeight;
                    this.Width = TableWidth;

                    if (field.ToolBarItems != null && field.ToolBarItems.Any())
                    {
                        IToolBarControl tbCtrl = (ctrlForEdit as IToolBarControl);
                        foreach (var item in field.ToolBarItems)
                        {
                            tbCtrl.AddToToolBar(item);
                        }
                    }
                }
                else if (type == typeof(int))
                {
                    int val;
                    int.TryParse(ReflectionHelper.GetValueOfObject(field.Object, field.Field), out val);
                    ctrlForEdit = AddNumericUpDown(val);
                }
                else if (type == typeof(bool))
                {
                    bool flag = (bool)ReflectionHelper.GetTypedValueOfObject(field.Object, field.Field);
                    ctrlForEdit = AddCheckBox(flag);
                }
                else if (type == typeof(DateTime))
                {
                    DateTime date;
                    if (!DateTime.TryParse(ReflectionHelper.GetValueOfObject(field.Object, field.Field), out date) || date == default(DateTime))
                    {
                        date = DateTime.Now;
                    }

                    ctrlForEdit = AddDateTimePicker(date);
                }
                else if (type == typeof(string))
                {
                    ctrlForEdit = AddTextBox();
                    ctrlForEdit.Text = ReflectionHelper.GetValueOfObject(field.Object, field.Field);
                }
                else if (type.IsEnum)
                {
                    object enumVal = ReflectionHelper.GetTypedValueOfObject(field.Object, field.Field);
                    ctrlForEdit = AddComboBox(type, Enum.GetName(type, enumVal));
                }
                else if (type == typeof(Image))
                {
                    Image picture = (Image)ReflectionHelper.GetTypedValueOfObject(field.Object, field.Field);
                    ctrlForEdit = AddPictureBox(picture);
                }
                else if (!type.IsValueType) // хорошо бы проверять, что это именно тип из БД
                {
                    // добавим контрол для отображения, если заполено поле с путём
                    TextBox tb = null;
                    tb = AddTextBox();
                    tb.ReadOnly = true;

                    if (!string.IsNullOrEmpty(field.FieldForUser))
                    {
                        ctrlForWatch = tb;
                        tb.Text = ReflectionHelper.GetValueOfObject(field.Object, field.FieldForUser);
                    }
                    else
                    {
                        ctrlForEdit = tb;
                        tb.Text = ReflectionHelper.GetValueOfObject(field.Object, field.Field);
                    }

                    // специальная кнопка для выбора объекта из списка
                    Type buttonType = typeof(OpenListFormButton<>).MakeGenericType(type);
                    Button btn = (Button)Activator.CreateInstance(buttonType, field.FormType);
                    tb.Width -= 30;
                    btn.Left = tb.Left + tb.Width + innerPadding;
                    btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    (btn as IReturnValueControl).Selected += FieldPresentation_Selected;

                    this.Controls.Add(btn);
                }
                else
                {
                    ctrlForEdit = AddTextBox();
                    ctrlForEdit.Text = ReflectionHelper.GetValueOfObject(field.Object, field.Field);
                }

                IsInit = true;
            }
        }

        /// <summary>
        /// После того, как кнопкой выбрали что-то на списковой форме
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldPresentation_Selected(object sender, EventArgs e)
        {
            object value = (sender as IReturnValueControl).Value; // значение отредактированного поля (выбранный на списке объект)

            field.Object.GetType().GetProperty(field.Field).SetValue(field.Object, value, null);

            if (ctrlForWatch != null)
            {
                ctrlForWatch.Text = ReflectionHelper.GetValueOfObject(field.Object, field.FieldForUser);
            }
            else
            {
                ctrlForEdit.Text = ReflectionHelper.GetValueOfObject(field.Object, field.Field);
            }
        }

        /// <summary>
        /// Обновляет данные, отображаемые в контроле
        /// </summary>
        public void RefreshData()
        {
            Type type = ReflectionHelper.GetTypeOfObject(field.Object, field.Field);
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (IsInit && ctrlForEdit != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                (ctrlForEdit as IRefreshedControl).RefreshData();
            }
        }

        /// <summary>
        /// Возвращает значение редактируемого поля объекта
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            object result;
            
            Type type = ReflectionHelper.GetTypeOfObject(field.Object, field.Field);
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(double))
            {
                result = double.Parse(ctrlForEdit.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            else if (type.IsEnum)
            {
                result = Enum.Parse(type, ctrlForEdit.Text);
            }
            else if (type == typeof(bool))
            {
                result = (ctrlForEdit as CheckBox).Checked;
            }
            else if (type == typeof(int) || type == typeof(string) || type.IsValueType)
            {
                MemberInfo[] memberArr = type.GetMember("Parse");
                MethodInfo mi = memberArr.Any() ? (memberArr[0] as MethodInfo) : null;

                if (mi != null)
                {
                    result = mi.Invoke(null, new object[] { ctrlForEdit.Text });
                }
                else
                {
                    result = Convert.ChangeType(ctrlForEdit.Text, type);
                }
            }
            else if (type == typeof(Image))
            {
                result = (ctrlForEdit as ExtendedPictureBox).Picture;
            }
            else
            {
                result = ReflectionHelper.GetTypedValueOfObject(field.Object, field.Field);
            }

            return result;
        }

        /// <summary>
        /// Возвращает значение поля, которое было до редактирования
        /// </summary>
        /// <returns></returns>
        public object OriginalValue()
        {
            return originalValue;
        }

    }
}
