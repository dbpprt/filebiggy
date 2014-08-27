using FileBiggy;
using FileBiggy.Contracts;

namespace UnitySample
{
    public class MovieContext : BiggyContext, IBiggyContext
    {
        public MovieContext(string connectionString)
            : base(connectionString)
        {
        }

        public EntitySet<Movie> Movies { get; set; }
    }
}
