using System;
using System.IO;
using System.Linq;
using FileBiggy.Contracts;
using FileBiggy.Factory;
using FileBiggy.IoC;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace UnitySample
{
    class Program
    {
        private readonly IMovieService _movieService;

        public Program(
            IMovieService movieService)
        {
            _movieService = movieService;
        }


        static void Main(string[] args)
        {
            var container = new UnityContainer();
            RegisterDependecies(container);

            container.RegisterType<Program>(new ContainerControlledLifetimeManager());

            using (var scope = container.CreateChildContainer())
            {
                var program = scope.Resolve<Program>();
                program.DoSomethingWithMovies();
            }
        }

        static void RegisterDependecies(IUnityContainer container)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;


            var jsonContext = ContextFactory.Create<MovieContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(Path.Combine(currentDirectory, "json"))
                .Build();

            container.RegisterInstance<IBiggyContext>(jsonContext);
            container.RegisterType(typeof(IEntitySet<>), typeof(Repository<>));
            container.RegisterType<IMovieService, MovieService>(new ContainerControlledLifetimeManager());
        }

        public void DoSomethingWithMovies()
        {
            _movieService.AddMovie(
                Guid.NewGuid(),
                "The Dark Knight",
                new[] { "Action" },
                2008
                );

            _movieService.AddMovie(
                Guid.NewGuid(),
                "The Dark Horse",
                new[] { "Drama" },
                2028
                );

            var myMovies = _movieService.GetMovies().ToList();
            Console.WriteLine(JsonConvert.SerializeObject(myMovies));
            Console.ReadLine();
        }
    }
}
