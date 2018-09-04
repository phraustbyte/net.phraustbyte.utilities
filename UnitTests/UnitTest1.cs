using SQLStoredProcedureGenerator;
using System;
using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string result = SQLStoredProcedureGenerator.Generator.ObjectToCreate(typeof(TestObject));
            Assert.NotNull(result);
        }
    }
    [Table("TestObject")]
    public class TestObject : IBaseObject
    {
        [UniqueIdentifer]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Changer { get; set; }
        [ActiveIdentifier]
        public bool Active { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        [Ignore]
        public TimeSpan Ignore { get; set; }
    }
    public interface IBaseObject
    {
        int Id { get; set; }
        DateTime CreatedDate { get; set; }
        String Changer { get; set; }
        bool Active { get; set; }
    }
}
