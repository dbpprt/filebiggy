using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileBiggy.Attributes;
using FileBiggy.Contracts;
using FileBiggy.Factory;
using Xunit;

namespace FileBiggy.Tests
{
    public class BasicMultithreading
    {
        public class Widget
        {
            [Identity]
            public Guid SKU { get; set; }

            public string Name { get; set; }
            public Decimal Price { get; set; }
        }

        public class EntityContext : BiggyContext
        {
            public EntityContext(string connectionString)
                : base(connectionString)
            {
            }

            public EntitySet<Widget> Widgets { get; set; }
        }

        private IEntitySet<Widget> _widgets;
        private string path;

        public BasicMultithreading()
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data", Guid.NewGuid().ToString());
        }

        private void Recreate(bool clear = true)
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(path)
                .Build();

            _widgets = context.Set<Widget>();
            if (clear) _widgets.Clear();
        }

        private void BsonRecreate(bool clear = true)
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsBsonDatabase()
                .WithDatabaseDirectory(path)
                .Build();

            _widgets = context.Set<Widget>();
            if (clear) _widgets.Clear();
        }

        private void AddSomeItems(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _widgets.Add(new Widget
                {
                    SKU = Guid.NewGuid(),
                    Name = "test",
                    Price = 214
                });
            }
        }

        private void QuerySomething()
        {
            var matches = _widgets.AsQueryable().Where(_ => _.SKU.ToString().Contains("d1"));
        }

        private void EnumerateSomething()
        {
            foreach (var widget in _widgets)
            {
                var sku = widget.SKU;
            }
        }

        [Fact]
        public void Multithreaded_Insert()
        {
            Recreate();

            var tasks = new List<Task>();
            var inserts = 10;
            var taskCount = 100;

            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(new Task(() => AddSomeItems(inserts)));
                tasks.Add(new Task(QuerySomething));
                tasks.Add(new Task(EnumerateSomething));
            }

            tasks.ForEach(_ => _.Start());
            Task.WaitAll(tasks.ToArray());

            Recreate(false);

            Assert.Equal(inserts*taskCount, _widgets.Count());
        }

        [Fact]
        public void Multithreaded_Mixture()
        {
            Recreate();

            var tasks = new List<Task>();
            var inserts = 10;
            var taskCount = 100;

            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(new Task(() => AddSomeItems(inserts)));
            }

            tasks.ForEach(_ => _.Start());
            Task.WaitAll(tasks.ToArray());

            Recreate(false);

            Assert.Equal(inserts*taskCount, _widgets.Count());
        }

        [Fact]
        public void Bson_Multithreaded_Insert()
        {
            BsonRecreate();

            var tasks = new List<Task>();
            var inserts = 10;
            var taskCount = 100;

            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(new Task(() => AddSomeItems(inserts)));
                tasks.Add(new Task(QuerySomething));
                tasks.Add(new Task(EnumerateSomething));
            }

            tasks.ForEach(_ => _.Start());
            Task.WaitAll(tasks.ToArray());

            BsonRecreate(false);

            Assert.Equal(inserts*taskCount, _widgets.Count());
        }

        [Fact]
        public void Bson_Multithreaded_Mixture()
        {
            BsonRecreate();

            var tasks = new List<Task>();
            var inserts = 10;
            var taskCount = 100;

            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(new Task(() => AddSomeItems(inserts)));
            }

            tasks.ForEach(_ => _.Start());
            Task.WaitAll(tasks.ToArray());

            BsonRecreate(false);

            Assert.Equal(inserts*taskCount, _widgets.Count());
        }
    }
}