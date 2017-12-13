using System;
using System.Collections.Generic;
using System.Text;

namespace Gremlin.Models.SWAPI
{
    class PersonEnvelope
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public IEnumerable<Person> results { get; set; }
    }
    class Person
    {
        public string name { get; set; }
        public string birth_year { get; set; }
        public string height { get; set; }
        public string eye_color { get; set; }
        public List<string> starships { get; set; }
        public string url { get; set; }

        public override string ToString()
        {
            return $"{name} - born: {birth_year}";
        }
    }
}
