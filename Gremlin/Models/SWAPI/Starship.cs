using System;
using System.Collections.Generic;
using System.Text;

namespace Gremlin.Models.SWAPI
{
    class Starship
    {
        public string name { get; set; }
        public string model { get; set; }
        public string manufacturer { get; set; }
        public string cost_in_credits { get; set; }
        public string length { get; set; }
        public string max_atmosphering_speed { get; set; }
        public string crew { get; set; }
        public string passengers { get; set; }
        public string cargo_capacity { get; set; }
        public string consumables { get; set; }
        public string hyperdrive_rating { get; set; }
        public string MGLT { get; set; }
        public string starship_class { get; set; }
        public object[] pilots { get; set; }
        public string url { get; set; }
    }
    class StarshipEnvelope
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public IEnumerable<Starship> results { get; set; }
    }
}
