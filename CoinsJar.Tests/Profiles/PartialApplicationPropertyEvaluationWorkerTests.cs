//-----------------------------------------------------------------------
// <copyright file="PartialApplicationPropertyEvaluationWorkerTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.Profiles
{
    using System;
    using CoinsJar.WebApi.Adapters.Profiles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Partial Application Property Evaluation Worker Tests
    /// </summary>
    [TestClass]
    public class PartialApplicationPropertyEvaluationWorkerTests
    {
        /// <summary>
        /// Properties the evaluation string update value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationStringUpdateValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                StringValue = "ABC"
            };
            worker.EvaluateAndUpdateChange(target, t => t.StringValue, "NewValue");

            Assert.AreEqual("NewValue", target.StringValue);
        }

        /// <summary>
        /// Properties the evaluation string null value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationStringNullValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                StringValue = "ABC"
            };
            worker.EvaluateAndUpdateChange(target, t => t.StringValue, null);

            Assert.AreEqual("ABC", target.StringValue);
        }

        /// <summary>
        /// Properties the evaluation string empty value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationStringEmptyValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                StringValue = "ABC"
            };
            worker.EvaluateAndUpdateChange(target, t => t.StringValue, string.Empty);

            Assert.IsNull(target.StringValue);
        }

        /// <summary>
        /// Properties the evaluation string same value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationStringSameValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                StringValue = "ABC"
            };
            var outcome = worker.EvaluateAndUpdateChange(target, t => t.StringValue, "ABC");

            Assert.IsFalse(outcome);
            Assert.AreEqual("ABC", target.StringValue);
        }

        /// <summary>
        /// Properties the evaluation decimal update value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDecimalUpdateValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                DecimalValue = 123.45M
            };
            worker.EvaluateAndUpdateChange(target, t => t.DecimalValue, 234.56M);

            Assert.AreEqual(234.56M, target.DecimalValue);
        }

        /// <summary>
        /// Properties the evaluation decimal null value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDecimalNullValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                DecimalValue = 123.45M
            };
            worker.EvaluateAndUpdateChange(target, t => t.DecimalValue, null);

            Assert.AreEqual(123.45M, target.DecimalValue);
        }

        /// <summary>
        /// Properties the evaluation decimal same value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDecimalSameValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                DecimalValue = 123.45M
            };
            var outcome = worker.EvaluateAndUpdateChange(target, t => t.DecimalValue, 123.45M);

            Assert.IsFalse(outcome);
            Assert.AreEqual(123.45M, target.DecimalValue);
        }

        /// <summary>
        /// Properties the evaluation date update value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDateUpdateValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                DateValue = DateTime.Now
            };

            DateTime? newDateValue = DateTime.Now.AddHours(1);

            worker.EvaluateAndUpdateChange(target, t => t.DateValue, newDateValue);

            Assert.AreEqual(newDateValue, target.DateValue);
        }

        /// <summary>
        /// Properties the evaluation date null value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDateNullValue()
        {
            var worker = new PropertyEvaluationWorker();
            var dateValue = DateTime.Now;
            var target = new TargetExample
            {
                DateValue = dateValue
            };

            worker.EvaluateAndUpdateChange(target, t => t.DateValue, null);

            Assert.AreEqual(dateValue, target.DateValue);
        }

        /// <summary>
        /// Properties the evaluation date same value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationDateSameValue()
        {
            var worker = new PropertyEvaluationWorker();
            var dateValue = DateTime.Now;
            var target = new TargetExample
            {
                DateValue = dateValue
            };

            var outcome = worker.EvaluateAndUpdateChange(target, t => t.DateValue, dateValue);

            Assert.IsFalse(outcome);
            Assert.AreEqual(dateValue, target.DateValue);
        }

        /// <summary>
        /// Properties the evaluation unique identifier update value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationGuidUpdateValue()
        {
            var worker = new PropertyEvaluationWorker();
            var target = new TargetExample
            {
                GuidValue = Guid.NewGuid()
            };

            Guid? newValue = Guid.NewGuid();

            worker.EvaluateAndUpdateChange(target, t => t.GuidValue, newValue);

            Assert.AreEqual(newValue, target.GuidValue);
        }

        /// <summary>
        /// Properties the evaluation unique identifier null value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationGuidNullValue()
        {
            var worker = new PropertyEvaluationWorker();
            var value = Guid.NewGuid();
            var target = new TargetExample
            {
                GuidValue = value
            };

            var outcome = worker.EvaluateAndUpdateChange(target, t => t.GuidValue, null);

            Assert.IsFalse(outcome);
            Assert.AreEqual(value, target.GuidValue);
        }

        /// <summary>
        /// Properties the evaluation unique identifier same value.
        /// </summary>
        [TestMethod]
        public void PropertyEvaluationGuidSameValue()
        {
            var worker = new PropertyEvaluationWorker();
            var value = Guid.NewGuid();
            var target = new TargetExample
            {
                GuidValue = value
            };

            var outcome = worker.EvaluateAndUpdateChange(target, t => t.GuidValue, value);

            Assert.IsFalse(outcome);
            Assert.AreEqual(value, target.GuidValue);
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
