//-----------------------------------------------------------------------
// <copyright file="WebApiAdapterTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------
namespace CoinsJar.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CoinsJar.Tests.TestData;
    using CoinsJar.WebApi.Adapters;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Web API State Adapter Tests
    /// </summary>
    [TestClass]
    public class WebApiAdapterTests
    {
        /// <summary>
        /// The test context instance
        /// </summary>3
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        #region Additional test attributes
        /// <summary>
        /// Initializes the tests.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            TestDataAdapter.ResetData();
        }

        #endregion
        
        /// <summary>
        /// Gets the current about box comment test.
        /// </summary>
        [TestMethod, Ignore]
        public void GetCurrentAboutBoxCommentTest()
        {
            var adapter = new WebApiStateAdapter();

            var currentComment = adapter.GetCurrentAboutBoxComment();
            
            Assert.AreEqual("No current comment found", currentComment);
        }

        /// <summary>
        /// Updates the current about box comment test.
        /// </summary>
        [TestMethod]
        public void UpdateCurrentAboutBoxCommentTest()
        {
            var adapter = new WebApiStateAdapter();

            var newComment = string.Format("New Comment: {0}", DateTime.Now.ToLongTimeString());

            var outcome = adapter.UpdateCurrentAboutBoxComment(newComment);

            Assert.IsTrue(outcome.IsSuccessful);

            var verifiedComment = adapter.GetCurrentAboutBoxComment();
            Assert.AreEqual(newComment, verifiedComment);
        }

        /// <summary>
        /// Updates the current about box comment multiple updates test.
        /// </summary>
        [TestMethod]
        public void UpdateCurrentAboutBoxCommentMultipleUpdatesTest()
        {
            var adapter = new WebApiStateAdapter();

            var firstNewComment = string.Format("New Comment: {0}", DateTime.Now.ToLongTimeString());

            var firstOutcome = adapter.UpdateCurrentAboutBoxComment(firstNewComment);

            Assert.IsTrue(firstOutcome.IsSuccessful);

            var secondNewComment = string.Format("Second New Comment: {0}", DateTime.Now.ToLongTimeString());
            var secondOutcome = adapter.UpdateCurrentAboutBoxComment(secondNewComment);

            Assert.IsTrue(secondOutcome.IsSuccessful);

            var verifiedComment = adapter.GetCurrentAboutBoxComment();
            Assert.AreEqual(secondNewComment, verifiedComment);
        }
    }
}
