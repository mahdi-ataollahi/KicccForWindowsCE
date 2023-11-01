using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Kiccc.Ing.PcPos.Multiplex
{
    internal sealed class MultiplexParameterGenerator
    {
        private string DepositIdentifier
        {
            get;
            set;
        }

        private string DigitControlA
        {
            get;
            set;
        }

        private string DigitControlB
        {
            get;
            set;
        }

        private string DigitControlC
        {
            get;
            set;
        }

        private string DigitControlD
        {
            get;
            set;
        }

        private string ServiceIdentifier
        {
            get;
            set;
        }

        public MultiplexParameterGenerator()
        {
        }

        private static string ByteToString(byte[] buff)
        {
            string str = "";
            for (int index = 0; index < (int)buff.Length; index++)
            {
                str = string.Concat(str, buff[index].ToString("X2"));
            }
            return str;
        }

        private string Encrypt(string toEncrypt, string key)
        {
            string[] strArray = key.Split(new char[] { ' ' });
            byte[] numArray = new byte[16];
            for (int index = 0; index < 16; index++)
            {
                int num = strArray[index][0] + strArray[index][1];
                numArray[index] = BitConverter.GetBytes(num)[0];
            }
            TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider()
            {
                KeySize = 128,
                Key = numArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };
            ICryptoTransform encryptor = cryptoServiceProvider.CreateEncryptor();
            byte[] bytes = Encoding.Unicode.GetBytes(toEncrypt);
            byte[] inArray = encryptor.TransformFinalBlock(bytes, 0, (int)bytes.Length);
            return Convert.ToBase64String(inArray, 0, (int)inArray.Length);
        }

        private string GenerateA(string docCode, string orgCode, string serviceCode, string SequenceCode)
        {
            string str = string.Concat(docCode, orgCode, serviceCode, SequenceCode);
            if (!string.IsNullOrEmpty(str))
            {
                int num = Verhoeff.CalculateCheckDigit(str.Trim());
                str = num.ToString();
            }
            return str;
        }

        private string GenerateB(string docCode, string orgCode, string serviceCode, string SequenceCode, string amount, string yearCode, string periodCode, string bankCode)
        {
            int num = Verhoeff.CalculateCheckDigit(string.Concat(new string[] { docCode, this.DigitControlA, orgCode, serviceCode, SequenceCode, amount, this.DigitControlC, yearCode, periodCode, bankCode }).Trim());
            return num.ToString();
        }

        private string GenerateC(string amount, string yearCode, string periodCode, string bankCode)
        {
            string str = string.Concat(amount, yearCode, periodCode, bankCode);
            if (!string.IsNullOrEmpty(str))
            {
                int num = Verhoeff.CalculateCheckDigit(str.Trim());
                str = num.ToString();
            }
            return str;
        }

        private string GenerateD(string docCode, string orgCode, string serviceCode, string SequenceCode, string amount, string yearCode, string periodCode, string bankCode)
        {
            int num = Verhoeff.CalculateCheckDigit(string.Concat(new string[] { amount, this.DigitControlC, yearCode, periodCode, bankCode, docCode, this.DigitControlA, this.DigitControlB, orgCode, serviceCode, SequenceCode }).Trim());
            return num.ToString();
        }

        public string GetDepositIdentifiers(string docCode, string orgCode, string serviceCode, string SequenceCode, string amount, string yearCode, string periodCode, string bankCode)
        {
            this.DigitControlA = this.GenerateA(docCode, orgCode, serviceCode, SequenceCode);
            this.DigitControlC = this.GenerateC(amount, yearCode, periodCode, bankCode);
            this.DigitControlB = this.GenerateB(docCode, orgCode, serviceCode, SequenceCode, amount, yearCode, periodCode, bankCode);
            this.DigitControlD = this.GenerateD(docCode, orgCode, serviceCode, SequenceCode, amount, yearCode, periodCode, bankCode);
            return string.Concat(new string[] { amount, this.DigitControlC, this.DigitControlD, yearCode, periodCode, bankCode });
        }

        public string GetServiceIdentifiers(string docCode, string orgCode, string serviceCode, string SequenceCode, string amount, string yearCode, string periodCode, string bankCode)
        {
            this.DigitControlA = this.GenerateA(docCode, orgCode, serviceCode, SequenceCode);
            this.DigitControlC = this.GenerateC(amount, yearCode, periodCode, bankCode);
            this.DigitControlB = this.GenerateB(docCode, orgCode, serviceCode, SequenceCode, amount, yearCode, periodCode, bankCode);
            this.DigitControlD = this.GenerateD(docCode, orgCode, serviceCode, SequenceCode, amount, yearCode, periodCode, bankCode);
            return string.Concat(new string[] { docCode, this.DigitControlA, this.DigitControlB, orgCode, serviceCode, SequenceCode });
        }

        public bool IsPaymentCipherValid(string depositIdentifier, string serviceIdentifier, string traceNumber, string paymentCipher, string key)
        {
            int num = Verhoeff.CalculateCheckDigit(string.Concat(serviceIdentifier.Trim(), depositIdentifier.Trim()).Trim());
            string str1 = num.ToString();
            num = Verhoeff.CalculateCheckDigit(string.Concat(traceNumber, str1).Trim());
            string str2 = num.ToString();
            bool flag = this.ReplaceWords(this.Encrypt(string.Concat(traceNumber, str1, str2, "00000000"), key)) == paymentCipher;
            return flag;
        }

        private string ReplaceWords(string input)
        {
            input = input.Replace('A', '1').Replace('B', '2').Replace('C', '3').Replace('D', '4').Replace('E', '5').Replace('F', '6');
            input = input.Replace('a', '1').Replace('b', '2').Replace('c', '3').Replace('d', '4').Replace('e', '5').Replace('f', '6');
            return input;
        }

        private static byte[] StringToByte(string StringToConvert)
        {
            char[] chArray = StringToConvert.ToCharArray();
            byte[] numArray = new byte[(int)chArray.Length];
            for (int index = 0; index < (int)chArray.Length; index++)
            {
                numArray[index] = Convert.ToByte(chArray[index]);
            }
            return numArray;
        }

        private static byte[] StringToByte(string StringToConvert, int length)
        {
            char[] chArray = StringToConvert.ToCharArray();
            byte[] numArray = new byte[length];
            for (int index = 0; index < (int)chArray.Length; index++)
            {
                numArray[index] = Convert.ToByte(chArray[index]);
            }
            return numArray;
        }
    }
}