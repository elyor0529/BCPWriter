﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BCPWriter
{
    /// <summary>
    /// SQL float.
    /// </summary>
    /// 
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/ms173773.aspx">float and real (Transact-SQL)</a><br/>
    /// <br/>
    /// From SQL Server 2008 Books Online:<br/>
    /// <br/>
    /// Data type      | Range 	                                                     | Storage<br/>
    /// float          | - 1.79E+308 to -2.23E-308, 0 and 2.23E-308 to 1.79E+308     | Depends on the value of n<br/>
    /// float n: 1-24  | Precision: 7 digits                                         | 4 bytes
    /// float n: 25-53 | Precision: 15 digits                                        | 8 bytes
    /// real           | - 3.40E + 38 to -1.18E - 38, 0 and 1.18E - 38 to 3.40E + 38 | 4 Bytes<br/>
    /// </remarks>
    public class SQLFloat : IBCPSerialization
    {
        private uint _nbBits;

        /// <summary>
        /// Creates a SQL float(n).
        /// </summary>
        /// <remarks>
        /// SQL Server treats n as one of two possible values.<br/>
        /// If 1<=n<=24, n is treated as 24. If 25<=n<=53, n is treated as 53.
        /// </remarks>
        /// <param name="nbBits">
        /// n is the number of bits that are used to store the mantissa of the float number
        /// in scientific notation and, therefore, dictates the precision and storage size
        /// </param>
        public SQLFloat(uint nbBits)
        {
            if (nbBits < 1 || nbBits > 53)
            {
                throw new ArgumentException("nbBits should be between 1-24 and 25-53");
            }

            _nbBits = nbBits;
        }

        /// <summary>
        /// Creates a float with the default value of n (53).
        /// </summary>
        public SQLFloat()
            : this(53)
        {
        }

        public uint NbBits
        {
            get { return _nbBits; }
        }

        public void Write(BinaryWriter writer, object value)
        {
            if (value is float)
            {
                Write(writer, (float)value);
            }
            else
            {
                Write(writer, (double)value);
            }
        }

        public void Write(BinaryWriter writer, double? value)
        {
            if (_nbBits < 25)
            {
                throw new ArgumentException("nbBits should be between 25-53");
            }

            //byte is 1 byte long :)
            byte size = 4 * 2;
            writer.Write(size);

            //double is 8 bytes long
            writer.Write(value.Value);
        }

        public void Write(BinaryWriter writer, float? value)
        {
            if (_nbBits > 24)
            {
                throw new ArgumentException("nbBits should be between 1-24");
            }

            //byte is 1 byte long :)
            byte size = 4;
            writer.Write(size);

            //float is 4 bytes long
            writer.Write(value.Value);
        }
    }
}
