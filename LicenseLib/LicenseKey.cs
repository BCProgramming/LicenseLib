using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace BASeCamp.Licensing
{
    /// <summary>
    /// Same as a "ProductKey" class I wrote a few years ago. Difference is that it is not locked to particular values (edition, feature, etc.) and can effectively
    /// hold any arrangement of data, based on the implementation of a separate class.
    /// </summary>
    public class LicenseHandler
    {
        public static T FromProductCode<T>(String pProductCode, String IDString) where T : LicenseKeyData
        {
            // remove any hyphens/ dashes, first.

            pProductCode = pProductCode.Replace("-", "");
            //convert the string to a byte array via zBase32:

            ZBase32Encoder zb = new ZBase32Encoder();
            byte[] acquiredcode = zb.Decode(pProductCode);


            //byte[] acquiredcode = Enumerable.Range(0, fromproductcode.Length)
            //         .Where(x => x % 2 == 0)
            //         .Select(x => Convert.ToByte(fromproductcode.Substring(x, 2), 16))
            //         .ToArray();


            //now, we need to decrypt it...
            byte[] decrypted = CryptHelper.Decrypt(acquiredcode, IDString);

            //armed with the decrypted data, toss it into a memory stream
            MemoryStream ms = new MemoryStream(decrypted);
            ms.Seek(0, SeekOrigin.Begin);

            //now, invoke FromStream...
            return FromDecryptedStream<T>(ms);

        }

        public static T FromDecryptedStream<T>(Stream Source) where T : LicenseKeyData
        {
            if (!Source.CanRead) throw new ArgumentException("Stream must be readable.", "readFrom");
            try
            {
                ConstructorInfo useConstructor = typeof (T).GetConstructor(new Type[] {typeof (Stream)});
                if (useConstructor == null)
                {
                    throw new ArgumentException("Type " + typeof (T).Name + " does not have a constructor accepting arguments of type (Stream)");
                }
                Object BuiltInstance = useConstructor.Invoke(new object[] {Source});
                return (T) BuiltInstance;
            }
            catch (Exception Exx)
            {
                //occurs when we try to cast a -1 to a byte
                throw new InvalidKeyException("Invalid Product Key", Exx);
            }
        }

        public static String ToProductCode<T>(T LicenseObject, String IDString) where T : LicenseKeyData
        {
            MemoryStream mstream = new MemoryStream();

            //first, write our data out to a memorystream.
            LicenseObject.PersistData(mstream);


            //seek to the start, read as a string.
            mstream.Seek(0, SeekOrigin.Begin);
            //read it back, as a array of bytes.
            Byte[] readdata = new byte[mstream.Length];
            mstream.Read(readdata, 0, readdata.Length);

            //Encrypt the array of bytes using the MachineID. this is set in the constructor to getLocalMachineID(), but can of course
            //be changed by the caller before calling GetProductCode (for example, for generating the key elsewhere)
            Byte[] encrypted = CryptHelper.Encrypt(readdata, IDString);

            //now we need a readable form, so encode using zBase32, which has 
            //good results for a human readable key.
            ZBase32Encoder zb = new ZBase32Encoder();


            return zb.Encode(encrypted).ToUpper();
        }
    }

    public class InvalidKeyException : Exception
    {
        public InvalidKeyException()
            : base("Invalid Key")
        {
        }

        public InvalidKeyException(String message)
            : base(message)
        {
        }

        public InvalidKeyException(String message, Exception innerexception)
            : base(message, innerexception)
        {
        }
    }

    public abstract class LicenseKeyData
    {
        protected LicenseKeyData(Stream SourceData)
        {
        }

        protected LicenseKeyData()
        {
        }

        public abstract void PersistData(Stream Target);
    }

}