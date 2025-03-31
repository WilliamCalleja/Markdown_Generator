using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum ParagraphFormat
{
    None, H1, H2, H3, H4, H5, Item, Note, Rules
}

public static class FormatParagraph
{
    public static string Format(this string text, ParagraphFormat format)
    {
        var output = "";
        switch (format)
        {
            case ParagraphFormat.H1:
                output += $"# {text}\n";
                break;
            case ParagraphFormat.H2:
                output += $"## {text}\n";
                break;
            case ParagraphFormat.H3:
                output += $"### {text}\n";
                break;
            case ParagraphFormat.H4:
                output += $"#### {text}\n";
                break;
            case ParagraphFormat.H5:
                output += $"##### {text}\n";
                break;
            case ParagraphFormat.Item:
                output += "item(\n";
                output += $"{text}\n";
                output += ")\n";
                break;
            case ParagraphFormat.Note:
                output += "note(\n";
                output += $"{text}\n";
                output += ")\n";
                break;
            case ParagraphFormat.Rules:
                output += "rules(\n";
                output += $"{text}\n";
                output += ")\n";
                break;
            case ParagraphFormat.None:
                output += $"{text}\n";
                break;
        }
        return output;
    }
}

[Serializable]
public class FormattingOptions
{
    public ParagraphFormat titleFormat;
    public ParagraphFormat contentFormat;
}

[Serializable]
public class Chapter
{
    public string title;
    public List<string> description;
    public List<Entry> entries;

    public string GetChapter()
    {
        var titleEntry = new SubEntry()
        {
            content = title,
            options = new FormattingOptions()
            {
                titleFormat = ParagraphFormat.H1
            }
        };
        var descriptionEntry = new SubEntry()
        {
            content = description.Aggregate("", (prev, next) => $"{prev}{next}\n"),
            options = new FormattingOptions()
            {
                titleFormat = ParagraphFormat.None
            }
        };
        
        return titleEntry.GetChapterTitle() + 
               descriptionEntry.GetContent() +
               entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
    }
}

[Serializable]
public class ListEntry
{
    public string title;
    public string content;

    public string GetListEntry()
    {
        var output = "- ";
        if (!string.IsNullOrEmpty(title)) output += $"**{title} -** ";
        if (!string.IsNullOrEmpty(content)) output += $"{content}\n";
        return output;
    }
    public string GetUnorderedListEntry()
    {
        var output = "";
        if (!string.IsNullOrEmpty(title)) output += $"**{title} -** ";
        if (!string.IsNullOrEmpty(content)) output += $"{content}\n";
        output += "\n";
        return output;
    }

    public string GetHeaderListEntry(int h)
    {
        var output = "";
        if (!string.IsNullOrEmpty(title))
        {
            switch (h)
            {
                case 1: output += $"# {title}\n"; break;
                case 2: output += $"## {title}\n"; break;
                case 3: output += $"### {title}\n"; break;
                case 4: output += $"#### {title}\n"; break;
                case 5: output += $"##### {title}\n"; break;
            }
        }
        if (!string.IsNullOrEmpty(content)) output += $"{content}\n";
        output += "\n";
        return output;
    }
}

[Serializable]
public class BoxEntry
{
    public string h1;
    public string h2;
    public string h3;
    public string h4;
    public List<string> content;
    public List<ListEntry> list;

    public string GetEntry(string type)
    {
        var output = $"{type}(";
        if (!string.IsNullOrEmpty(h1)) output += $"\n#{h1}";
        if (!string.IsNullOrEmpty(h2)) output += $"\n##{h2}";
        output += $"\n-";
        if (!string.IsNullOrEmpty(h3)) output += $"\n###{h3}";
        if (!string.IsNullOrEmpty(h4)) output += $"\n####{h4}";
        foreach (var item in content)
        {
            var listText = list.Aggregate("", (prev, next) => $"{prev}{next.GetListEntry()}");
            var ulistText = list.Aggregate("", (prev, next) => $"{prev}{next.GetUnorderedListEntry()}");
            var h3ListText = list.Aggregate("", (prev, next) => $"{prev}{next.GetHeaderListEntry(3)}");
            var h4ListText = list.Aggregate("", (prev, next) => $"{prev}{next.GetHeaderListEntry(4)}");
            var contentText = item
                .Replace("[LIST]", $"{listText}")
                .Replace("[ULIST]", $"\n{ulistText}")
                .Replace("[H3LIST]", $"\n{h3ListText}")
                .Replace("[H4LIST]", $"\n{h4ListText}");
            if (!string.IsNullOrEmpty(contentText)) output += $"\n{contentText}";
        }
        output += "\n)";
        return output;
    }
}

[Serializable]
public class Entry
{
    public string title;
    public List<string> content;
    public FormattingOptions options;
    public List<ListEntry> list;
    public List<BoxEntry> boxes;
    public List<BoxEntry> items;

    public string GetEntry()
    {
        var titleEntry = new SubEntry()
        {
            content = title,
            options = options
        };
        var contentEntry = new SubEntry()
        {
            content = content.Aggregate("", (prev, next) => $"{prev}{next}\n\n"),
            options = options
        };
        var listText = list.Aggregate("", (prev, next) => $"{prev}{next.GetListEntry()}");
        var ulistText = list.Aggregate("", (prev, next) => $"{prev}{next.GetUnorderedListEntry()}");
        var h2ListText = list.Aggregate("", (prev, next) => $"{prev}{next.GetHeaderListEntry(2)}");
        var h3ListText = list.Aggregate("", (prev, next) => $"{prev}{next.GetHeaderListEntry(3)}");
        var h4ListText = list.Aggregate("", (prev, next) => $"{prev}{next.GetHeaderListEntry(4)}");
        var box0 = boxes is { Count: > 0 } ? boxes[0].GetEntry("note") : "";
        var box1 = boxes is { Count: > 1 } ? boxes[1].GetEntry("note") : "";
        var box2 = boxes is { Count: > 2 } ? boxes[2].GetEntry("note") : "";
        var box3 = boxes is { Count: > 3 } ? boxes[3].GetEntry("note") : "";
        var box4 = boxes is { Count: > 4 } ? boxes[4].GetEntry("note") : "";
        
        var item0 = items is { Count: > 0 } ? items[0].GetEntry("item") : "";
        var item1 = items is { Count: > 1 } ? items[1].GetEntry("item") : "";
        var item2 = items is { Count: > 2 } ? items[2].GetEntry("item") : "";
        var item3 = items is { Count: > 3 } ? items[3].GetEntry("item") : "";
        var item4 = items is { Count: > 4 } ? items[4].GetEntry("item") : "";
        var item5 = items is { Count: > 5 } ? items[5].GetEntry("item") : "";
        var item6 = items is { Count: > 6 } ? items[6].GetEntry("item") : "";
        var item7 = items is { Count: > 7 } ? items[7].GetEntry("item") : "";
        var item8 = items is { Count: > 8 } ? items[8].GetEntry("item") : "";
        var item9 = items is { Count: > 9 } ? items[9].GetEntry("item") : "";
        
        var boxes2 = "";
        for (var i = 0; i < boxes.Count; i++)
        {
            boxes2 += boxes[i].GetEntry("note");
            boxes2 += (i % 2 != 0) ? "|" : "/";
        }
        
        var itemText = items.Aggregate("", (prev, next) => $"{prev}\n{next.GetEntry("item")}");

        if (items is { Count: > 4 })
        {
            var count = 0;
            var column = true;
            itemText = "";
            foreach (var item in items)
            {
                count++;
                itemText += $"{item.GetEntry("item")}\n";
                if (count != 4) continue;
                count = 0;
                if (column)
                {
                    itemText += "|\n";
                    column = false;
                }
                else
                {
                    itemText += "=\n";
                    column = true;
                }
            }
        }
        
        var contentText = contentEntry.GetContent()
            .Replace("[LIST]", $"{listText}")
            .Replace("[ULIST]", $"\n{ulistText}")
            .Replace("[H2LIST]", $"\n{h2ListText}")
            .Replace("[H3LIST]", $"\n{h3ListText}")
            .Replace("[H4LIST]", $"\n{h4ListText}")
            .Replace("[BOX0]", $"\n{box0}")
            .Replace("[BOX1]", $"\n{box1}")
            .Replace("[BOX2]", $"\n{box2}")
            .Replace("[BOX3]", $"\n{box3}")
            .Replace("[BOX4]", $"\n{box4}")
            .Replace("[ITEMS]", $"\n{itemText}")
            .Replace("[ITEM0]", $"\n{item0}")
            .Replace("[ITEM1]", $"\n{item1}")
            .Replace("[ITEM2]", $"\n{item2}")
            .Replace("[ITEM3]", $"\n{item3}")
            .Replace("[ITEM4]", $"\n{item4}")
            .Replace("[ITEM5]", $"\n{item5}")
            .Replace("[ITEM6]", $"\n{item6}")
            .Replace("[ITEM7]", $"\n{item7}")
            .Replace("[ITEM8]", $"\n{item8}")
            .Replace("[ITEM9]", $"\n{item9}")
            .Replace("[BOXES2]", $"\n{boxes2}");
        return $"{titleEntry.GetTitle()}{contentText}";
    }
}

[Serializable]
public class SubEntry
{
    public string content;
    public FormattingOptions options;

    public SubEntry() { options = new FormattingOptions(); }

    public string GetContent()
    {
        var output = "";
        output += $"{content.Format(options.contentFormat)}\n";
        return output;
    }
    public string GetTitle()
    {
        var output = "";
        output += $"{content.Format(options.titleFormat)}\n";
        return output;
    }

    public string GetChapterTitle()
    {
        var output = "";
        output += $"head(\n{content.Format(options.titleFormat)}\n)\n";
        return output;
    }
}

public class EntryManager : MonoBehaviour
{
    public List<Chapter> chapters;
    public string output;
    private EquipmentManager _equipmentManager;

    private void OnValidate()
    {
        if (_equipmentManager == null) _equipmentManager = FindFirstObjectByType<EquipmentManager>();
        var equipmentChapter = _equipmentManager.output;
        
        output = "";
        output += "watermark (";
        output += "\nWilliam Calleja - Katacross 2025";
        output += "\n)\n";
        output += "title (\nMain Rulebook\n)\n";
        output += chapters.Aggregate("", (prev, next) => $"{prev}{next.GetChapter()}");
        output += equipmentChapter;
        output = output.Replace(@"[\n]", "\n");
    }
}
