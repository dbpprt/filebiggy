using FileBiggy;

namespace ConsoleApplication
{
    public class MovieContext : BiggyContext
    {
        public MovieContext(string connectionString)
            : base(connectionString)
        {
        }

        public EntitySet<Movie> Movies { get; set; }
    }
}
