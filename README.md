# FileBiggy: A simple document store with json, bson, and in memory stores

I looked for a small embedded database for a small website. I dont want to setup a full database engine just to store some megabytes of data. So i found Rob Connery's biggy which covers my aspects. But biggy implements alot of other features which i dont need, like psql, mongo, etc. So i created FileBiggy which is just a document database with a file storage. 

FileBiggy ships a BiggyContext, which is an entityframework inspired data context. Just define some entities, a context and it works.

FileBiggy supports a JsonStorage, a BsonStorage (good to store files in a managed manner), and a simple in memory store for unit testing.

Data is loaded into memory when your application starts, and you query it with Linq. That's it. It loads incredibly fast (100,000 records in about 1 second) and from there will sync your in-memory list with whatever store you choose. 

## Getting started

Let's say you want to store some movies in your database so define a movie entity

```csharp
public class Movie
{
    [Identity]
    public string MovieId { get; set; }

    public string Name { get; set; }

    public string[] Genres { get; set; }

    public int Year { get; set; }

    // and some other properties
}
```

The next step is to define your context.

```csharp
public class MovieContext : BiggyContext
{
    public MovieContext(string connectionString)
        : base(connectionString)
    {
    }

    public EntitySet<Movie> Movies { get; set; }
}
```

And that's it. Context classes should be stored as singleton instances, because they keep the data synchronized in memory.
FileBiggy has a simple fluent api to obtain a context instance:

```csharp
var jsonContext = ContextFactory.Create<MovieContext>()
    .AsJsonDatabase()
    .WithDatabaseDirectory("C:\\mydatabase")
    .Build();

// this wont work. you need to change Movie's identity from string to Guid
var bsonContext = ContextFactory.Create<MovieContext>()
    .AsBsonDatabase()
    .WithDatabaseDirectory("C:\\mydatabase")
    .Build();

var memoryContext = ContextFactory.Create<MovieContext>()
    .AsInMemoryDatabase()
    .Build();
```

Let's add some stuff to our created database:

```csharp
var movies = jsonContext.Movies;
var theSameMovies = jsonContext.Set<Movie>();

movies.Add(new Movie
{
    Genres = new[] { "Action" },
    MovieId = "tt0468569",
    Name = "The Dark Knight",
    Year = 2008
});

var darkKnight = movies.Where(movie => movie.Name.Contains("Dark"));
```

## Features

- EntitySets are threadsafe (had some problems with biggy's non-threadsafe singleton instances)
- Dynamic data scheme
- IdentityAttribute marks a key, and gives you really fast access to these entities
- Less dependencies and a small assembly (about 35kB)
- Entityframework like datacontext which is nice for IoC uses (i'll add a sample solution)
- Clear interfaces and good capabilities to add your own fancy Store


## Limitations

- each BiggyContext must contain only one EntitySet per type
- BsonStore requires a Guid Identity
- BsonStore isnt really optimized at the moment but quite good for storing files

## How to install

Just grad the FileBiggy package from nuget and install it

```
Install-Package FileBiggy
```

## Roadmap

- add builds for other platfroms (phone, xamarin, etc)
- add some other usefull attributes [Expires], [ForeignIdentity]
- add some sample projects
- the issues for more informations
