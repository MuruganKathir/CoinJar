//-----------------------------------------------------------------------
// <copyright file="RegexTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Regular expression tests.
    /// </summary>
    [TestClass]
    public class RegexTests
    {
        /// <summary>
        /// Values the in braces.
        /// </summary>
        [TestMethod]
        public void ValueInBraces()
        {
            var inputString = "Values are {FooValue} and {BarValue} but not {OmValue}.";
            var model = new { FooValue = "Happy", BarValue = "Sad" };
            var workingString = this.ReplaceTemplateWithValues(inputString, model);
            string finalString = workingString;
            Assert.AreEqual("Values are Happy and Sad but not Value could not be accessed.", finalString);
        }

        /// <summary>
        /// Resolves the property value.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="valueAccessor">The value accessor.</param>
        /// <returns>
        /// The object type
        /// </returns>
        public object ResolvePropertyValue(object model, string valueAccessor)
        {
            var valueAccessorParts = valueAccessor.Split('.');

            if (model == null)
            {
                return "Value could not be accessed";
            }

            object containingObject = model;
            foreach (var valueAccessorPart in valueAccessorParts)
            {
                var propertyAccessor = containingObject.GetType().GetProperty(valueAccessorPart);
                if (propertyAccessor == null)
                {
                    containingObject = "Value could not be accessed";
                    break;
                }
                else
                {
                    containingObject = propertyAccessor.GetValue(containingObject);
                }
            }

            object resolvedValue = containingObject;
            return resolvedValue;
        }

        /// <summary>
        /// Replaces the template with values.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="model">The model.</param>
        /// <returns>The string type.</returns>
        private string ReplaceTemplateWithValues(string inputString, object model)
        {
            var rex = new Regex(@"\{([^}]+)}");

            var match = rex.Match(inputString);

            var replacements = new Dictionary<string, object>();

            while (match.Success)
            {
                string prop = match.Groups[0].Value;

                var propValue = this.ResolvePropertyValue(model, prop.Substring(1, prop.Length - 2));

                replacements.Add(prop, propValue);

                match = match.NextMatch();
            }

            var workingString = inputString;
            foreach (var replacement in replacements)
            {
                workingString = workingString.Replace(replacement.Key, replacement.Value.ToString());
            }

            return workingString;
        }
    }
}
