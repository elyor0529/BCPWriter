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
    class SQLDateTests
    {
        private void WriteDate(DateTime date, string myFileName)
        {
            BinaryWriter writer = BCPTests.CreateBinaryFile(myFileName);

            SQLDate sqlDate = new SQLDate();
            writer.Write(sqlDate.ToBCP(date));

            writer.Close();
        }

        [Test]
        public void TestDate()
        {
            DateTime date = DateTime.Parse(
                                    "1753-01-01",
                                    System.Globalization.CultureInfo.InvariantCulture
                                );

            string myFileName = "date.bcp";
            WriteDate(date, myFileName);
            BCPTests.CheckFile(myFileName);
        }
    }
}