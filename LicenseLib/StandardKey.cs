using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BASeCamp.Licensing
{
    public class StandardKey : LicenseKeyData
    {
        public enum Edition
        {
            Standard,
            Professional,
            Enterprise,
            Ultimate
        }

        public Edition LicensedEdition { get; set; }

        public byte LicensedMajorVersion { get; set; }
        public byte LicensedMinorVersion { get; set; }

        public UInt16 LicensedUsers { get; set; }

        public DateTime ExpiryDate { get; set; }

        public StandardKey()
        {
        }

        private byte calcheader()
        {
            return (byte)(((LicensedUsers +
                             (Int16)((Math.Pow((double)LicensedEdition, (double)ExpiryDate.Day)))) +
                            (Math.Pow(LicensedMajorVersion, ((DateOnly)ExpiryDate).ToInt()))) % 255);
        }

        public override string ToString()
        {
            return LicensedEdition.ToString() + " Edition, Major version " + LicensedMajorVersion + " Minor Version:" + LicensedMinorVersion + " Licensed for " + LicensedUsers +
                   " Users, Expires " + ExpiryDate.ToString();
        }

        /// <summary>
        /// calculates our footer value for the given header value.
        /// </summary>
        /// <param name="headervalue"></param>
        /// <returns></returns>
        private byte calcfooter(byte headervalue)
        {
            return
                (byte)
                    ((((DateOnly)ExpiryDate).ToInt() - (LicensedMajorVersion * LicensedMinorVersion) / (byte)(LicensedEdition + 1) + LicensedUsers ^
                      headervalue) % 255);
        }

        public StandardKey(Stream SourceData) : base(SourceData)
        {
            BinaryReader br = new BinaryReader(SourceData);
            byte Major, Minor, Revision;
            UInt16 Build;
            byte header = br.ReadByte();
            LicensedEdition = (Edition)br.ReadInt16();
            LicensedMajorVersion = br.ReadByte();
            LicensedMinorVersion = br.ReadByte();


            LicensedUsers = br.ReadUInt16();
            byte exDay, exMonth;
            UInt16 exYear;
            exDay = (byte)br.ReadByte();
            exMonth = (byte)br.ReadByte();
            exYear = br.ReadUInt16();
            ExpiryDate = new DateOnly(exDay, exMonth, exYear);
            byte footer = br.ReadByte();
            byte calculatedHeader, calculatedFooter;
            calculatedHeader = calcheader();
            calculatedFooter = calcfooter(header);
            if (calculatedHeader != header || calculatedFooter != footer)
            {
                throw new InvalidKeyException("Invalid Product Key.");
            }
        }

        public override void PersistData(Stream Target)
        {
            //add header/footer ints with verification of validity...

            Byte Major, Minor, Revision;
            UInt16 Build;
            byte Header = calcheader();
            byte Footer = calcfooter(Header);
            Major = LicensedMajorVersion;
            Minor = LicensedMinorVersion;
            BinaryWriter bw = new BinaryWriter(Target);
            bw.Write(Header);
            bw.Write((short)LicensedEdition);
            bw.Write(Major);
            bw.Write(Minor);
            bw.Write(LicensedUsers);
            DateOnly dw = new DateOnly(ExpiryDate);
            bw.Write(dw.Day);
            bw.Write(dw.Month);
            bw.Write(dw.Year);
            bw.Write(Footer);
        }
    }
}
