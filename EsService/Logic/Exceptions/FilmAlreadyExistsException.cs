using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Exceptions
{
    public class FilmAlreadyExistsException : Exception
    {
        private const string defaultMessage = "Фильм уже существует в базе данных.";

        public FilmAlreadyExistsException()
            : base(defaultMessage)
        {
        }

        public FilmAlreadyExistsException(Exception inner)
            : base(defaultMessage, inner)
        {

        }
    }
}
