using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Tests.Services
{
    [TestFixture]
    public class FakeMethodTest
    {


        [Test]
        public void FakeTestMethod()
        {
            // Arrange
            FakeApiService service = new FakeApiService();

            // Act
            double val = 7.0;
            // Assert
            Assert.AreEqual(service.FakeMethodTest(), val);
        }
    }
}
