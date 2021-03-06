﻿namespace BCPWriter.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    /// <summary>
    /// Tests for SQLTime.
    /// </summary>
    /// <see cref="SQLTime"/>
    [TestFixture]
    internal class SQLTimeTests
    {
        private static void WriteTime(DateTime? time, string myFileName)
        {
            BinaryWriter writer = BCPTests.CreateBinaryFile(myFileName);

             SQLTime.Write(writer, time);

            writer.Close();
        }

        [Test]
        public void TestTime()
        {
            DateTime time = DateTime.Parse(
                                    "12:35:29.1234567",
                                    System.Globalization.CultureInfo.InvariantCulture);

            const string myFileName = "time.bcp";
            WriteTime(time, myFileName);
            BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestTimeNull()
        {
            const string myFileName = "time_null.bcp";
            WriteTime(null, myFileName);
            BCPTests.CheckFile(myFileName);
        }
    }
}
