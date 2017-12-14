using Gremlin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gremlin
{
    class StarWarsGraphGetter
    {
        public async Task<StarWarsGraph> Get()
        {
            return new StarWarsGraph
            {
                People = await GetStarWarsPeople(),
                Starships = await GetStarships()
            };
        }
        private async Task<IEnumerable<Person>> GetStarWarsPeople(int page = 1)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://swapi.co");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResult = await httpClient.GetAsync($"/api/people/?page={page}");
            var result = await httpResult.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.SWAPI.PersonEnvelope>(result);

            var results = models.results.Select(x => new Person
            {
                Id = int.Parse(x.url.Trim('/').Split('/').Last()),
                Name = x.name,
                BirthYear = x.birth_year,
                EyeColour = x.eye_color,
                HeightInCm = int.TryParse(x.height, out _) ? int.Parse(x.height) : 0,
                StarShips = x.starships.Select(y => int.Parse(y.Trim('/').Split('/').Last())).ToList()
            }).ToList();
            if (models.next != null)
                results.AddRange(await GetStarWarsPeople(page + 1));
            return results;
        }

        private async Task<IEnumerable<Starship>> GetStarships(int page = 1)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://swapi.co");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResult = await httpClient.GetAsync($"/api/starships/?page={page}");
            var result = await httpResult.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.SWAPI.StarshipEnvelope>(result);

            var results = models.results.Select(x => new Starship
            {
                Id = int.Parse(x.url.Trim('/').Split('/').Last()),
                Name = x.name,
                HyperDriveRating = decimal.TryParse(x.hyperdrive_rating, out _) ? decimal.Parse(x.hyperdrive_rating) : 0,
                Length = decimal.TryParse(x.length, out _) ? decimal.Parse(x.length) : 0,
                Manufacturer = x.manufacturer,
                StarshipClass = x.starship_class, 
                Crew = decimal.TryParse(x.crew, out _) ? decimal.Parse(x.crew) : 0
            }).ToList();
            if (models.next != null)
                results.AddRange(await GetStarships(page + 1));
            return results;
        }

    }
}
