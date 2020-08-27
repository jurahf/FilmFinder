using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadFilms
{
    public class TranslateApiParams
    {
        public string input { get; set; }
        public string lang { get { return "ru"; } }

        public TranslateApiParams(string text)
        {
            this.input = text;
        }
    }

    public class TranslateApiResponse
    {
        public string Text { get; set; }
        public string Translated { get; set; }
        public string Target_lang { get; set; }
        public string Source_lang { get; set; }
    }

}
