using FilmDb.Model;
using FilmsServices.Converters.Common;
using FilmsServices.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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

        public Advice ConvertToDb(AdviceVM viewModel)
        {
            var advice = new Advice()
            {
                Id = viewModel.Id,
                Key = viewModel.Key
            };
            
            advice.AdviceCustomProperty = viewModel.CustomProperties
                .Select(x => new AdviceCustomProperty()
                {
                    Value = x.Value,
                    Advice = advice,
                    CustomProperty = customPropConverter.ConvertToDb(x),
                }).ToHashSet();

            advice.AdviceFilm = viewModel.Films
                .Select(x => new AdviceFilm()
                {
                    //Value = x.        // TODO:
                    Advice = advice,
                    Film = filmConverter.ConvertToDb(x),
                })
                .ToHashSet();
            
            return advice;
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
