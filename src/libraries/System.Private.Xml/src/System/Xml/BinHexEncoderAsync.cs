// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Xml
{
    internal static partial class BinHexEncoder
    {
        internal static Task EncodeAsync(byte[] buffer, int index, int count, XmlWriter writer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            if (index < 0 || (uint)count > buffer.Length - index)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count));
            }
            return Core(buffer, index, count, writer);

            static async Task Core(byte[] buffer, int index, int count, XmlWriter writer)
            {
                char[] chars = new char[(count * 2) < CharsChunkSize ? (count * 2) : CharsChunkSize];
                int endIndex = index + count;
                while (index < endIndex)
                {
                    int cnt = (count < CharsChunkSize / 2) ? count : CharsChunkSize / 2;
                    HexConverter.EncodeToUtf16(buffer.AsSpan(index, cnt), chars);
                    await writer.WriteRawAsync(chars, 0, cnt * 2).ConfigureAwait(false);
                    index += cnt;
                    count -= cnt;
                }
            }
        }
    } // class
} // namespace
