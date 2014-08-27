using System;
using System.IO;
using System.Linq;
using FileBiggy.Attributes;
using FileBiggy.Factory;
using FileBiggy.Json;
using FileBiggy.Memory;
using Xunit;

namespace FileBiggy.Tests
{
    public class ContextFactoryTests
    {
        public class Entity
        {
            [Identity]
            public string Id { get; set; }

            public string Test { get; set; }
        }

        public class EntityContext : BiggyContext
        {
            public EntityContext(string connectionString)
                : base(connectionString)
            {

            }

            public EntitySet<Entity> Entities { get; set; } 
        }

        [Fact]
        public void Create_Memory_Store()
        {
            var context = ContextBuilderExtensions.AsInMemoryDatabase(ContextFactory.Create<EntityContext>())
                .Build();

            Assert.IsType(typeof(EntityContext), context);
            Assert.Equal(context.UnderlayingStore, typeof(MemoryStore<>));
        }

        [Fact]
        public void Create_Json_Store()
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tests-", Guid.NewGuid().ToString()))
                .Build();

            Assert.IsType(typeof(EntityContext), context);
            Assert.Equal(context.UnderlayingStore, typeof(JsonStore<>));
        }
    }
}
