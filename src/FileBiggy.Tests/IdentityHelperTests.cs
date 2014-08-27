using FileBiggy.Attributes;
using FileBiggy.Common;
using FileBiggy.Exceptions;
using Xunit;

namespace FileBiggy.Tests
{
    public class IdentityHelperTests
    {
        private class OneIdentityColumn
        {
            [Identity]
            public int Id { get; set; }

            public string SomeData { get; set; }

            public string SomeOtherData { get; set; }
        }

        private class TwoIdentityColumn
        {
            [Identity]
            public int Id { get; set; }

            [Identity]
            public string SomeData { get; set; }

            public string SomeOtherData { get; set; }
        }

        private class NoIdentityColumn
        {
            public int Id { get; set; }

            public string SomeData { get; set; }

            public string SomeOtherData { get; set; }
        }

        [Fact]
        public void Read_No_Identity_Property_Value()
        {
            var obj = new NoIdentityColumn
            {
                Id = 37,
                SomeData = "somedata",
                SomeOtherData = "someotherdata"
            };

            var identity = obj.GetKeyFromEntity();

            Assert.Null(identity);
        }

        [Fact]
        public void Read_Identity_Property_Value()
        {
            var obj = new OneIdentityColumn
            {
                Id = 37,
                SomeData = "somedata",
                SomeOtherData = "someotherdata"
            };

            var identity = obj.GetKeyFromEntity();

            Assert.IsType(typeof (int), identity);

            var value = (int) identity;
            Assert.Equal(value, 37);
        }

        [Fact]
        public void Read_Two_Identity_Property_Value()
        {
            var obj = new TwoIdentityColumn
            {
                Id = 37,
                SomeData = "somedata",
                SomeOtherData = "someotherdata"
            };

            Assert.Throws<IdentityAttributeMismatchException>(() => obj.GetKeyFromEntity());
        }
    }
}