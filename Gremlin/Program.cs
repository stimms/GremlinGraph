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
                Console.WriteLine($"Connecting with {configuration.Endpoint} - {configuration.AuthKey}");
                using (var client = new DocumentClient(new Uri(configuration.Endpoint), configuration.AuthKey))
                {
                    var database = (Database)await client.CreateDatabaseIfNotExistsAsync(new Database { Id = configuration.DatabaseName });

                    var graph = (DocumentCollection)await client.CreateDocumentCollectionIfNotExistsAsync(
                        UriFactory.CreateDatabaseUri(configuration.DatabaseName),
                        new DocumentCollection { Id = configuration.CollectionName },
                        new RequestOptions { OfferThroughput = 1000 });
                    var command = "";
                    while (!command.Trim().Equals("q", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("1. Rebuild database");
                        Console.WriteLine("2. Query");
                        Console.WriteLine("q. Quit");
                        command = Console.ReadLine();
                        if (command.Trim() == "1")
                        {
                            Console.WriteLine("Fetching collection");
                            var swGraph = await new StarWarsGraphGetter().Get();
                            await PopulateGraph(swGraph, client, graph);
                            Console.WriteLine("Done loading graph");
                        }
                        if (command.Trim() == "2")
                        {
                            await PresentQueryMenu(client, graph);
                        }
                    }
                }

                Console.WriteLine("done.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
                Console.Error.Write(ex);
                Console.ReadLine();
            }
        }

        private static async Task PopulateGraph(StarWarsGraph swGraph, DocumentClient client, DocumentCollection graph)
        {
            await Clear(client, graph);
            await AddPeople(client, graph, swGraph.People);
            await AddStarships(client, graph, swGraph.Starships);
            await AddPeopleStarshipEdges(client, graph, swGraph.People);
        }

        static async Task PresentQueryMenu(DocumentClient client, DocumentCollection graph)
        {
            var readString = "";
            while (!readString.Trim().Equals("q", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Pick a query:");
                Console.WriteLine("1. All people");
                Console.WriteLine("2. All starships");
                Console.WriteLine("3. All starships with a crew > 10 000");
                Console.WriteLine("4. People who served on starships with a length > 100m");
                Console.WriteLine("5. People who served together");
                Console.WriteLine("q. Quit");
                readString = Console.ReadLine().Trim();
                var query = "";
                switch (readString)
                {
                    case "1":
                        query = "g.V().hasLabel('person')";
                        break;
                    case "2":
                        query = "g.V().hasLabel('starship')";
                        break;
                    case "3":
                        query = "g.V().hasLabel('starship').has('crew', gt(10000))";
                        break;
                    case "4":
                        query = "g.V().hasLabel('starship').has('length', gt(100)).inE('servedOn').outV().dedup()";
                        break;
                    case "5":
                        query = "g.V().hasLabel('person').as('a').outE('servedOn').inV().as('c').inE('servedOn').outV().as('b').select('a','b','c').by('name').where('a',neq('b'))";
                        break;
                }
                if (!String.IsNullOrEmpty(query))
                {
                    var graphQuery = client.CreateGremlinQuery(graph, query);
                    do
                    {
                        var chunk = await graphQuery.ExecuteNextAsync();
                        Console.WriteLine($"\t {JsonConvert.SerializeObject(chunk, Formatting.Indented)}");

                    } while (graphQuery.HasMoreResults);
                }
            }

        }

        private static async Task Clear(DocumentClient client, DocumentCollection graph)
        {
            var clearQuery = client.CreateGremlinQuery(graph, "g.v().drop()");
            await clearQuery.ExecuteNextAsync();
        }

        private static async Task AddPeople(DocumentClient client, DocumentCollection graph, IEnumerable<Person> people)
        {
            foreach (var person in people)
                await client.CreateGremlinQuery(graph, $"g.addV('person').property('id', 'person:{person.Id}').property('name', '{person.Name}')").ExecuteNextAsync();

        }

        private static async Task AddStarships(DocumentClient client, DocumentCollection graph, IEnumerable<Starship> starships)
        {
            foreach (var starship in starships)
                await client.CreateGremlinQuery(graph, $@"g.addV('starship').property('id', 'starship:{starship.Id}')
                                                            .property('name', '{starship.Name}')
                                                            .property('manufacturer', '{starship.Manufacturer}')
                                                            .property('crew', {starship.Crew})
                                                            .property('length', {starship.Length})").ExecuteNextAsync();

        }
        private static async Task AddPeopleStarshipEdges(DocumentClient client, DocumentCollection graph, IEnumerable<Person> people)
        {
            foreach (var person in people)
                foreach (var starship in person.StarShips)
                    await client.CreateGremlinQuery(graph, $"g.V('person:{person.Id}').addE('servedOn').to(g.V('starship:{starship}'))").ExecuteNextAsync();
        }
    }
}
