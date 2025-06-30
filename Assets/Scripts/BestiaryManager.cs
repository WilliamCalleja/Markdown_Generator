using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum CreatureSize
{
    Gargantuan = 8,
    Vast = 9, 
    Colossal = 10,
    Immense = 11,
    Titanic = 12,
    Tiny = -2,
    Small = -1,
    Medium = 0,
    Large = 1,
    Huge = 2,
    Enormous = 3, 
    Gigantic = 4,
    Tremendous = 5,
    Mountainous = 6,
    Humongous = 7,
}
[Serializable]
public enum CreatureType
{
    Lizard,
    Mammal,
    Undead,
    Insect,
    Cephalopod,
    Bird,
    Automata,
    Character,
    Extraplanar,
    Horror
}

[Serializable]
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[Serializable]
public enum AttackType
{
    Melee,
    Ranged
}

[Serializable]
public enum ActionType
{
    Movement,
    Action,
    Main,
    Passive,
    Initiative,
    Disease,
    Attack,
    Death
}

[Serializable]
public class ChapterData
{
    public List<AbilityEntry> abilities;
    public string title;
    public string description;

    public List<Category> categories;
    
    public string output;
    
    public void UpdateOutput()
    {
        output = "";
        output += $"# {title}";
        output += "\n-";
        if(!string.IsNullOrEmpty(description)) output += $"{description}";
        foreach (var category in categories)
        {
            output += $"\n{category.UpdateOutput(abilities)}";
        }
    }
}

[Serializable]
public class Category
{
    public string title;
    public string description;
    public List<BeastEntry> beasts;
    
    public string UpdateOutput(List<AbilityEntry> fullAbilities)
    {
        var output = "";
        output += $"## {title}";
        output += "\n-";
        if(!string.IsNullOrEmpty(description)) output += $"\n{description}";
        if (beasts.Count > 1) output += "\n/\n";
        else output += "\n";
        for (var i = 0; i < beasts.Count; i++)
        {
            output += beasts[i].UpdateOutput(fullAbilities);
            if (i%2 == 0)
                output += "\n|\n";
            else
                output += "\n/\n";
        }
        output += "\n/\n";
        return output;
    }
}

[Serializable]
public class AttackEntry
{
    public string title;
    public AttackType type;
    [Range(0, 4)] public int damage;
    public int range;
    public string traits;

    public string UpdateOutput(int closeCombat, int rangedCombat, int sizeModifier)
    {
        var output = "";
        var attackBonus = (type == AttackType.Melee ? closeCombat : rangedCombat) +
                          (Mathf.FloorToInt(sizeModifier / 10.0f));
        var damageBonus = Mathf.Max(damage + sizeModifier, 0);
        output += $"**{title}** *ATT* **+{attackBonus}**, *DMG* **+{damageBonus}**, *RNG* **{range}m**, *{traits}*\n";
        return output;
    }
}

[Serializable]
public class AbilityEntry
{
    public string title;
    public ActionType type;
    public string description;
    public bool hasVariable;
    public int valueVariable;
    public string textVariable;
    
    public string UpdateOutput()
    {
        var output = "";
        output += $"**{title} [{type}] -** {description}";
        return output;
    }
}

[Serializable]
public class BeastEntry
{
    public string title;
    public string description;
    public int encumbrance;
    public CreatureType type;
    public Rarity rarity;
    [Range(0, 4)] public int perception;
    [Range(0, 4)] public int closeCombat;
    [Range(0, 4)] public int rangedCombat;
    [Range(0, 4)] public int conditioning;
    [Range(0, 4)] public int conviction;
    [Range(0, 4)] public int athletics;
    [Range(0, 4)] public int soakBonus;
    [Range(0, 3)] public int durability;
    
    public List<AttackEntry> attacks;
    public List<string> abilities;

    public int GetWounds() { return Mathf.Max(1 + conditioning + GetSizeValue(), 1); }
    public int GetSizeValue() { return Mathf.FloorToInt(encumbrance / 5.0f) -5; }
    public int GetSoakValue() { return Mathf.Max(0 + soakBonus + (Mathf.FloorToInt(GetSizeValue() / 5.0f)),0); }
    public int GetHighestAttackDamage()
    {
        var highestAttack = attacks
            .OrderByDescending(a =>
                Mathf.Max(a.damage + GetSizeValue() + (a.type == AttackType.Melee ? closeCombat : rangedCombat), 0))
            .First();
        
        return Mathf.Max(highestAttack.damage + GetSizeValue() + (highestAttack.type == AttackType.Melee ? closeCombat : rangedCombat),0);
    }
    public int GetThreatLevel()
    {
        var defence = Mathf.Max(6 + closeCombat - GetSizeValue(), 1);
        var closeCombatAttackPoints = 1 + closeCombat;
        return Mathf.FloorToInt((((defence + GetSoakValue()) * (GetWounds() + durability)) + (closeCombatAttackPoints * GetHighestAttackDamage())) / 10.0f) + 1;
    }
    private int GetDefenceValue() { return Mathf.Max(6 + closeCombat - GetSizeValue(), 0); }
    private int GetCarryingCapacity() { return encumbrance + (conditioning * (10 + (GetSizeValue() * 5))); }
    public string UpdateOutput(List<AbilityEntry> fullAbilities)
    {
        var sizeValue = GetSizeValue();
        var sizeCategory = (CreatureSize)Mathf.Min(sizeValue, 7);
        var defence = GetDefenceValue();
        var soak = GetSoakValue();
        var wounds = Mathf.Max(1 + conditioning + sizeValue, 1);
        var vigour = 6 + conditioning + sizeValue;
        var willpower = 6 + conviction + sizeValue;
        var perceptionRange = (perception + 1) * 20;
        var perceptionScore = perception + 1;
        var movementPoints = 5 + (athletics * 2) + Mathf.FloorToInt(sizeValue / 2.0f);
        var carryingCapacity = GetCarryingCapacity();
        var closeCombatAttackPoints = 1 + closeCombat;
        var rangedCombatAttackPoints = 1 + rangedCombat;
        var threatLevel = GetThreatLevel();
        
        var output = "";
        output += $"#### {title}";
        if(!string.IsNullOrEmpty(description)) output += $"\n{description}";
        output += "\nitem(";
        output += $"\n# {rarity}";
        output += $"\n## {type}";
        output += "\n-";
        output += $"\n*DEF* **{defence}**, *ARM* **{soak}**, *WND* **{wounds}{(durability>0?$"({durability})":"")}**, *VIG* **{vigour}**, *WIL* **{willpower}**";
        output += $"\n*Perception:* *Range* **{perceptionRange}m**, *Score* **{perceptionScore}**; *Initiative* **+{perception}**";
        output += $"\n*Movement Points* **{movementPoints}**; *Carrying Capacity* **{carryingCapacity}**";
        output += $"\n*Size* **{sizeCategory} [{sizeValue}]**, ENC **{encumbrance}**; *Threat level* **{threatLevel}**";
        output += $"\n#### Action Points [{Mathf.Max(closeCombatAttackPoints,rangedCombatAttackPoints)}]";
        output += "\n-";
        output += "\n \n";
        output = attacks
            .Aggregate(output, (current, attack) => 
                current + attack.UpdateOutput(closeCombat, rangedCombat, GetSizeValue()));

        output += "\n#### Abilities";
        output += "\n-";
        output = abilities
            .Select(ability => 
                fullAbilities.First(a => a.title == ability))
            .Aggregate(output, (current, data) => 
                current + $"\n{data.UpdateOutput()}\n");
        output += "\n)";
        return output;
    }
}

public class BestiaryManager : MonoBehaviour
{
    public ChapterData chapter;
    public string list;

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        chapter.UpdateOutput();
        GenerateCreatureList();
    }

    private void GenerateCreatureList()
    {
        var creatures = new List<BeastEntry>();

        foreach (var category in chapter.categories)
        {
            creatures.AddRange(category.beasts);
        }

        creatures = creatures.OrderBy(b => b.GetThreatLevel()).ToList();

        var output = "# Dangerous Encounters";
        output += "\n-";
        output += "\n/";
        output += "\nd% | Result | DL";
        output += "\n-- | --";
        for (var i = 0; i < creatures.Count; i++)
        {
            output += $"\n{i + 1} | {creatures[i].title} [{creatures[i].type} {creatures[i].rarity} {creatures[i].GetThreatLevel()}] | {Mathf.CeilToInt((i + 1) / 10.0f)}";
        }

        list = output;
    }
}
