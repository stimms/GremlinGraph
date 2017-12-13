using System;
using System.Collections.Generic;
using System.Text;

namespace Gremlin.Models
{
    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BirthYear { get; set; }
        public string EyeColour { get; set; }
        public int HeightInCm { get; set; }
        public List<int> StarShips { get; set; }

        public override string ToString()
        {
            return $"{Name} - born: {BirthYear}";
        }
    }
}
