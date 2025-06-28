using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum ItemType
{
    CloseCombatWeapon,
    RangedWeapon,
    Armour,
    Gear,
    Tool,
    Consumable
}

[Serializable]
public class ItemTraitInstance
{
    public string name;
    public string variableValue;
}

[Serializable]
public class ItemTrait
{
    public string name;
    public string description;
    public bool hasNumVariable;
    public bool hasStringVariable;

    public string GetTrait()
    {
        return $"###{name}{(hasNumVariable || hasStringVariable?" [X]":"")}\n{description}\n";
    }
}

[Serializable]
public class Item
{
    public string name;
    public string description;
    public int encumbrance;
    public int cost;
    public Rarity rarity;
    public ItemType type;
    public List<ItemTraitInstance> traits;

    public virtual string GetTableHeader()
    {
        return "Name | ENC | Cost | Rarity | Traits \n-- | --\n";
    }

    public virtual string GetItem()
    {
        return $"{name} | {encumbrance} | {cost}c | {rarity} | {traits.Aggregate("", (prev, next) => $"{prev}{next.name}{(!string.IsNullOrEmpty(next.variableValue) ? $" [{next.variableValue}]" : "")}, ")}\n";
    }
}

[Serializable]
public class Weapon : Item
{
    public int range;
    public int damage;
    
    public override string GetTableHeader()
    {
        return "Name | RNG | DMG | ENC | Cost | Rarity | Traits \n-- | --\n";
    }

    public override string GetItem()
    {
        return $"{name} | {range}m | +{damage} | {encumbrance} | {cost}c | {rarity} | {traits.Aggregate("", (prev, next) => $"{prev}{next.name}{(!string.IsNullOrEmpty(next.variableValue) ? $" [{next.variableValue}]" : "")}, ")}\n";
    }
}

[Serializable]
public class Armour : Item
{
    public int armour;
    public int durability;
    
    public override string GetTableHeader()
    {
        return "Name | ARM | DUR | ENC | Cost | Rarity | Traits \n-- | --\n";
    }

    public override string GetItem()
    {
        return $"{name} | {armour} | {durability} | {encumbrance} | {cost}c | {rarity} | {traits.Aggregate("", (prev, next) => $"{prev}{next.name}{(!string.IsNullOrEmpty(next.variableValue) ? $" [{next.variableValue}]" : "")}, ")}\n";
    }
}

[Serializable]
public class Gear : Item
{
    public string slot;
    public int capacity;
    
    public override string GetTableHeader()
    {
        return "Name | Slot | CAP | ENC | Cost | Rarity | Traits \n-- | --\n";
    }

    public override string GetItem()
    {
        return $"{name} | {slot} | {capacity} | {encumbrance} | {cost}c | {rarity} | {traits.Aggregate("", (prev, next) => $"{prev}{next.name}{(!string.IsNullOrEmpty(next.variableValue) ? $" [{next.variableValue}]" : "")}, ")}\n";
    }
}

[Serializable]
public class Consumable : Item
{
    public int toxicity;
    
    public override string GetTableHeader()
    {
        return "Name | Toxicity | ENC | Cost | Rarity | Traits \n-- | --\n";
    }

    public override string GetItem()
    {
        return $"{name} | {toxicity} | {encumbrance} | {cost}c | {rarity} | {traits.Aggregate("", (prev, next) => $"{prev}{next.name}{(!string.IsNullOrEmpty(next.variableValue) ? $" [{next.variableValue}]" : "")}, ")}\n";
    }
}

[Serializable]
public class ItemCategory
{
    public string name;
    public string description;
    public List<Item> items;
    public List<Entry> entries;

    public virtual string GetTable()
    {
        return items.Aggregate(items[0].GetTableHeader(), (prev, next) => $"{prev}{next.GetItem()}");
    }
}

[Serializable]
public class WeaponCategory : ItemCategory
{
    public List<Weapon> weapons;

    public override string GetTable()
    {
        return weapons.Aggregate(weapons[0].GetTableHeader(), (prev, next) => $"{prev}{next.GetItem()}");
    }
}

[Serializable]
public class ArmourCategory : ItemCategory
{
    public List<Armour> armours;

    public override string GetTable()
    {
        return armours.Aggregate(armours[0].GetTableHeader(), (prev, next) => $"{prev}{next.GetItem()}");
    }
}

[Serializable]
public class GearCategory : ItemCategory
{
    public List<Gear> gear;

    public override string GetTable()
    {
        return gear.Aggregate(gear[0].GetTableHeader(), (prev, next) => $"{prev}{next.GetItem()}");
    }
}

[Serializable]
public class ConsumableCategory : ItemCategory
{
    public List<Consumable> consumables;

    public override string GetTable()
    {
        return consumables.Aggregate(consumables[0].GetTableHeader(), (prev, next) => $"{prev}{next.GetItem()}");
    }
}
public class EquipmentManager : MonoBehaviour
{
    public List<ItemTrait> traits;
    public List<WeaponCategory> weaponCategories;
    public List<ItemCategory> itemCategories;
    public List<ArmourCategory> armourCategories;
    public List<GearCategory> gearCategories;
    public List<ConsumableCategory> consumableCategories;
    public string output;

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        output = "";
        foreach (var weaponCategory in weaponCategories)
        {
            output += weaponCategory.entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
            output = output.Replace("[TABLE]", weaponCategory.GetTable());
        }
        
        foreach (var itemCategory in itemCategories)
        {
            output += itemCategory.entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
            output = output.Replace("[TABLE]", itemCategory.GetTable());
        }
        
        foreach (var armourCategory in armourCategories)
        {
            output += armourCategory.entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
            output = output.Replace("[TABLE]", armourCategory.GetTable());
        }
        
        foreach (var gearCategory in gearCategories)
        {
            output += gearCategory.entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
            output = output.Replace("[TABLE]", gearCategory.GetTable());
        }
        
        foreach (var consumableCategory in consumableCategories)
        {
            output += consumableCategory.entries.Aggregate("", (prev, next) => $"{prev}{next.GetEntry()}");
            output = output.Replace("[TABLE]", consumableCategory.GetTable());
        }
        
        output += "\n=\n";
        output += "##Traits\n-\n/\n";
        var index = 0;
        
        foreach (var trait in traits.OrderBy(t => t.name))
        {
            output += trait.GetTrait();
            index++;
            switch (index)
            {
                case 10 or 30:
                    output += "\n|\n";
                    break;
                case 20 or 40:
                    output += "\n=\n";
                    break;
            }
        }
    }
}
