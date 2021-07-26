// <copyright file="PerformanceCounterTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------
namespace CoinsJar.Tests
{
    using CoinsJar.WebApi.Adapters.PerformanceCounters;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The Performance Counter Tests
    /// </summary>
    [TestClass]
    public class PerformanceCounterTests
    {
        /// <summary>
        /// Logs the request test.
        /// </summary>
        [TestMethod]
        public void LogRequestTest()
        {
            var v = new PerformanceCountersAdapterComponent();

            for (int i = 0; i <= 10000; i++)
            {
                v.LogRequest();
            }
        }
    }
}
