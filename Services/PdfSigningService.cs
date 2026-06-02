using iText.Bouncycastle;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Kernel.Crypto;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Services
{
    public class PdfSigningService
    {
        public byte[] SignPdf(byte[] pdfData, string certificatePath, string certificatePassword)
        {
            // Загружаем PFX через BouncyCastle (обходим ограничения CNG)
            Pkcs12Store store;
            using (var stream = new FileStream(certificatePath, FileMode.Open, FileAccess.Read))
            {
                store = new Pkcs12StoreBuilder().Build();
                store.Load(stream, certificatePassword.ToCharArray());
            }

            // Ищем первый ключ и сертификат
            string alias = null;
            foreach (string a in store.Aliases)
            {
                if (store.IsKeyEntry(a))
                {
                    alias = a;
                    break;
                }
            }
            if (alias == null)
                throw new System.Exception("Закрытый ключ не найден в PFX");

            var chain = store.GetCertificateChain(alias);
            var certificate = chain[0].Certificate;
            var privateKey = store.GetKey(alias).Key;

            // Оборачиваем в адаптеры iText
            var bcCert = new X509CertificateBC(certificate);
            var certificateChain = new List<iText.Commons.Bouncycastle.Cert.IX509Certificate> { bcCert };
            var keyBC = new PrivateKeyBC(privateKey);
            var signature = new PrivateKeySignature(keyBC, DigestAlgorithms.SHA256);

            using (var reader = new PdfReader(new MemoryStream(pdfData)))
            using (var outputStream = new MemoryStream())
            {
                var signer = new PdfSigner(reader, outputStream, new StampingProperties());
                signer.SignDetached(signature, certificateChain.ToArray(), null, null, null, 0,
                    PdfSigner.CryptoStandard.CMS);
                return outputStream.ToArray();
            }
        }

        public class PdfSignatureInfo
        {
            public string SignerName { get; set; }
            public DateTime SigningTime { get; set; }
            public bool IsValid { get; set; }
            public string Reason { get; set; }
        }

        public static List<PdfSignatureInfo> GetSignatureInfo(byte[] pdfData)
        {
            var result = new List<PdfSignatureInfo>();
            try
            {
                using (var reader = new PdfReader(new MemoryStream(pdfData)))
                using (var document = new PdfDocument(reader))
                {
                    var signatureUtil = new SignatureUtil(document);
                    foreach (var fieldName in signatureUtil.GetSignatureNames())
                    {
                        var signature = signatureUtil.GetSignature(fieldName);
                        var pkcs7 = signatureUtil.ReadSignatureData(fieldName);

                        string signerName = "Неизвестный подписант";
                        DateTime signingTime = DateTime.MinValue;
                        bool isValid = false;

                        if (pkcs7 != null)
                        {
                            // --- Дата подписи ---
                            var signDateField = pkcs7.GetType().GetField("signDate",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            if (signDateField != null)
                                signingTime = (DateTime)signDateField.GetValue(pkcs7);

                            // --- Сертификат ---
                            var signingCert = pkcs7.GetSigningCertificate();
                            if (signingCert != null)
                            {
                                try
                                {
                                    var certBytes = signingCert.GetEncoded();
                                    var parser = new Org.BouncyCastle.X509.X509CertificateParser();
                                    var bcCert = parser.ReadCertificate(certBytes);
                                    if (bcCert != null)
                                    {
                                        var subjectDN = bcCert.SubjectDN.ToString();
                                        var cnMatch = System.Text.RegularExpressions.Regex.Match(subjectDN, @"CN=([^,]+)");
                                        signerName = cnMatch.Success ? cnMatch.Groups[1].Value.Trim() : subjectDN;
                                        isValid = bcCert.IsValidNow;
                                    }
                                }
                                catch { }
                            }
                        }

                        result.Add(new PdfSignatureInfo
                        {
                            SignerName = signerName,
                            SigningTime = signingTime,
                            IsValid = isValid
                        });
                    }
                }
            }
            catch { }
            return result;
        }
    }
}
