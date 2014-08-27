using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileBiggy.Factory;
using Newtonsoft.Json;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;


            var jsonContext = ContextFactory.Create<MovieContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(Path.Combine(currentDirectory, "json"))
                .Build();

            var bsonContext = ContextFactory.Create<MovieContext>()
                .AsBsonDatabase()
                .WithDatabaseDirectory(Path.Combine(currentDirectory, "bson"))
                .Build();

            var memoryContext = ContextFactory.Create<MovieContext>()
                .AsInMemoryDatabase()
                .Build();

            var movie = new Movie
            {
                Genres = new[] {"Action"},
                MovieId = Guid.NewGuid(),
                Name = "The Dark Knight",
                Year = 2008
            };

            jsonContext.Movies.Clear();
            bsonContext.Movies.Clear();

            jsonContext.Movies.Add(movie);
            bsonContext.Movies.Add(movie);
            memoryContext.Movies.Add(movie);

            var bsonEntity = bsonContext.Movies.Single(entity => entity.Name.Contains("Dark"));
            var jsonEntity = jsonContext.Movies.Single(entity => entity.Name.Contains("Dark"));
            var memoryEntity = memoryContext.Movies.Single(entity => entity.Name.Contains("Dark"));

            Console.WriteLine(JsonConvert.SerializeObject(bsonEntity, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(jsonEntity, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(memoryEntity, Formatting.Indented));

            Console.Read();
        }
    }
}
