using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BASeCamp.Licensing
{
    public class CryptHelper
    {
        /// <summary>
        /// Creates and returns a Machine specific identifier for this machine. 
        /// </summary>
        /// <returns></returns>
        public static String LocalMachineID
        {
            get
            {
                String ReadKey = "SOFTWARE\\Microsoft\\Cryptography";
                int keyresult = 0;
                RegistryKey rk = null;
                String mkid = "";
                try
                {
                    rk = Registry.LocalMachine.OpenSubKey(ReadKey);
                }
                catch
                {
                }
                if (rk != null)
                    mkid = (String)rk.GetValue("MachineGuid");


                if (String.IsNullOrEmpty(mkid))
                {
                    mkid = System.Environment.MachineName;
                }
                return mkid;
            }
        }
        /// <summary>
        /// Returns the Windows Product ID of this system.
        /// </summary>
        public static String WindowsProductID
        {
            get
            {
                //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\ CurrentVersion\ProductId
                String ReadKey = @"SOFTWARE\Microsoft\WIndows NT\CurrentVersion";
                int keyresult = 0;
                RegistryKey rk = null;
                String mkid = "";
                try
                {
                    rk = Registry.LocalMachine.OpenSubKey(ReadKey);
                }
                catch
                {
                }
                if (rk != null)
                    mkid = (String)rk.GetValue("ProductId");


                if (String.IsNullOrEmpty(mkid))
                {
                    mkid = System.Environment.MachineName;
                }
                return mkid;
            }
        }

        /// <summary>
        /// Encrypts a byte array using a string password.
        /// </summary>
        /// <param name="clearData">Bytes to Encrypt</param>
        /// <param name="password">Password to be used to encrypt this data. This password will be needed to decrypt the 
        /// data as well.</param>
        /// <returns>An array of bytes corresponding to the result of encrypting the passed array with the given password.</returns>
        public static byte[] Encrypt(byte[] clearData, String password)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(password);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] readresults = new byte[ms.Length];
            ms.Read(readresults, 0, readresults.Length);
            //We need a specific amount of data for the key and Initial Vector of the 
            //encryption algorithm, so we'll use the PasswordDeriveBytes class to 
            //salt the data. 
            //the current salt is hard coded, but it could also be made to use some sort of machine-specific value.
            Rfc2898DeriveBytes pdb = GetPdb(password);

            return Encrypt(clearData, pdb.GetBytes(8), pdb.GetBytes(8));
        }

        /// <summary>
        /// Dencrypts a byte array using a string password.
        /// </summary>
        /// <param name="clearData">Bytes to Decrypt</param>
        /// <param name="password">Password to be used to decrypt this data. This password needs to be the same as the password used during encryption.</param>
        /// <returns>An array of bytes corresponding to the result of encrypting the passed array with the given password.</returns>
        /// <exception cref="CryptographicException">The data is invalid, or the password specified is incorrect.</exception>
        public static byte[] Decrypt(byte[] clearData, String password)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(password);

            ms.Seek(0, SeekOrigin.Begin);
            byte[] readresults = new byte[ms.Length];
            ms.Read(readresults, 0, readresults.Length);
            Rfc2898DeriveBytes pdb = GetPdb(password);
            return Decrypt(clearData, pdb.GetBytes(8), pdb.GetBytes(8));
        }
        /// <summary>
        /// Takes the given string and inserts dashes, creating a product key in groups of 4 or 5 characters.
        /// </summary>
        /// <param name="usevalue"></param>
        /// <returns></returns>
        public static String InsertDashes(String usevalue)
        {
            bool flip5 = false;
            int countfrom = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < usevalue.Length; i++)
            {
                countfrom++;
                sb.Append(usevalue[i]);
                if ((countfrom%(flip5 ? 5 : 4)) == 0)
                {
                    sb.Append("-");
                    flip5 = !flip5;
                    countfrom = 0;
                }
            }

            return sb.ToString();
        }

        public static Rfc2898DeriveBytes GetPdb(string password)
        {
            String sWindowsID = WindowsProductID.Replace("-", "");
            byte[] useSalt = Enumerable.Range(0, sWindowsID.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(sWindowsID.Substring(x, 2), 16))
                .ToArray();
            return new Rfc2898DeriveBytes
                (password,
                    useSalt);
        }

        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();
            RC2 alg = RC2.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream
                (ms,
                    alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }
        // Decrypt a byte array into a byte array using a key and an IV 
        public static byte[] Decrypt(byte[] cipherData,
            byte[] Key, byte[] IV)
        {
            // Create a MemoryStream that is going to accept the
            // decrypted bytes 
            MemoryStream ms = new MemoryStream();
            RC2 alg = RC2.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream
                (ms,
                    alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }
    }
    /// <summary>
    /// basic class to store Date without Time Information.
    /// </summary>
    public class DateOnly
    {
        public DateOnly(byte pDay, byte pMonth, UInt16 pYear)
        {
            Day = pDay;
            Month = pMonth;
            Year = pYear;
        }
        public DateOnly(DateTime SourceDate)
        {
            Year = (ushort) SourceDate.Year;
            Month = (byte) SourceDate.Month;
            Day = (byte) SourceDate.Day;
        }
        public UInt16 Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        public int ToInt()
        {
            return Year ^ Month | Day;
        }
        public override string ToString()
        {
            return ((DateTime) this).ToString();
        }
        public static implicit operator DateTime(DateOnly dateobj)
        {
            return new DateTime(dateobj.Year, dateobj.Month, dateobj.Day);
        }
        public static implicit operator DateOnly(DateTime dateobj)
        {
            return new DateOnly((byte) dateobj.Day, (byte) dateobj.Month, (UInt16) dateobj.Year);
        }
    }
}