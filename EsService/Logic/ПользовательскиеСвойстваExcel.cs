using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpertSystemDb;
using NetOffice;
using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

namespace Logic
{
    // TODO: тут все надо на методы разбить

    public class ПользовательскиеСвойстваExcel
    {
        private IDataWork db;
        private const int ColumnDataStart = 4;
        private const int RowDataStart = 3;

        public ПользовательскиеСвойстваExcel()
        {
            db = new DBWork();
        }

        public void ToExcel()
        {
            //                              | названия свойств
            // -----------------------------+------------------
            // id | название фильма | год   | свойство в фильме

            Excel.Application app = null;
            try
            {
                // создаем excel
                app = new Excel.Application();
                app.DisplayAlerts = false;
                Excel.Workbook book = app.Workbooks.Add();
                Excel.Worksheet sheet = (Excel.Worksheet)book.Worksheets[1];

                // сам отчет
                var filmList = db.GetFromDatabase<Film>().OrderBy(x => x.Name).ToList();
                var propList = db.GetFromDatabase<CustomProperty>().OrderBy(x => x.Name).ToList();

                // заголовки для свойств
                for (int j = 0; j < propList.Count; j++)
                {
                    var prop = propList[j];
                    int columnIndex = j + ColumnDataStart;

                    sheet.Cells[1, columnIndex].Value = prop.Id;
                    sheet.Cells[2, columnIndex].Value = prop.Name;
                }

                // заголовки фильмов и значения свойств
                for (int i = 0; i < filmList.Count; i++)
                {
                    var film = filmList[i];
                    int rowIndex = i + RowDataStart;

                    sheet.Cells[rowIndex, 1].Value = film.Id;
                    sheet.Cells[rowIndex, 2].Value = film.Name;
                    sheet.Cells[rowIndex, 3].Value = film.Year;

                    for (int j = 0; j < propList.Count; j++)
                    {
                        var prop = propList[j];
                        int columnIndex = j + ColumnDataStart;

                        FilmCustomProperty filmProp =
                            db.GetFromDatabase<FilmCustomProperty>(x => x.Film.Id == film.Id && x.CustomProperty.Id == prop.Id).FirstOrDefault();

                        if (filmProp != null)
                            sheet.Cells[rowIndex, columnIndex].Value = filmProp.Value;
                    }
                }

                // показваем, что получилось
                app.Visible = true;
            }
            catch (Exception ex)
            {
                if (app != null)
                {
                    app.Quit();
                    app.Dispose();
                }

                throw;
            }
        }


        public bool FromExcel(string fileName)
        {
            // открываем книгу
            // идем по свойствам
            // если такого еще нет - добавляем
            // если было - удаляем все связанные значения
            // находим числа в столбце
            // добавляем связанные значения с фильмами из строки

            Excel.Application app = null;
            try
            {
                app = new Excel.Application();
                app.DisplayAlerts = false;
                Excel.Workbook book = app.Workbooks.Open(fileName);
                Excel.Worksheet sheet = (Excel.Worksheet)book.Worksheets[1];

                // загружаем используемые фильмы в список
                List<int> usedFilmIds = new List<int>();
                int i = 0;
                int? filmId = null;
                do
                {
                    filmId = GetInt(sheet.Cells[RowDataStart + i, 1].Value);
                    if (filmId != null)
                        usedFilmIds.Add(filmId.Value);

                    i++;
                } while (filmId != null);

                List<Film> usedFilms = db.GetFromDatabase<Film>(x => usedFilmIds.Contains(x.Id));

                // идем по свойствам
                i = 0;
                string propName = "";
                CustomProperty prop = null;
                do
                {
                    int? propId = GetInt(sheet.Cells[1, ColumnDataStart + i].Value);
                    propName = GetString(sheet.Cells[2, ColumnDataStart + i].Value);

                    if (string.IsNullOrEmpty(propName) && propId == null)
                        break;

                    if (propId != null)
                        prop = db.GetFromDatabase<CustomProperty>(x => x.Id == propId).FirstOrDefault();
                    else
                        prop = db.GetFromDatabase<CustomProperty>(x => x.Name == propName).FirstOrDefault();

                    if (prop != null)
                    {
                        // удаляем значения для используемых фильмов
                        var filmProp = db.GetFromDatabase<FilmCustomProperty>(x => 
                            x.CustomProperty.Id == prop.Id
                            && usedFilmIds.Contains(x.Film.Id));
                        db.DeleteObject(filmProp);  // удаляем значения, чтобы обновить их из таблицы
                    }
                    else
                    {
                        // новое свойство
                        prop = new CustomProperty() { Name = propName };
                        db.Insert(prop);
                    }

                    // добавляем значения свойств
                    int j = 0;
                    filmId = null;
                    do
                    {
                        filmId = GetInt(sheet.Cells[RowDataStart + j, 1].Value);
                        int? propValue = GetInt(sheet.Cells[RowDataStart + j, ColumnDataStart + i].Value);

                        if (propValue != null)
                        {
                            var filmProp = new FilmCustomProperty()
                            {
                                Film = usedFilms.First(x => x.Id == filmId),
                                CustomProperty = prop,
                                Value = propValue.Value
                            };

                            db.Insert(filmProp);
                        }

                        j++;
                    } while (filmId != null);

                    i++;
                } while (!string.IsNullOrEmpty(propName));
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (app != null)
                {
                    app.Quit();
                    app.Dispose();
                }
            }


            return true;
        }

        private string GetString(object value)
        {
            if (value == null)
                return "";
            else
                return value.ToString();
        }

        private int? GetInt(object value)
        {
            if (value == null)
                return null;
            else
            {
                if (int.TryParse(value.ToString(), out int result))
                    return result;
                else
                    return null;
            }
        }
    }
}
