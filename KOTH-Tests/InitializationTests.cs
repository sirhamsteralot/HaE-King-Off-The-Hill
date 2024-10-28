using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace KOTH_Tests
{
    [TestClass]
    public class InitializationTests
    {
        [TestMethod]
        public void Should_Equal_HelloWorld()
        {
            Assert.AreEqual("Hello World!", "Hello World!");
        }
    }
}
