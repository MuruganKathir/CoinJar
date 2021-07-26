//-----------------------------------------------------------------------
// <copyright file="TestDataAdapter.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.TestData
{
    /// <summary>
    /// Test Data Adapter.
    /// </summary>
    internal static class TestDataAdapter
    {
        /// <summary>
        /// Resets the data.
        /// </summary>
        public static void ResetData()
        {
            var context = new CoinsJarWebStateTestDataManagementEntities();
            context.ResetTestData();
        }
    }
}
