using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Microsoft.Extensions.Configuration;
using System;
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
                    await AddThing(client, graph);
                }
                Console.WriteLine("done.");
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.Error.Write(ex.Message);
            }
        }

        private static async Task AddThing(DocumentClient client, DocumentCollection graph)
        {
            var clearQuery = client.CreateGremlinQuery(graph, "g.v().drop()");
            await clearQuery.ExecuteNextAsync();
            await client.CreateGremlinQuery(graph, "g.addV('person').property('id', 'simon').property('firstName', 'Simon')").ExecuteNextAsync();
            
        }
    }
}
