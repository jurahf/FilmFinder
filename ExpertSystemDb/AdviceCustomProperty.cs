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
    
    public partial class AdviceCustomProperty
    {
        public int Id { get; set; }
        public int Value { get; set; }
    
        public virtual CustomProperty CustomProperty { get; set; }
        public virtual Advice Advice { get; set; }
    }
}
