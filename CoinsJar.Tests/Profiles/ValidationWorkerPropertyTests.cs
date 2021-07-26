//-----------------------------------------------------------------------
// <copyright file="ValidationWorkerPropertyTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.Profiles
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WebApi.Adapters.Profiles;

    /// <summary>
    /// The validation worker test class.
    /// </summary>
    [TestClass]
    public class ValidationWorkerPropertyTests
    {
        /// <summary>
        /// Property validate string required pass test.
        /// </summary>
        [TestMethod]
        public void PropertyValidateStringRequiredPassTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                StringValue = "ABC"
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.StringValue);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Property validate string required fail test.
        /// </summary>
        [TestMethod]
        public void PropertyValidateStringRequiredFailTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                StringValue = null
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.StringValue);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Property validate decimal required pass test.
        /// </summary>
        [TestMethod]
        public void PropertyValidateDecimalRequiredPassTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                DecimalValue = 200m
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.DecimalValue);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Property validate decimal required fail.
        /// </summary>
        [TestMethod]
        public void PropertyValidateDecimalRequiredFailTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                DecimalValue = null
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.DecimalValue);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Properties the validate date required pass.
        /// </summary>
        [TestMethod]
        public void PropertyValidateDateRequiredPassTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                DateValue = new DateTime()
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.DateValue);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Properties the validate date required fail.
        /// </summary>
        [TestMethod]
        public void PropertyValidateDateRequiredFailTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                DecimalValue = null
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.DateValue);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Properties the validate unique identifier required pass.
        /// </summary>
        [TestMethod]
        public void PropertyValidateGuidRequiredPassTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                GuidValue = Guid.NewGuid()
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.GuidValue);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Properties the validate unique identifier required fail.
        /// </summary>
        [TestMethod]
        public void PropertyValidateGuidRequiredFailTest()
        {
            var worker = new ValidationWorker();
            var target = new TargetExample
            {
                DecimalValue = null
            };

            var outcome = worker.ValidPopulatedProperty(target, p => p.GuidValue);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Updates the property if not same fail test.
        /// </summary>
        [TestMethod]
        public void UpdatePropertyIfNotSameStringFailTest()
        {
            var worker = new ValidationWorker();
            var proposedValue = "ABC";
            var target = new TargetExample
            {
                StringValue = proposedValue
            };

            var outcome = worker.UpdatePropertyIfNotSame(target, p => p.StringValue, proposedValue);

            Assert.IsFalse(outcome);
            Assert.AreEqual(target.StringValue, "ABC");
        }

        /// <summary>
        /// Updates the property if not same string pass test.
        /// </summary>
        [TestMethod]
        public void UpdatePropertyIfNotSameStringPassTest()
        {
            var worker = new ValidationWorker();
            var proposedValue = "ABC";
            var target = new TargetExample
            {
                StringValue = "EFG"
            };

            var outcome = worker.UpdatePropertyIfNotSame(target, p => p.StringValue, proposedValue);

            Assert.IsTrue(outcome);
            Assert.AreEqual(target.StringValue, proposedValue);
        }

        /// <summary>
        /// Updates the property if not same target string null pass test.
        /// </summary>
        [TestMethod]
        public void UpdatePropertyIfNotSameTargetStringNullPassTest()
        {
            var worker = new ValidationWorker();
            var proposedValue = "ABC";
            var target = new TargetExample
            {
                StringValue = null
            };

            var outcome = worker.UpdatePropertyIfNotSame(target, p => p.StringValue, proposedValue);

            Assert.IsTrue(outcome);
            Assert.AreEqual(target.StringValue, proposedValue);
        }

        /// <summary>
        /// Updates the property if not same decimal fail test.
        /// </summary>
        [TestMethod]
        public void UpdatePropertyIfNotSameDecimalFailTest()
        {
            var worker = new ValidationWorker();
            var proposedValue = 200m;
            var target = new TargetExample
            {
                DecimalValue = proposedValue
            };

            var outcome = worker.UpdatePropertyIfNotSame(target, p => p.DecimalValue, proposedValue);

            Assert.IsFalse(outcome);
            Assert.AreEqual(target.DecimalValue, proposedValue);
        }

        /// <summary>
        /// Updates the property if not same string pass test.
        /// </summary>
        [TestMethod]
        public void UpdatePropertyIfNotSameDecimalPassTest()
        {
            var worker = new ValidationWorker();
            var proposedValue = 200m;
            var target = new TargetExample
            {
                DecimalValue = 0.0m
            };

            var outcome = worker.UpdatePropertyIfNotSame(target, p => p.DecimalValue, proposedValue);

            Assert.IsTrue(outcome);
            Assert.AreEqual(target.DecimalValue, proposedValue);
        }
        
        /// <summary>
        /// Target Example.
        /// </summary>
        public class TargetExample
        {
            /// <summary>
            /// Gets or sets the string value.
            /// </summary>
            /// <value>
            /// The string value.
            /// </value>
            public string StringValue { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier value.
            /// </summary>
            /// <value>
            /// The unique identifier value.
            /// </value>
            public Guid? GuidValue { get; set; }

            /// <summary>
            /// Gets or sets the decimal value.
            /// </summary>
            /// <value>
            /// The decimal value.
            /// </value>
            public decimal? DecimalValue { get; set; }

            /// <summary>
            /// Gets or sets the date value.
            /// </summary>
            /// <value>
            /// The date value.
            /// </value>
            public DateTime? DateValue { get; set; }
        }
    }
}
