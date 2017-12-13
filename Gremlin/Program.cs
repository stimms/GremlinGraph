using Gremlin.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Gremlin
{
    class Program
    {
        static Config configuration;
        static async Task Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();
                configuration = new Config();
                builder.GetSection("CosmosDB").Bind(configuration);
                Console.WriteLine("Fetching collection");
                var people = await GetStarWarsPeople();
                foreach (var person in people)
                    Console.WriteLine(person.ToString());
                Console.WriteLine($"Connecting with {configuration.Endpoint} - {configuration.AuthKey}");
                using (var client = new DocumentClient(new Uri(configuration.Endpoint), configuration.AuthKey))
                {
                    var database = (Database)await client.CreateDatabaseIfNotExistsAsync(new Database { Id = configuration.DatabaseName });

                    var graph = (DocumentCollection)await client.CreateDocumentCollectionIfNotExistsAsync(
                        UriFactory.CreateDatabaseUri(configuration.DatabaseName),
                        new DocumentCollection { Id = configuration.CollectionName },
                        new RequestOptions { OfferThroughput = 1000 });
                    await AddThing(client, graph);
                }
                Console.WriteLine("done.");
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.Error.Write(ex.Message);
                Console.Error.Write(ex);
                Console.ReadLine();
            }
        }

        private static async Task<IEnumerable<Person>> GetStarWarsPeople()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress =new Uri("https://swapi.co");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResult = await httpClient.GetAsync("/api/people");
            var result = await httpResult.Content.ReadAsStringAsync();
            var models = JsonConvert.DeserializeObject<Models.SWAPI.PersonEnvelope>(result);
            return models.results.Select(x => new Person
            {
                Id = int.Parse(x.url.Trim('/').Split('/').Last()),
                Name = x.name, 
                BirthYear = x.birth_year,
                EyeColour = x.eye_color, 
                HeightInCm = int.Parse(x.height),
                StarShips = x.starships.Select(y=> int.Parse(y.Trim('/').Split('/').Last())).ToList()
            });
        }

        private static async Task AddThing(DocumentClient client, DocumentCollection graph)
        {
            var clearQuery = client.CreateGremlinQuery(graph, "g.v().drop()");
            await clearQuery.ExecuteNextAsync();
            await client.CreateGremlinQuery(graph, "g.addV('person').property('id', 'simon').property('firstName', 'Simon')").ExecuteNextAsync();
            
        }
    }
}
