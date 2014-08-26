using System;
using FileBiggy.Attributes;
using Xunit;

namespace FileBiggy.Tests
{
    public class IdentityMemoryStoreTests
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

            public EntityContext()
                : base("provider=FileBiggy.Memory.MemoryStore`1")
            {

            }
        }

        [Fact]
        public void Identity_Column_Recognized()
        {
            var context = new EntityContext();
            var entities = context.Set<Entity>();

            var entity = new Entity
            {
                Id = "some string id",
                Test = "some value"
            };

            var anotherEntity = new Entity
            {
                Id = "some string id",
                Test = "some other value"
            };

            entities.Add(entity);
            Assert.Throws<ArgumentException>(() => entities.Add(anotherEntity));

        }

    }
}
