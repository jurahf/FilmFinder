﻿using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class Fact : BaseEntity
    {
        public virtual DomainValue DomainValue { get; set; }

        public virtual Variable Variable { get; set; }
    }
}
