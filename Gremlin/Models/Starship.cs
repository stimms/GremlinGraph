using System;
using System.Collections.Generic;
using System.Text;

namespace Gremlin.Models
{
    class Starship
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal Length { get; set; }
        public string StarshipClass { get; set; }
        public decimal HyperDriveRating { get; set; }
        public decimal Crew { get; set; }
    }
}
