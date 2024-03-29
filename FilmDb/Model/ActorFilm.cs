﻿using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class ActorFilm : BaseEntity
    {
        public virtual int FilmId { get; set; }
        public virtual Film Film { get; set; }

        public virtual int ActorId { get; set; }
        public virtual Actor Actor { get; set; }
    }
}
