// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Net.Security
{
    internal partial struct SslConnectionInfo
    {
        public int Protocol { get; private set; }
        public TlsCipherSuite TlsCipherSuite { get; private set; }
        public int DataCipherAlg { get; private set; }
        public int DataKeySize { get; private set; }
        public int DataHashAlg { get; private set; }
        public int DataHashKeySize { get; private set; }
        public int KeyExchangeAlg { get; private set; }
        public int KeyExchKeySize { get; private set; }

        public byte[]? ApplicationProtocol { get; internal set; }
    }
}
