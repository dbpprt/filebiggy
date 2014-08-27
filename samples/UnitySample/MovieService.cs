using System;
using System.Collections.Generic;
using System.Linq;
using FileBiggy.Contracts;

namespace UnitySample
{
    public class MovieService : IMovieService
    {
        private readonly IEntitySet<Movie> _movies;

        public MovieService(
            IEntitySet<Movie> movies
            )
        {
            _movies = movies;
        }

        public IEnumerable<Movie> GetMovies()
        {
            return _movies.ToList();
        }

        public void AddMovie(Guid id, string name, string[] genres, int year)
        {
            _movies.Add(new Movie
            {
                MovieId = id,
                Genres = genres,
                Name = name,
                Year = year
            });
        }
    }
}
