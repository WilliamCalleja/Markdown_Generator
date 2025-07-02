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
    public string output;
    public string chapter => GetChapter();

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
        output = titleEntry.GetChapterTitle() + 
                   descriptionEntry.GetContent() +
                   entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
        return output;
    }
}

[Serializable]
public class ListEntry
{
    public string title;
    public string content;

    public string GetListEntry()
    {
        var modContent = content.Replace(@"[\n]", "\n");
        var output = "- ";
        if (!string.IsNullOrEmpty(title)) output += $"**{title} -** ";
        if (!string.IsNullOrEmpty(modContent)) output += $"{modContent}\n";
        return output;
    }
    public string GetUnorderedListEntry()
    {
        var modContent = content.Replace(@"[\n]", "\n");
        var output = "";
        if (!string.IsNullOrEmpty(title)) output += $"**{title} -** ";
        if (!string.IsNullOrEmpty(modContent)) output += $"{modContent}\n";
        output += "\n";
        return output;
    }

    public string GetHeaderListEntry(int h)
    {
        var modContent = content.Replace(@"[\n]", "\n");
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
        if (!string.IsNullOrEmpty(modContent)) output += $"{modContent}\n";
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
        var modContent = content.Select(c => c.Replace(@"[\n]", "\n")).ToList();
        var output = $"{type}(";
        if (!string.IsNullOrEmpty(h1)) output += $"\n#{h1}";
        if (!string.IsNullOrEmpty(h2)) output += $"\n##{h2}";
        output += $"\n-";
        if (!string.IsNullOrEmpty(h3)) output += $"\n###{h3}";
        if (!string.IsNullOrEmpty(h4)) output += $"\n####{h4}";
        foreach (var item in modContent)
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
public class TableEntry
{
    public List<RowEntry> rows;
    public string Text => GetText();

    public string GetText()
    {
        var output = rows[0].Text;
        output += "\n-- | --\n";
        output += rows.Skip(1).Aggregate("", (prev, next) => $"{prev}\n{next.Text}");
        return output;
    }
}

[Serializable]
public class RowEntry
{
    public List<string> cells;
    public string Text => GetText();

    public string GetText()
    {
        return cells.Aggregate("", (prev, next) => $"{prev} | {next}");
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
    public List<TableEntry> tables;

    public string GetEntry()
    {
        var modContent = content.Select(c => c.Replace(@"[\n]", "\n")).ToList();
        var titleEntry = new SubEntry()
        {
            content = title,
            options = options
        };
        var contentEntry = new SubEntry()
        {
            content = modContent.Aggregate("", (prev, next) => $"{prev}{next}\n\n"),
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
        var box5 = boxes is { Count: > 5 } ? boxes[5].GetEntry("note") : "";
        var box6 = boxes is { Count: > 6 } ? boxes[6].GetEntry("note") : "";
        var box7 = boxes is { Count: > 7 } ? boxes[7].GetEntry("note") : "";
        var box8 = boxes is { Count: > 8 } ? boxes[8].GetEntry("note") : "";
        var box9 = boxes is { Count: > 9 } ? boxes[9].GetEntry("note") : "";
        var box10 = boxes is { Count: > 10 } ? boxes[10].GetEntry("note") : "";
        var box11 = boxes is { Count: > 11 } ? boxes[11].GetEntry("note") : "";
        var box12 = boxes is { Count: > 12 } ? boxes[12].GetEntry("note") : "";
        var box13 = boxes is { Count: > 13 } ? boxes[13].GetEntry("note") : "";
        var box14 = boxes is { Count: > 14 } ? boxes[14].GetEntry("note") : "";
        var box15 = boxes is { Count: > 15 } ? boxes[15].GetEntry("note") : "";
        var box16 = boxes is { Count: > 16 } ? boxes[16].GetEntry("note") : "";
        var box17 = boxes is { Count: > 17 } ? boxes[17].GetEntry("note") : "";
        var box18 = boxes is { Count: > 18 } ? boxes[18].GetEntry("note") : "";
        var box19 = boxes is { Count: > 19 } ? boxes[19].GetEntry("note") : "";
        
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
        var item10 = items is { Count: > 10 } ? items[10].GetEntry("item") : "";
        var item11 = items is { Count: > 11 } ? items[11].GetEntry("item") : "";
        var item12 = items is { Count: > 12 } ? items[12].GetEntry("item") : "";
        var item13 = items is { Count: > 13 } ? items[13].GetEntry("item") : "";
        var item14 = items is { Count: > 14 } ? items[14].GetEntry("item") : "";
        var item15 = items is { Count: > 15 } ? items[15].GetEntry("item") : "";
        var item16 = items is { Count: > 16 } ? items[16].GetEntry("item") : "";
        var item17 = items is { Count: > 17 } ? items[17].GetEntry("item") : "";
        var item18 = items is { Count: > 18 } ? items[18].GetEntry("item") : "";
        var item19 = items is { Count: > 19 } ? items[19].GetEntry("item") : "";

        var tableText = tables.Aggregate("", (prev, next) => $"{prev}\n\n{next.Text}");
        
        var boxes2 = "";
        for (var i = 0; i < boxes.Count; i++)
        {
            boxes2 += boxes[i].GetEntry("note");
            boxes2 += (i % 2 == 0) ? "\n|\n" : "\n/\n";
            boxes2 += ((i+1) % 6 == 0) ? "\n=\n" : "";
        }

        var itemText = items.Aggregate("", (prev, next) => $"{prev}\n{next.GetEntry("item")}");
        var uItemText = itemText;
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
            .Replace("[BOX5]", $"\n{box5}")
            .Replace("[BOX6]", $"\n{box6}")
            .Replace("[BOX7]", $"\n{box7}")
            .Replace("[BOX8]", $"\n{box8}")
            .Replace("[BOX9]", $"\n{box9}")
            .Replace("[BOX10]", $"\n{box10}")
            .Replace("[BOX11]", $"\n{box11}")
            .Replace("[BOX12]", $"\n{box12}")
            .Replace("[BOX13]", $"\n{box13}")
            .Replace("[BOX14]", $"\n{box14}")
            .Replace("[BOX15]", $"\n{box15}")
            .Replace("[BOX16]", $"\n{box16}")
            .Replace("[BOX17]", $"\n{box17}")
            .Replace("[BOX18]", $"\n{box18}")
            .Replace("[BOX19]", $"\n{box19}")
            .Replace("[ITEMS]", $"\n{itemText}")
            .Replace("[UITEMS]", $"\n{uItemText}")
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
            .Replace("[ITEM10]", $"\n{item10}")
            .Replace("[ITEM11]", $"\n{item11}")
            .Replace("[ITEM12]", $"\n{item12}")
            .Replace("[ITEM13]", $"\n{item13}")
            .Replace("[ITEM14]", $"\n{item14}")
            .Replace("[ITEM15]", $"\n{item15}")
            .Replace("[ITEM16]", $"\n{item16}")
            .Replace("[ITEM17]", $"\n{item17}")
            .Replace("[ITEM18]", $"\n{item18}")
            .Replace("[ITEM19]", $"\n{item19}")
            .Replace("[BOXES2]", $"\n{boxes2}")
            .Replace("[TABLES]", $"\n{tableText}");
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
        var modContent = content.Replace(@"[\n]", "\n");
        var output = "";
        output += $"{modContent.Format(options.contentFormat)}\n";
        return output;
    }
    public string GetTitle()
    {
        var modContent = content.Replace(@"[\n]", "\n");
        var output = "";
        output += $"{modContent.Format(options.titleFormat)}\n";
        return output;
    }

    public string GetChapterTitle()
    {
        var modContent = content.Replace(@"[\n]", "\n");
        var output = "";
        output += $"head(\n{modContent.Format(options.titleFormat)}\n)\n";
        return output;
    }
}

public class EntryManager : MonoBehaviour
{
    public BestiaryManager bestiaryManager;
    public string input;
    public List<BoxEntry> entries;
    public List<ListEntry> list;
    public TableEntry table;
    public List<Chapter> chapters;
    public string output;
    private EquipmentManager _equipmentManager;


    [ContextMenu("ConvertToList")]
    public void ConvertToList()
    {
        list = new List<ListEntry>();
        var lines = input.Split("- **").Where(i => !string.IsNullOrEmpty(i)).ToList();
        foreach (var line in lines)
        {
            list.Add(new ListEntry()
            {
                title = line.Split("-**")[0],
                content = line.Split("-**")[1]
            });
        }
    }
    

    [ContextMenu("ConvertToEntries")]
    public void ConvertToEntries()
    {
        input = input.Replace(")", "");
        input = input.Replace("|", "");
        input = input.Replace("=", "");
        input = input.Replace("-", "");
        input = input.Replace("/", "");
        var data = input.Split("note(").Where(i => !string.IsNullOrEmpty(i.Trim())).ToList();
        
        entries = new List<BoxEntry>();
        Debug.Log(data.Aggregate("", (prev, next) => $"{prev}\n{next}"));
        foreach (var line in data)
        {
            var value = line.Trim();
            var h1 = value.Substring(value.IndexOf("#") + 1, value.IndexOf("##") -1);
            var h2 = value.Substring(value.IndexOf("##") + 2, value.IndexOf("###") - value.IndexOf("##") - 2);
            var h3 = value.Substring(value.IndexOf("###") + 3, value.IndexOf("####") - value.IndexOf("###") - 3);
            var h4 = value.Substring(value.IndexOf("####") + 4, value.IndexOf("*") - value.IndexOf("####") - 4);
            var desc = value.Split("*")[1];
            var mech = value.Split("*")[2];
            entries.Add(new BoxEntry()
            {
                h1 = h1.Trim(),
                h2 = h2.Trim(),
                h3 = h3.Trim(),
                h4 = h4.Trim(),
                content = new List<string>() {
                    $"*{desc.Trim()}*", mech.Trim()
                }
            });
            Debug.Log($"H1 {h1}, H2 {h2}, H3 {h3}, H4 {h4}, D {desc}, m {mech}");
        }
    }
    [ContextMenu("ConvertToTable")]
    public void ConvertToTable()
    {
        table = new TableEntry()
        {
            rows = new List<RowEntry>()
        };
        input = input.Replace("-- | --", "");
        var entries = input
            .Split("|")
            .Where(i => !string.IsNullOrEmpty(i.Trim()))
            .Select(i => i.Trim())
            .ToList();
        
        for (var i = 0; i < entries.Count; i+= 5)
        {
            table.rows.Add(new RowEntry()
            {
                cells = entries.Skip(i).Take(5).ToList()
            });
        }
        
        foreach (var row in table.rows)
        {
            Debug.Log(row.cells.Aggregate("", (prev, next) => $"{prev}|{next}"));
        }
    }

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        if (_equipmentManager == null) _equipmentManager = FindFirstObjectByType<EquipmentManager>();
        var equipmentChapter = _equipmentManager.output;
        var bestiaryChapter = bestiaryManager.chapter.output;
        
        output = "";
        output += "watermark (";
        output += "\nWilliam Calleja - Katacross 2025";
        output += "\n)\n";
        output += "title (\nMain Rulebook\n)\n";
        output += chapters.Aggregate("", (prev, next) => $"{prev}{next.chapter}");
        output = output.Replace("[EQUIPMENT]", equipmentChapter);
        output = output.Replace("[BESTIARY]", bestiaryChapter);
        //output += equipmentChapter;
        output = output.Replace(@"[\n]", "\n");
    }
}
