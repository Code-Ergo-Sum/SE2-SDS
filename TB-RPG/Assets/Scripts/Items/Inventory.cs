using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

// holds Items
// TODO: add image field
public class Inventory {

    #region Private Vars

    [XmlIgnore]
    private Player _player;
    private string _name;
    private decimal _weightCap;
    private decimal _weight = 0;

    public decimal gold;

    #endregion

    #region Public Vars

    public string name { // small pouch, huge backpack, etc..
        get {
            return _name;
        }
    }

    public decimal weightCap { // how much it can hold
        get {
            return _weightCap;
        }
        set
        {
            _weightCap = value;
        }
    }

    public decimal weight { // current weight
        get {
            return _weight;
        }
        set
        {
            _weight = value;
        }
    }

    [XmlIgnore]
    public Player player {
        get {
            return _player;
        }
    }

    // actual list of items in inventory
    public List<Item> items = new List<Item>();

    #endregion

    #region Constructors & Methods

    public Inventory() { }

    public Inventory(Player player, string name, decimal weightCap) {
        _player = player;
        _name   = name;
        _weightCap = weightCap;
    }

    public Inventory(Player player, string name, decimal weightCap, List<Item> items) {
        _player = player;
        _name   = name;
        _weightCap = weightCap;
        this.items = items;

        calcWeight();
    }
    

    public void addGold(decimal amount)
    {
        gold += amount;
    }

    private void calcWeight() {
        _weight = 0;

        for (int x = 0; x < items.Count; x++) {
            _weight += items[x].weight;
        }
    }

    /// <summary>
    /// Attempts to add an item to Inventory.items
    /// </summary>
    /// <param name="item">Item to add to the Inventory</param>
    /// <returns>True if there is room to add the item</returns>
    /// <remarks>USE THIS METHOD OVER Inventory.items.Add(Item)!!!</remarks>
    public bool addItem(Item item) {
        decimal newWeight = weight + item.weight;

        if (newWeight > weightCap) { // too heavy
            return false; // don't add it
        } else { 
            // there's room
            items.Add(item); // add it
            calcWeight(); //update weight


            GameObject scrollView = null;

            //Figure out which scrollview we want to add the item to
            if (_name == "shop")
            {
                switch(item.itemType)
                {
                    case Item.itemTypes.weapon:
                        scrollView = ShopInventoryScript.instance.WeaponsBuyScrollView;
                        break;
                    case Item.itemTypes.loot:
                        Debug.Log("Error: Trying to add loot to shop.");
                        break;
                    case Item.itemTypes.abilityItem:
                        scrollView = ShopInventoryScript.instance.ItemBuyScrollView;
                        break;
                }
            }
            else
            {
                switch (item.itemType)
                {
                    case Item.itemTypes.weapon:
                        scrollView = ShopInventoryScript.instance.WeaponScrollView;
                        break;
                    case Item.itemTypes.loot:
                        scrollView = ShopInventoryScript.instance.LootScrollView;
                        break;
                    case Item.itemTypes.abilityItem:
                        scrollView = ShopInventoryScript.instance.ItemScrollView;
                        break;
                }
            }

            //Load the gameObject from prefab
            switch (item.itemType)
            {
                case Item.itemTypes.weapon:
                    item.invObject = Resources.Load("InventoryWeapon") as GameObject;
                    break;
                case Item.itemTypes.loot:
                    item.invObject = Resources.Load("InventoryItem") as GameObject;
                    break;
                case Item.itemTypes.abilityItem:
                    item.invObject = Resources.Load("InventoryItem") as GameObject;
                    break;
            }

            //Instantiate the gameObject, and set its position in the scene
            item.invObject = GameObject.Instantiate(item.invObject, item.invObject.transform.position, item.invObject.transform.rotation) as GameObject;
            Transform tr = scrollView.transform;
            GameObject content = tr.FindChild("Viewport").FindChild("Content").gameObject;
            item.invObject.transform.SetParent(content.transform, false);

            //Set the gameObject's texts to match the item
            tr = item.invObject.transform;
            tr.Find("Name").GetComponent<Text>().text = item.name;
            var name = item.name;
            if (item.itemType == Item.itemTypes.weapon) {
                switch(((Weapon)item).weaponClass)
                {
                    case Weapon.WeaponClass.Magic:
                        name += "\r\n(Mage Weapon)";
                        break;
                    case Weapon.WeaponClass.Melee:
                        name += "\r\n(Fighter Weapon)";
                        break;
                    case Weapon.WeaponClass.Ranged:
                        name += "\r\n(Rogue Weapon)";
                        break;
                }
                tr.Find("Level").GetComponent<Text>().text = ((Weapon)item).level.ToString();
            }

            tr.Find("Name").GetComponent<Text>().text = name;
            tr.Find("Cost").GetComponent<Text>().text = item.value.ToString();
            tr.Find("Weight").GetComponent<Text>().text = item.weight.ToString("00");
            //Setup the buysell button
            GameObject buySell = tr.FindChild("BuySellButton").gameObject;
            if (_name == "shop")
            {
                buySell.GetComponentInChildren<Text>().text = "BUY";
                buySell.GetComponent<Button>().onClick.AddListener(delegate { ShopInventoryScript.instance.buyItem(item); });
            }
            else
            {
                buySell.GetComponentInChildren<Text>().text = "SELL";
                buySell.GetComponent<Button>().onClick.AddListener(delegate { ShopInventoryScript.instance.sellItem(item); });
            }

            //Setup the equip button
            if(item.itemType == Item.itemTypes.weapon)
            {
                GameObject equip = tr.FindChild("EquipButton").gameObject;
                if (_name == "shop")
                {
                    equip.SetActive(false);
                }
                else
                {
                    equip.SetActive(true);
                }
                equip.GetComponent<Button>().onClick.AddListener(delegate { ShopInventoryScript.instance.equipWeapon((Weapon)(item)); });

            }
            return true;
        }
    }
    
    /// <summary>
    /// Deletes an item from Inventory.items
    /// </summary>
    /// <param name="item">Item to remove</param>
    /// <remarks>USE THIS METHOD OVER Inventory.items.Remove(Item)!!!</remarks>
    public void deleteItem(Item item) {
        _weight -= item.weight;
        items.Remove(item);
    }

    /// <summary>
    /// Sells an Item, updating the player's gold amount
    /// </summary>
    /// <param name="item">Item to sell</param>
    public void sellItem(Item item) {
        //Note: loot items are sold for +200% if you have ValueCrafter
        foreach(var uC in GameMaster.instance.thePlayer.theParty)
        {
            if (uC != null)
            {
                if (uC.hasPassive("Value Crafter"))
                {
                    player.inventory.gold += item.value * 2;
                    return;
                }
            }
        }
        //Still here? Sell it for the regular price.
        player.inventory.gold += item.value;
        player.inventory.weight -= item.weight;
        items.Remove(item);
    }

    #endregion
}
