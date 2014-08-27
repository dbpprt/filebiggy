using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitySample
{
    public interface IMovieService
    {
        IEnumerable<Movie> GetMovies();

        void AddMovie(Guid id, string name, string[] genres, int year);
    }
}
