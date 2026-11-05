using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class TextUtils
{
    private readonly IFileManager files;

    public TextUtils(IFileManager fileManager)
        => this.files = fileManager;

    public async ValueTask LoadAllExternalText()
    {
        await this.LoadAllItemNames();
    }

    /// <summary>
    /// Reads every item's names and description from BINARYDATA\ITEMDESC.EDT (LoadAllItemNames). Each record is
    /// the short name, the name and the description, one after the other.
    /// </summary>
    private ValueTask LoadAllItemNames()
    {
        const int recordSize = SIZE_SHORT_ITEM_NAME + SIZE_ITEM_NAME + SIZE_ITEM_INFO;
        var maxItems = (int)Items.MAXITEMS;

        using var stream = this.files.FileOpen(ITEMSTRINGFILENAME, FileAccess.Read);
        if (stream == Stream.Null)
        {
            return ValueTask.CompletedTask;
        }

        var data = new byte[(int)Math.Min(stream.Length, (long)recordSize * maxItems)];
        int total = 0;
        while (total < data.Length)
        {
            int read = stream.Read(data, total, data.Length - total);
            if (read <= 0)
            {
                break;
            }

            total += read;
        }

        for (int loop = 0; loop < maxItems && (loop + 1) * recordSize <= total; loop++)
        {
            var item = (Items)loop;
            int offset = loop * recordSize;

            ShortItemNames[item] = ExtractString(data.AsSpan(offset, SIZE_SHORT_ITEM_NAME));
            ItemNames[item] = ExtractString(data.AsSpan(offset + SIZE_SHORT_ITEM_NAME, SIZE_ITEM_NAME));
            ItemDescriptions[item] = ExtractString(data.AsSpan(offset + SIZE_SHORT_ITEM_NAME + SIZE_ITEM_NAME, SIZE_ITEM_INFO));
        }

        return ValueTask.CompletedTask;
    }

    private string LoadShortNameItemInfo(ushort ubIndex)
    {
        using var stream = this.files.FileOpen(ITEMSTRINGFILENAME, FileAccess.Read);

        // Get current mercs bio info
        int uiStartSeekAmount = ((SIZE_SHORT_ITEM_NAME + SIZE_ITEM_NAME + SIZE_ITEM_INFO) * ubIndex);

        this.files.FileSeek(stream, uiStartSeekAmount, SeekOrigin.Begin);

        Span<byte> itemNameBuffer = stackalloc byte[SIZE_ITEM_NAME];
        this.files.FileRead(stream, itemNameBuffer, out _);

        var shortItemName = TextUtils.ExtractString(itemNameBuffer);

        return shortItemName;
    }

    private (string itemName, string shortItemName, string itemDescription) LoadItemInfo(ushort ubIndex)
    {
        using var stream = this.files.FileOpen(ITEMSTRINGFILENAME, FileAccess.Read);

        // Get current mercs bio info
        int uiStartSeekAmount = ((SIZE_SHORT_ITEM_NAME + SIZE_ITEM_NAME + SIZE_ITEM_INFO) * ubIndex);

        this.files.FileSeek(stream, uiStartSeekAmount, SeekOrigin.Begin);

        Span<byte> shortItemNameBuffer = stackalloc byte[SIZE_ITEM_NAME];
        this.files.FileRead(stream, shortItemNameBuffer, out _);

        var shortItemName = TextUtils.ExtractString(shortItemNameBuffer);

        Span<byte> itemNameBuffer = stackalloc byte[SIZE_ITEM_NAME];
        this.files.FileRead(stream, itemNameBuffer, out _);

        var itemName = TextUtils.ExtractString(itemNameBuffer);

        // condition added by Chris - so we can get the name without the item info
        // when desired, by passing in a null pInfoString

        // Get the additional info
//            uiStartSeekAmount = (uint)((Text.SIZE_ITEM_NAME + Text.SIZE_SHORT_ITEM_NAME + Text.SIZE_ITEM_INFO) * ubIndex) + Text.SIZE_ITEM_NAME + Text.SIZE_SHORT_ITEM_NAME;
        this.files.FileSeek(stream, uiStartSeekAmount, SeekOrigin.Begin);

        Span<byte> itemInfoBuffer = stackalloc byte[SIZE_ITEM_INFO];
        this.files.FileRead(stream, itemInfoBuffer, out _);

        var itemDescription = TextUtils.ExtractString(itemInfoBuffer);

        return (itemName, shortItemName, itemDescription);
    }

    public static string ExtractString(Span<byte> span)
        => ExtractString((ReadOnlySpan<byte>)span);

    public static string ExtractString(ReadOnlySpan<byte> span)
    {
        var buffer = MemoryMarshal.Cast<byte, char>(span);
        Span<char> mutated = stackalloc char[buffer.Length];

        buffer.CopyTo(mutated);

        int i;
        // Decrement, by 1, any value > 32
        for (i = 0; (i < buffer.Length) && (mutated[i] != 0); i++)
        {
            if (mutated[i] > 33)
            {
                mutated[i] -= (char)1;
            }
        }

        return new String(mutated.Slice(0, i));
    }
}
