// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace System.Xml
{
    public partial class XmlUrlResolver : XmlResolver
    {
        // Maps a URI to an Object containing the actual resource.
        public override async Task<object> GetEntityAsync(Uri absoluteUri, string? role, Type? ofObjectToReturn)
        {
            if (ofObjectToReturn == null || ofObjectToReturn == typeof(System.IO.Stream) || ofObjectToReturn == typeof(object))
            {
                return await XmlDownloadManager.GetStreamAsync(absoluteUri, _credentials, _proxy).ConfigureAwait(false);
            }

            throw new XmlException(SR.Xml_UnsupportedClass, string.Empty);
        }
    }
}
