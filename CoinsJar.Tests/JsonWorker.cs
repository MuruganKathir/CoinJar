//-----------------------------------------------------------------------
// <copyright file="JsonWorker.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The JSON Worker.
    /// </summary>
    internal class JsonWorker
    {
        /// <summary>
        /// The JSON settings
        /// </summary>
        public static readonly JsonSerializerSettings Settings = null;

        /// <summary>
        /// Gets the JSON settings.
        /// </summary>
        /// <value>
        /// The JSON settings.
        /// </value>
        public static JsonSerializerSettings JsonSettings
        {
            get
            {
                if (Settings == null)
                {
                    return new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };
                }

                return Settings;
            }
        }
    }
}
