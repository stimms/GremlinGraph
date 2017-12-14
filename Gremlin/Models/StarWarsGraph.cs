using System;
using System.Collections.Generic;
using System.Text;

namespace Gremlin.Models
{
    class StarWarsGraph
    {
        public IEnumerable<Person> People { get; set; }
        public IEnumerable<Starship> Starships { get; set; }

    }
}
