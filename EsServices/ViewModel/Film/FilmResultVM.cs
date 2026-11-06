namespace EsServices.ViewModel.Film
{
    public class FilmResultVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Year { get; set; }
        public decimal Rating { get; set; }
        public string? Description { get; set; }
        public string? Slogan { get; set; }
        public string? Poster { get; set; }
        public string? Link { get; set; }
        public string? KinopoiskId { get; set; }

        public List<string> Genres { get; set; } = new();
        public List<string> Countries { get; set; } = new();
        public List<string> Actors { get; set; } = new();
        public List<string> Producers { get; set; } = new();
        public List<FilmTagVM> Tags { get; set; } = new();
    }

    public class FilmTagVM
    {
        public string Name { get; set; } = string.Empty;
        public int Percent { get; set; }
    }

    public class FilmLookupItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
