using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBiggy.Attributes;
using FileBiggy.Contracts;
using FileBiggy.Factory;
using Xunit;

namespace FileBiggy.Tests.ExistingTests
{
    [Trait("Biggy List with File Db", "")]
    public class BiggyListWithBsonDb
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

        private Guid g1 = Guid.NewGuid();
        private Guid g2 = Guid.NewGuid();
        private Guid g3 = Guid.NewGuid();

        private void Recreate()
        {
            var context = ContextFactory.Create<EntityContext>()
                .AsBsonDatabase()
                .WithDatabaseDirectory(path)
                .Build();

            _widgets = context.Set<Widget>();
        }

        public BiggyListWithBsonDb()
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data", Guid.NewGuid().ToString());
        }

        [Fact(DisplayName = "Adds an item to List and Store")]
        public void Adds_Item_To_List_And_Store()
        {
            Recreate();
            _widgets.Clear();
            _widgets.Add(new Widget {SKU = g1, Name = "Test widget 1", Price = 2.00M});

            Recreate();

            var addedItem = _widgets.FirstOrDefault(w => w.SKU == g1);
            Assert.True(addedItem != null && _widgets.Count() == 1);
        }


        [Fact(DisplayName = "Adds an item to List and Existing Store")]
        public void Adds_Item_To_List_And_Existong_Store()
        {
            Recreate();
            _widgets.Clear();
            _widgets.Add(new Widget {SKU = g1, Name = "Test widget 1", Price = 2.00M});

            Recreate();
            _widgets.Add(new Widget {SKU = g2, Name = "Test widget 2", Price = 4.00M});

            var addedItem = _widgets.FirstOrDefault(w => w.SKU == g1);
            Assert.True(addedItem != null && _widgets.Count() == 2);
        }


        [Fact(DisplayName = "Updates an item in List and Store")]
        public void Updates_Item_In_List_And_Store()
        {
            Recreate();
            _widgets.Clear();
            var updateMe = new Widget {SKU = g1, Name = "Test widget 1", Price = 2.00M};
            _widgets.Add(updateMe);

            // Update and Save:
            updateMe.Name = "UPDATED";
            _widgets.Update(updateMe);

            Recreate();
            // Grab from the list:
            var updatedInList = _widgets.FirstOrDefault(w => w.Name == "UPDATED");

            // Make sure updated in both:
            Assert.True(updatedInList.Name == "UPDATED");
        }


        [Fact(DisplayName = "Removes an item from the List and Store")]
        public void Removes_Item_From_List_And_Store()
        {
            Recreate();
            _widgets.Clear();
            _widgets.Add(new Widget {SKU = g1, Name = "Test widget 1", Price = 2.00M});

            Recreate();

            var removeMe = _widgets.FirstOrDefault();
            _widgets.Remove(removeMe);

            Recreate();

            Assert.True(!_widgets.Any());
        }


        [Fact(DisplayName = "Adds batch of items to List")]
        public void Adds_Batch_Of_Items_To_Store()
        {
            Recreate();
            _widgets.Clear();

            int INSERT_QTY = 100;
            var batch = new List<Widget>();
            for (int i = 0; i < INSERT_QTY; i++)
            {
                batch.Add(new Widget {SKU = Guid.NewGuid(), Name = string.Format("Test widget {0}", i), Price = 2.00M});
            }
            _widgets.Add(batch);

            Recreate();
            Assert.True(_widgets.Count() == INSERT_QTY);
        }

        [Fact(DisplayName = "Removes Range of items from Store")]
        public void Removes_Range_From_Store()
        {
            Recreate();
            _widgets.Clear();

            int INSERT_QTY = 100;
            var batch = new List<Widget>();
            for (int i = 0; i < INSERT_QTY; i++)
            {
                batch.Add(new Widget
                {
                    SKU = Guid.NewGuid(),
                    Name = string.Format("Test widget {0}", i),
                    Price = 2.00M + i
                });
            }
            _widgets.Add(batch);
            Recreate();

            // Grab a range of items to remove:
            var itemsToRemove = _widgets.AsQueryable().Where(w => w.Price > 5 && w.Price <= 20);
            int removedQty = itemsToRemove.Count();

            _widgets.Remove(itemsToRemove.ToList());

            Recreate();
            Assert.True(removedQty > 0 && removedQty < INSERT_QTY && _widgets.Count() == (INSERT_QTY - removedQty));
        }
    }
}