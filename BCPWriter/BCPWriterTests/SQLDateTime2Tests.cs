﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

using BCPWriter;

namespace BCPWriter.Tests
{
    [TestFixture]
    class SQLDateTime2Tests
    {
        private void WriteDateTime2(DateTime? dateTime, string myFileName)
        {
            BinaryWriter writer = BCPTests.CreateBinaryFile(myFileName);

            SQLDateTime2 sql = new SQLDateTime2();
            sql.Write(writer, dateTime);

            writer.Close();
        }

        [Test]
        public void TestDateTime2Seconds()
        {
            DateTime dateTime = DateTime.Parse(
                                    "2004-05-23T14:25:10",
                                    System.Globalization.CultureInfo.InvariantCulture
                                );

            string myFileName = "datetime2_seconds.bcp";
            WriteDateTime2(dateTime, myFileName);
            BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestDateTime2Milliseconds()
        {
            DateTime dateTime = DateTime.Parse(
                                    "2004-05-23T14:25:10.123456789",
                                    System.Globalization.CultureInfo.InvariantCulture
                                );

            string myFileName = "datetime2_milliseconds.bcp";
            WriteDateTime2(dateTime, myFileName);
            BCPTests.CheckFile(myFileName);
        }

        [Test]
        public void TestDateTime2Null()
        {
            string myFileName = "datetime2_null.bcp";
            WriteDateTime2(null, myFileName);
            BCPTests.CheckFile(myFileName);
        }
    }
}