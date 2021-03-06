// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;

namespace System.Net.Security
{
    internal partial struct SslConnectionInfo
    {
        private void MapCipherSuite(TlsCipherSuite cipherSuite)
        {
            TlsCipherSuite = cipherSuite;

            TlsCipherSuiteData data = TlsCipherSuiteData.GetCipherSuiteData(cipherSuite);
            KeyExchangeAlg = (int)data.KeyExchangeAlgorithm;
            KeyExchKeySize = 0;
            DataCipherAlg = (int)data.CipherAlgorithm;
            DataKeySize = data.CipherAlgorithmStrength;
            DataHashAlg = (int)data.MACAlgorithm;
            DataHashKeySize = data.MACAlgorithmStrength;
        }
    }
}
