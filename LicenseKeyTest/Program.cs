using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Licensing;

namespace LicenseKeyTest
{
    class Program
    {
        static void Main(string[] args)
        {

            LicenseKey lk = new LicenseKey();
            lk.ExpiryDate = DateTime.Now.AddMonths(1);
            lk.LicensedEdition = LicenseKey.Edition.Professional;
            lk.LicensedUsers = 50;
            lk.LicensedMajorVersion = 8;
            lk.LicensedMinorVersion = 2;
            String sKey = CryptHelper.InsertDashes(LicenseHandler.ToProductCode<LicenseKey>(lk, CryptHelper.LocalMachineID));
            Console.WriteLine(lk);
            Console.WriteLine("Generated Product Key:" + sKey);
            Console.WriteLine("Decrypting Product Key...");
            LicenseKey decryptKey = LicenseHandler.FromProductCode<LicenseKey>(sKey, CryptHelper.LocalMachineID);

            Console.WriteLine("Decrypted Result:" + decryptKey.ToString());
            sKey = "5" + sKey.Substring(1);
            Console.WriteLine("Changed one character in key- using " + sKey);
            try
            {
                LicenseKey faildecrypt = LicenseHandler.FromProductCode<LicenseKey>(sKey, CryptHelper.LocalMachineID);
            }
            catch (Exception exx)
            {
                Console.WriteLine("Exception:" + exx.Message);
            }
            Console.ReadKey();


        }
    }
}
