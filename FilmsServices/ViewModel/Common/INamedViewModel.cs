namespace FilmsServices.ViewModel.Common
{
    /// <summary>
    /// ViewModel справочника, у которого редактируется имя.
    /// </summary>
    public interface INamedViewModel
    {
        string Name { get; set; }
    }
}
