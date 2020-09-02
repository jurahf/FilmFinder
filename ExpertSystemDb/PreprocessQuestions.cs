//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExpertSystemDb
{
    using System;
    using System.Collections.Generic;
    
    public partial class PreprocessQuestions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PreprocessQuestions()
        {
            this.GenreForFilter = new HashSet<GenreForFilter>();
            this.CustomPropertyForFilter = new HashSet<CustomPropertyForFilter>();
        }
    
        public int Id { get; set; }
        public Nullable<bool> IKnowThatIWant { get; set; }
        public FilterType ActiveFilterType { get; set; }
    
        public virtual Session Session { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GenreForFilter> GenreForFilter { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CustomPropertyForFilter> CustomPropertyForFilter { get; set; }
    }
}