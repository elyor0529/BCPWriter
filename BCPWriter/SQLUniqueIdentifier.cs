﻿namespace BCPWriter
{
    using System;
    using System.IO;

    /// <summary>
    /// SQL uniqueidentifier.
    /// </summary>
    /// 
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/ms187942.aspx">uniqueidentifier (Transact-SQL)</a><br/>
    /// <br/>
    /// From SQL Server 2008 Books Online:<br/>
    /// <br/>
    /// A column or local variable of uniqueidentifier data type can be initialized to a value in the following ways:<br/>
    /// * By using the NEWID function.<br/>
    /// * By converting from a string constant in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx,
    ///   in which each x is a hexadecimal digit in the range 0-9 or a-f.
    ///   For example, 6F9619FF-8B86-D011-B42D-00C04FC964FF is a valid uniqueidentifier value.<br/>
    /// <br/>
    /// .Net provides a class named Guid (Globally Unique Identifier), see http://en.wikipedia.org/wiki/Globally_Unique_Identifier
    /// </remarks>
    public class SQLUniqueIdentifier : IBCPSerialization
    {
        public void Write(BinaryWriter writer, object value)
        {
            Write(writer, (Guid?)value);
        }

        public static void Write(BinaryWriter writer, Guid? guid)
        {
            if (!guid.HasValue)
            {
                // 1 byte long
                byte[] nullBytes = { 255 };
                writer.Write(nullBytes);
                return;
            }

            // This can never happen since Guid will throw an exception before
            /*if (string.IsNullOrEmpty(guid.Value.ToString()))
            {
                throw new ArgumentNullException("Empty guid");
            }*/

            // byte is 1 byte long :)
            // Guid is always of length 16
            const byte size = 16;
            writer.Write(size);

            // int is 4 bytes long
            writer.Write(guid.Value.ToByteArray());
        }
    }
}
