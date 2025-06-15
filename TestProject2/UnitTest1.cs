using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using WIS.Pages;

namespace TestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Autorisation_Creation_DoesNotThrow()
        {
            var page = new AutorisationPage();
            Assert.IsNotNull(page);
        }
    }
}