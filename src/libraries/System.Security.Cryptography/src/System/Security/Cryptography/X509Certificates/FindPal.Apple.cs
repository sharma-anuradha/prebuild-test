// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Security.Cryptography.X509Certificates
{
    internal sealed partial class FindPal
    {
        private static partial IFindPal OpenPal(X509Certificate2Collection findFrom, X509Certificate2Collection copyTo, bool validOnly)
        {
            return new AppleCertificateFinder(findFrom, copyTo, validOnly);
        }

        private sealed class AppleCertificateFinder : ManagedCertificateFinder
        {
            public AppleCertificateFinder(X509Certificate2Collection findFrom, X509Certificate2Collection copyTo, bool validOnly)
                : base(findFrom, copyTo, validOnly)
            {
            }

            protected override byte[] GetSubjectPublicKeyInfo(X509Certificate2 cert)
            {
                AppleCertificatePal pal = (AppleCertificatePal)cert.Pal;
                return pal.SubjectPublicKeyInfo;
            }

            protected override X509Certificate2 CloneCertificate(X509Certificate2 cert)
            {
                var clone = new X509Certificate2(cert.Handle);
                GC.KeepAlive(cert); // ensure cert's safe handle isn't finalized while raw handle is in use
                return clone;
            }
        }
    }
}
