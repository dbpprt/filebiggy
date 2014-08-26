using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            public string SKU { get; set; }
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

        IEntitySet<Widget> _widgets;
        private string path;

        public BasicMultithreading()
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data", Guid.NewGuid().ToString());
        }

        void Recreate(bool clear = true)
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(path)
                .Build();

            _widgets = context.Set<Widget>();
            if (clear) _widgets.Clear();
        }

        void AddSomeItems(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _widgets.Add(new Widget
                {
                    SKU = Guid.NewGuid().ToString(),
                    Name = "test",
                    Price = 214
                });
            }
        }

        void QuerySomething()
        {
            var matches = _widgets.AsQueryable().Where(_ => _.SKU.Contains("d1"));
        }

        void EnumerateSomething()
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

            Assert.Equal(inserts * taskCount, _widgets.Count());
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

            Assert.Equal(inserts * taskCount, _widgets.Count());
        }
    }
}
