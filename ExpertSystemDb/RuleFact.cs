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
    
    public partial class RuleFact
    {
        public int Id { get; set; }
        public Nullable<int> Order { get; set; }
    
        public virtual Rule Rule { get; set; }
        public virtual Fact Fact { get; set; }
    }
}
