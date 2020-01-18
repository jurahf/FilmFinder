using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BaseUI
{
    public static class ReflectionHelper
    {
        private static string Nameof(MemberExpression expr)
        {
            MemberExpression rest = expr.Expression as MemberExpression;

            if (rest == null)
            {
                return expr.Member.Name;
            }
            else
            {
                return $"{Nameof(rest)}.{expr.Member.Name}";
            }
        }

        public static string GetValueOfObject(object obj, string field)
        {
            return GetTypedValueOfObject(obj, field)?.ToString() ?? "";
        }
        
        public static object GetTypedValueOfObject<T>(T obj, string field)
        {
            if (obj == null || string.IsNullOrWhiteSpace(field))
            {
                return null;
            }

            string[] path = field.Split('.');

            Type tempType = obj.GetType();
            object tempObject = obj;

            foreach (var tempField in path)
            {
                PropertyInfo pi = tempType.GetProperty(tempField);
                if (pi == null)
                {
                    tempObject = null;
                }
                else
                {
                    tempObject = pi.GetValue(tempObject, null);
                }

                if (tempObject == null)
                {
                    break;
                }

                tempType = tempObject.GetType();
            }

            return tempObject;
        }


        public static Type GetTypeOfObject(object obj, string field)
        {
            return obj.GetType().GetProperty(field).PropertyType;
        }


        public static string Nameof<T>(Expression<Func<T, object>> expr)
        {
            MemberExpression me;

            if (expr.Body.NodeType == ExpressionType.Convert
                || expr.Body.NodeType == ExpressionType.ConvertChecked)
            {
                me = (expr.Body as UnaryExpression).Operand as MemberExpression;
            }
            else
            {
                me = expr.Body as MemberExpression;
            }

            return Nameof(me);
        }


        public static string GetTypeName(Type type)
        {
            string typeName = type.Name;
            if (typeName.Contains('_'))
            {
                typeName = typeName.Substring(0, typeName.IndexOf('_'));
            }

            return typeName;
        }

    }
}
