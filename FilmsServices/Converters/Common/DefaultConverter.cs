using CommonRepositories;
using FilmsServices.ViewModel.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Converters.Common
{
    public class DefaultConverter<DB, VM> : IEntityConverter<DB, VM>
        where DB : BaseEntity, new ()
        where VM : BaseViewModel, new ()
    {
        public DB ConvertToDb(VM viewModel)
        {
            return ConvertProperties<VM, DB>(viewModel);
        }

        public VM ConvertToVm(DB database)
        {
            return ConvertProperties<DB, VM>(database);
        }

        private DEST ConvertProperties<SOURCE, DEST>(SOURCE src)
            where SOURCE : new()
            where DEST : new()
        {
            DEST dest = new DEST();
            Type destType = typeof(DEST);
            Type srcType = typeof(SOURCE);
            foreach (var property in srcType.GetProperties())
            {
                if (IsCollectionType(property.PropertyType))
                    continue;

                var convertedProp = destType.GetProperty(property.Name);

                if (convertedProp != null)
                {
                    var value = property.GetValue(src);
                    convertedProp.SetValue(dest, value);
                }

            }

            return dest;
        }

        private bool IsCollectionType(Type type)
        {
            return (type.Name != nameof(String)
                   && type.GetInterface(nameof(IEnumerable)) != null);
        }
    }
}
