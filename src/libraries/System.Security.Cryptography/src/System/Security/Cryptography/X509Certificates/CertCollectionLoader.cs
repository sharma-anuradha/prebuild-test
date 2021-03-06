// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Internal.Cryptography;

namespace System.Security.Cryptography.X509Certificates
{
    internal sealed class CertCollectionLoader : ILoaderPal
    {
        private List<ICertificatePal>? _certs;

        internal CertCollectionLoader(List<ICertificatePal> certs)
        {
            _certs = certs;
        }

        public void Dispose()
        {
            // If there're still certificates, dispose them.
            _certs?.DisposeAll();
        }

        public void MoveTo(X509Certificate2Collection collection)
        {
            Debug.Assert(collection != null);

            List<ICertificatePal>? localCerts = Interlocked.Exchange(ref _certs, null);
            Debug.Assert(localCerts != null);

            foreach (ICertificatePal certPal in localCerts)
            {
                collection.Add(new X509Certificate2(certPal));
            }
        }
    }
}
