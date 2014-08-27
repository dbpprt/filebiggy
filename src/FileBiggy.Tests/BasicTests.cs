using System.Linq;
using FileBiggy.Attributes;
using Xunit;

namespace FileBiggy.Tests
{
    public class BasicTests
    {
        public class Entity
        {
            [Identity]
            public string Id { get; set; }

            public string Test { get; set; }
        }

        public class EntityContext : BiggyContext
        {
            public EntitySet<Entity> Entities { get; set; }

            public EntityContext() : base("provider=FileBiggy.Memory.MemoryStore`1")
            {
            }
        }

        [Fact]
        public void TestMethod1()
        {
            var context = new EntityContext();
            var entities = context.Entities;

            entities.Add(new Entity
            {
                Id = "hey",
                Test = "some value"
            });

            var all = entities.AsQueryable().ToList();
        }
    }
}