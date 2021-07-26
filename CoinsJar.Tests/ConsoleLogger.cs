//-----------------------------------------------------------------------
// <copyright file="ConsoleLogger.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests
{
    using System;

    /// <summary>
    /// The console logger.
    /// </summary>
    public class ConsoleLogger : ILoggingConcern
    {
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <param name="level">The level.</param>
        public void Log(string message, string category, System.Diagnostics.EventLogEntryType level)
        {
            var dateTimeStamp = DateTime.Now;
            Console.WriteLine(string.Format(
                "{0:dd MMM yyyy HH:mm:ss}: {1}({2}) - {3}",
                dateTimeStamp,
                category,
                level,
                message));
        }
    }
}
