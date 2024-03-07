using FilmDb.Model;
using FilmsServices.Converters.Common;
using FilmsServices.ViewModel;

namespace FilmsServices.Converters
{
    public class AdviceConverter : IEntityConverter<Advice, AdviceVM>
    {
        private readonly IEntityConverter<CustomProperty, CustomPropertyVM> customPropConverter;
        private readonly IEntityConverter<Film, FilmVM> filmConverter;

        public AdviceConverter()
        {
            customPropConverter = new DefaultConverter<CustomProperty, CustomPropertyVM>();
            filmConverter = new DefaultConverter<Film, FilmVM>();
        }

        public Advice FillDb(Advice advice, AdviceVM viewModel)
        {
            // переносим значения полей
            advice.Key = viewModel.Key;

            FillAdviceCustomProp(advice, viewModel);
            FillAdviceFilm(advice, viewModel);

            return advice;
        }

        // TODO:

        private void FillAdviceCustomProp(Advice advice, AdviceVM viewModel)
        {
            // удяляем пропавшие связи
            List<AdviceCustomProperty> forDel = new List<AdviceCustomProperty>();
            foreach (var acpDb in advice.AdviceCustomProperty)
            {
                if (!viewModel.CustomProperties.Any(x => x.Id == acpDb.CustomPropertyId))
                {
                    forDel.Add(acpDb);
                }
            }

            foreach (var del in forDel)
            {
                advice.AdviceCustomProperty.Remove(del);
            }

            // еще и редактируем связи
            foreach (var vm in viewModel.CustomProperties)
            {
                AdviceCustomProperty? acp = advice.AdviceCustomProperty.FirstOrDefault(x => x.CustomPropertyId == vm.Id);

                if (acp != null)
                {
                    acp.Value = vm.Value;
                }
            }

            // добавляем новые связи
            foreach (var vm in viewModel.CustomProperties)
            {
                AdviceCustomProperty? acp = advice.AdviceCustomProperty.FirstOrDefault(x => x.CustomPropertyId == vm.Id);

                if (acp == null)
                {
                    acp = new AdviceCustomProperty
                    {
                        Advice = advice,
                        CustomPropertyId = vm.Id,
                        Value = vm.Value
                    };

                    advice.AdviceCustomProperty.Add(acp);
                }
            }
        }

        private void FillAdviceFilm(Advice advice, AdviceVM viewModel)
        {
            // удяляем пропавшие связи
            List<AdviceFilm> forDel = new List<AdviceFilm>();
            foreach (var db in advice.AdviceFilm)
            {
                if (!viewModel.Films.Any(x => x.Id == db.FilmId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                advice.AdviceFilm.Remove(del);
            }

            // добавляем новые связи
            foreach (var vm in viewModel.Films)
            {
                AdviceFilm? adviceFilm = advice.AdviceFilm.FirstOrDefault(x => x.FilmId == vm.Id);

                if (adviceFilm == null)
                {
                    adviceFilm = new AdviceFilm()
                    {
                        Advice = advice,
                        FilmId = vm.Id,
                        //Value = vm.Value,     // TODO:
                    };

                    advice.AdviceFilm.Add(adviceFilm);
                }
            }
        }



        public AdviceVM ConvertToVm(Advice database)
        {
            return new AdviceVM()
            {
                Id = database.Id,
                Key = database.Key,
                Films = database.AdviceFilm
                            .Select(x => filmConverter.ConvertToVm(x.Film))     // TODO: Value потерялось
                            .ToList(),
                CustomProperties = database.AdviceCustomProperty
                            .Select(x =>
                            {
                                var res = customPropConverter.ConvertToVm(x.CustomProperty);
                                res.Value = x.Value;
                                return res;
                            })
                            .ToList()
            };
        }
    }
}
