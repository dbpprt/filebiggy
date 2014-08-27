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
    public class AsyncTests
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

        private IEntitySet<Widget> _widgets;
        private string path;

        private void Recreate()
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsJsonDatabase()
                .WithDatabaseDirectory(path)
                .Build();

            _widgets = context.Set<Widget>();
        }

        public AsyncTests()
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task Adds_Item_To_List_And_Store_Async()
        {
            Recreate();
            await _widgets.ClearAsync();
            await _widgets.AddAsync(new Widget {SKU = "001", Name = "Test widget 1", Price = 2.00M});

            Recreate();

            var addedItem = _widgets.FirstOrDefault(w => w.SKU == "001");
            Assert.True(addedItem != null && _widgets.Count() == 1);
        }


        [Fact]
        public async Task Adds_Item_To_List_And_Existong_Store_Async()
        {
            Recreate();
            await _widgets.ClearAsync();
            await _widgets.AddAsync(new Widget {SKU = "001", Name = "Test widget 1", Price = 2.00M});

            Recreate();
            await _widgets.AddAsync(new Widget {SKU = "002", Name = "Test widget 2", Price = 4.00M});

            var addedItem = _widgets.FirstOrDefault(w => w.SKU == "001");
            Assert.True(addedItem != null && _widgets.Count() == 2);
        }


        [Fact]
        public async Task Updates_Item_In_List_And_Store_Async()
        {
            Recreate();
            _widgets.Clear();
            var updateMe = new Widget {SKU = "001", Name = "Test widget 1", Price = 2.00M};
            await _widgets.AddAsync(updateMe);

            // Update and Save:
            updateMe.Name = "UPDATED";
            await _widgets.UpdateAsync(updateMe);

            Recreate();
            // Grab from the list:
            var updatedInList = _widgets.FirstOrDefault(w => w.Name == "UPDATED");

            // Make sure updated in both:
            Assert.True(updatedInList.Name == "UPDATED");
        }


        [Fact]
        public async Task Removes_Item_From_List_And_Store_Async()
        {
            Recreate();
            await _widgets.ClearAsync();
            await _widgets.AddAsync(new Widget {SKU = "001", Name = "Test widget 1", Price = 2.00M});

            Recreate();

            var removeMe = _widgets.FirstOrDefault();
            await _widgets.RemoveAsync(removeMe);

            Recreate();

            Assert.True(!_widgets.Any());
        }


        [Fact]
        public async Task Adds_Batch_Of_Items_To_Store_Async()
        {
            Recreate();
            await _widgets.ClearAsync();

            int INSERT_QTY = 100;
            var batch = new List<Widget>();
            for (int i = 0; i < INSERT_QTY; i++)
            {
                batch.Add(new Widget
                {
                    SKU = string.Format("00{0}", i),
                    Name = string.Format("Test widget {0}", i),
                    Price = 2.00M
                });
            }
            await _widgets.AddAsync(batch);

            Recreate();
            Assert.True(_widgets.Count() == INSERT_QTY);
        }

        [Fact]
        public async Task Removes_Range_From_Store_Async()
        {
            Recreate();
            await _widgets.ClearAsync();

            int INSERT_QTY = 100;
            var batch = new List<Widget>();
            for (int i = 0; i < INSERT_QTY; i++)
            {
                batch.Add(new Widget
                {
                    SKU = string.Format("00{0}", i),
                    Name = string.Format("Test widget {0}", i),
                    Price = 2.00M + i
                });
            }
            await _widgets.AddAsync(batch);
            Recreate();

            // Grab a range of items to remove:
            var itemsToRemove = _widgets.AsQueryable().Where(w => w.Price > 5 && w.Price <= 20);
            int removedQty = itemsToRemove.Count();

            await _widgets.RemoveAsync(itemsToRemove.ToList());

            Recreate();
            Assert.True(removedQty > 0 && removedQty < INSERT_QTY && _widgets.Count() == (INSERT_QTY - removedQty));
        }
    }
}
