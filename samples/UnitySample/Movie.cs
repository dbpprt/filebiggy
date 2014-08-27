using System;
using FileBiggy.Attributes;

namespace UnitySample
{
    public class Movie
    {
        [Identity]
        public Guid MovieId { get; set; }

        public string Name { get; set; }

        public string[] Genres { get; set; }

        public int Year { get; set; }

        // and some other properties
    }
}
