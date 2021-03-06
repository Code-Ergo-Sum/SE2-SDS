using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;


// TODO:? Make resources (h/m/s) scale from level? Eliminate mana/stamina?
// Actor is the class from which all characters will inherit
public class Actor {

    #region Private Vars

    private string _name;
    private string _fullName;
    [XmlIgnore]
    private Title  _title;

    private bool _isAlive = true;
    private int  _level = 1;

    [XmlIgnore]
    private System.Random rand = new System.Random();


    #endregion

    #region Public Vars
    
    public List<Ability> passiveAbilities = new List<Ability>();

    public int id; //unique across all monsters and actors
    public bool isUserControllable;

    [XmlIgnore]
    public Dictionary<string, int> statusEffects = new Dictionary<string, int>();
    [XmlIgnore]
    public Slider battleHealthBar;
    [XmlIgnore]
    public Slider battleStaminaBar;
    [XmlIgnore]
    public GameObject battleDamageText;
    [XmlIgnore]
    public GameObject battleStatusEffectText;
    [XmlIgnore]
    public GameObject battleStatusEffectBackground;

    public enum hitType {
        hit,
        crit,
        miss
    }

    public string name {
        get {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    // name + title or title + name
    public string fullName {
        get {
            return _fullName;
        }
    }

    // just added this in for fun, see Title.cs (Title as in Dr. or President or .. THE UNKILLABLE)
    [XmlIgnore]
    public Title title {
        get {
            return _title;
        }
    }

    public bool isAlive {
        get {
            return _isAlive;
        }
        set
        {
            _isAlive = value;
        }
    }

    public int level {
        get {
            return _level;
        }
        set
        {
            _level = value;
        }
    }

    public Weapon weapon;

    public Resource health;

    public Resource stamina;

    [XmlIgnore]
    public Dictionary<string, Stat> stats = new Dictionary<string, Stat>();

    public Stat[] statsArray = new Stat[5];

    public Ability.damageType weakness;

    #endregion

    #region Constructor & Methods

    //simple constructor, used primarilly by Monsters
    public Actor(string name, int level, Title title = null, Resource[] resources = null, int[] statArray = null, Ability.damageType weakness = Ability.damageType.none) {
        _name = name;
        _level = level;
        this.weakness = weakness;

        _isAlive = true;

        if (title != null) { // title
            setTitle(title);
        } else { // no title
            _fullName = name;
        }

        if (resources != null) { // h/m/s specified
            health  = resources[0];
            stamina = resources[1];
        } else { // h/m/s not specified - go by this formula
            int resourceModifier = 10 * level; // no idea if this formula will be good
            health  = new Resource(resourceModifier, 0);
            stamina = new Resource(100, level * 2); //Stamina should alway max out at 100.
        }
        health.owner = this;
        stamina.owner = this;

        // setting up the default stats
        stats.Add("strength",  new Stat(1, 0));
        stats.Add("intellect", new Stat(1, 0));
        stats.Add("dexterity", new Stat(1, 0));
        stats.Add("cunning",   new Stat(1, 0));
        stats.Add("charisma",  new Stat(1, 0));

        if (statArray != null) { // stats specified
            setStatLevels(statArray);
        }

        initStatusEffects();

    }

    // Simple constructor (used primarilly by userControllables)
    public Actor() {
        decimal resourceModifier = 10; // no idea if this formula will be good
        health = new Resource(resourceModifier, 0);
        stamina = new Resource(100, 1);
        health.owner = this;
        stamina.owner = this;

        this.weakness = Ability.damageType.none;

        //Debug.Log("max stamina in player is " + stamina.maxValue);

        _isAlive = true;

        // setting up the default stats
        stats.Add("strength",  new Stat(1, 0));
        stats.Add("intellect", new Stat(1, 0));
        stats.Add("dexterity", new Stat(1, 0));
        stats.Add("cunning",   new Stat(1, 0));
        stats.Add("charisma",  new Stat(1, 0));

        initStatusEffects();
    }

    //Initializes the dictionary of status effects, sets them all to 0
    public void initStatusEffects()
    {
        statusEffects.Add("pin", 0);
        statusEffects.Add("shield", 0);
        statusEffects.Add("regen", 0);
        statusEffects.Add("confuse", 0);
        statusEffects.Add("wither", 0);
        statusEffects.Add("poison", 0);
        statusEffects.Add("float", 0);
    }

    /// <summary>
    /// Sets Actor.title with the specified Title, and applies the title to Actor.fullName
    /// </summary>
    /// <param name="newTitle">Title to apply</param>
    /// Todo: Delete setTitle
    public void setTitle(Title newTitle) {
        _title = newTitle;

        if (title.beforeName) {
            _fullName = title.text + " " + name;
        } else {
            _fullName = name + " " + title.text;
        }
    }

    /// <summary>
    /// Clears the current title, setting Actor.title to null, and Actor.fullName to Actor.name
    /// </summary>
    public void clearTitle() {
        _title = null;
        _fullName = name;
    }

    /// <summary>
    /// Sets all stats to the specified values
    /// </summary>
    /// <param name="stats">
    /// Array of ints corresponding to the Actor's stats in order:
    /// strength, intellect, dexterity, cunning, charisma 
    /// The order of stats is in alphabetical order:  charisma, cunning, dexterity, intellect, strength
    /// </param>
    public void setStatLevels(int[] newStats) {
        stats["charisma"].setLevel(newStats[0]);
        stats["cunning"].setLevel(newStats[1]);
        stats["dexterity"].setLevel(newStats[2]);
        stats["intellect"].setLevel(newStats[3]);
        stats["strength"].setLevel(newStats[4]);
    }

    /// <summary>
    /// Sets all stats to the specified value
    /// </summary>
    /// <param name="newStats">int to set ALL stat values to</param>
    public void setStatLevels(int newStats) {
        foreach (KeyValuePair<string, Stat> stat in stats) {
            stat.Value.setLevel(newStats);
        }
    }

    /// <summary>
    /// Brings the actor back to life with specified percentage of resources
    /// </summary>
    /// <param name="percentResources">
    /// Percentage of health/mana/stamina Actor is res'd with (1.0 for 100%)
    /// </param>
    public void resurrect(decimal percentResources) {
        health.setValue(health.maxValue * percentResources);
        stamina.setValue(stamina.maxValue * percentResources);

        _isAlive = true;
    }

    //Shows the animation for the monster / player dying
    //Todo: make _isAlive false After the animation is finished

    public void showDeathAnimation()
    {
        if (!this.isUserControllable)
        {
            ((Monster)this).monsterPrefab.GetComponent<Animator>().SetBool("Alive", false); //test monster death
            //Object.Destroy(((Monster)this).monsterPrefab);
        }
    }

    /// <summary>
    /// Kills the Actor, setting all resources to 0
    /// It also calls showDeathAnimation
    /// It also checks to see if the party is dead,
    /// if the party is dead it updates partyIsDead variable in player
    /// </summary>
    public virtual void kill() {
        health.setValue(0);
        stamina.setValue(0);

        showDeathAnimation();
        


        _isAlive = false;
    }

    /// <summary>
    /// Checks to see if the Actor has the specified passive ability
    /// </summary>
    /// <param name="name">Name of passive ability</param>
    /// <returns>true if Actor has the passive</returns>
    public bool hasPassive(string name) {
        foreach (Ability ab in passiveAbilities) {
            if (ab.name == name) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to damage the actor by the specified amount
    /// </summary>
    /// <param name="damageAmount">amount to damage</param>
    /// <param name="damager">Actor who is dealing the damage</param>
    /// <returns>hitType.hit, hitType.crit, or hitType.miss</returns>
    /// autoHit always hits if its set to true (used in bowAttack)
    public hitType damage(decimal damageAmount, Actor damager,  Ability.damageType damageType, bool autoHit = false) {
        System.Random ran = new System.Random();
        hitType ht;

        // temporary formula - needs balance testing
        decimal dodgeRoll = (ran.Next(0, 45));

        bool isWeak = (weakness == damageType);

        //Dodge roll for uC is based on weapon accuracy, for Monster is based on hitAccuracy
        /*if (damager.isUserControllable)
        {
            dodgeRoll = dodgeRoll + (damager.weapon.accuracy * 0.5m);
            dodgeRoll = 1000; //Temporary adjustment since we've been missing too much
        }
        else
        {
            dodgeRoll = dodgeRoll + ((Monster)damager).hitAccuracy * .05m;
        }*/
       

        if ((stats["cunning"].level) < dodgeRoll || statusEffects["pin"] != 0 || autoHit) {
            ht = hitType.hit;
            decimal critRoll = (ran.Next(0, 100) / 100m);

            // crit
            if (isWeak || (damager.stats["cunning"].modifier * 0.5m) > critRoll) {
                damageAmount *= 3;
                ht = hitType.crit;
            }

            // logic for Last Chance
            if (damager.hasPassive("Last Chance") && (damager.health.value / damager.health.maxValue) <= 0.1m) {
                damageAmount *= 3;
            }

            // logic for Iron Skin
            if (hasPassive("Iron Skin")) {
                damageAmount -= damageAmount * 0.2m;
            }

            showHitResult(damageAmount, ht);
            health.subtract(damageAmount); // ouch

            if(!damager.isUserControllable && isUserControllable)
            {
                DrawDamageLine((Monster)damager, (UserControllable) this);
            }

            if (health.value == 0) {
                kill(); // yo dead
            } else
            {
                //Logic for counter-attack
                if (damageType == Ability.damageType.melee && hasPassive("Counter Attack"))
                {
                    damager.damage(damageAmount * .5m, this, Ability.damageType.melee);
                    //Todo: show animation for physical attack occurring on the target
                }
            }

            playAttackSound(ht, damageType);

            return ht;
        }

        playAttackSound(hitType.miss, damageType);

        showHitResult(-1, hitType.miss);
        return hitType.miss; // dodged
    }

    private void playAttackSound(hitType ht, Ability.damageType dt)
    {
        if(ht == hitType.hit || ht == hitType.crit)
        {
            switch(dt)
            {
                case Ability.damageType.fire:
                    AudioControl.playSound("fire_spell");
                    break;
                case Ability.damageType.ground:
                    AudioControl.playSound("ground_spell");
                    break;
                case Ability.damageType.lightning:
                    AudioControl.playSound("lightning_spell");
                    break;
                case Ability.damageType.water:
                    AudioControl.playSound("water_spell");
                    break;
                case Ability.damageType.melee:
                case Ability.damageType.none:
                    if(ht == hitType.crit)
                    {
                        AudioControl.playSound("melee_2");
                    }
                    if(isUserControllable)
                    {
                        AudioControl.playSound("melee_1");
                    }
                    else
                    {
                        AudioControl.playSound("slash_hit");
                    }
                    break;
                case Ability.damageType.ranged:
                    AudioControl.playSound("arrow_hit");
                    break;
            }
        } else
        {
            if(dt == Ability.damageType.melee || dt == Ability.damageType.none)
            {
                AudioControl.playSound("slash_miss");
            }
            else
            {
                AudioControl.playSound("arrow_miss");
            }
        }
    }

    //Shows how much health was lost after taking damage
    public void showHitResult(decimal damageAmount, hitType ht)
    {
        GameObject dmgText = (GameObject) GameObject.Instantiate(battleDamageText, battleDamageText.transform, true);
        
        string formattedDmg = damageAmount.ToString("######.##");

        if (ht == hitType.miss)
        {
            dmgText.GetComponent<Text>().text = MLH.tr("MISS!");
        }
        else if(ht == hitType.crit)
        {
            dmgText.GetComponent<Text>().text = "" + formattedDmg +  MLH.tr(" CRIT!");
        } else
        {
            dmgText.GetComponent<Text>().text = "" + formattedDmg;
        }

        dmgText.transform.SetParent(GameObject.Find("BattleCanvas").transform);
        DamageFloatUpward d = dmgText.GetComponent<DamageFloatUpward>();
        dmgText.SetActive(true);

        if (d != null) {
            d.floatUpThenDisappear(dmgText);
        }
    }

    //Draws the damage line for when we get damaged.
    public void DrawDamageLine(Monster damager, UserControllable damagee)
    {
        Vector3 pointA = damager.monsterPrefab.transform.localPosition;
        Vector3 pointB = damagee.battleObj.transform.localPosition;

        GameObject imageRectTransform = Resources.Load("DamageLine") as GameObject;
        imageRectTransform = GameObject.Instantiate(imageRectTransform, imageRectTransform.transform.position, imageRectTransform.transform.rotation) as GameObject;
        imageRectTransform.transform.SetParent(GameObject.Find("BattleCanvas").transform, false);

        pointB = new Vector3(pointB.x, pointB.y + 50, pointB.z);

        Vector3 differenceVector = pointB - pointA;


        imageRectTransform.gameObject.AddComponent<Image>();

        imageRectTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(differenceVector.magnitude, 1);
        imageRectTransform.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
        imageRectTransform.transform.localPosition = pointA;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;

        imageRectTransform.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, angle);

        GameObject.Destroy(imageRectTransform, (float).75);
    }

    //Updates the status effect box so that it reflects the actor's status effects
    public void updateStatusEffectBox()
    {
        string effects = "";
        effects += (statusEffects["pin"] == 0 ? "" : "pin" + "\n");
        effects += (statusEffects["shield"] == 0 ? "" : "Shield" + (statusEffects["shield"] == 1 ? "" : " x" + statusEffects["shield"]) + "\n");
        effects += (statusEffects["wither"] == 0 ? "" : "Slow" + (statusEffects["wither"] == 1 ? "" : " x" + statusEffects["wither"]) + "\n");
        effects += (statusEffects["regen"] == 0 ? "" : "Regen" + (statusEffects["regen"] == 1 ? "" : " x" + statusEffects["regen"]) + "\n");
        effects += (statusEffects["confuse"] == 0 ? "" : "Confused" + (statusEffects["confuse"] == 1 ? "" : "x" + statusEffects["confuse"]) + "\n");
        effects += (statusEffects["poison"] == 0 ? "" : "Poison" + (statusEffects["poison"] == 1 ? "" : "x" + statusEffects["poison"]) + "\n");
        effects += (statusEffects["float"] == 0 ? "" : "Armor" + (statusEffects["float"] == 1 ? "" : " x" + statusEffects["float"]) + "\n");

        battleStatusEffectText.GetComponent<Text>().text = effects;

    }

    /// <summary>
    /// Heals the Actor by the specified amount
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    public void heal(decimal healAmount) {
        health.add(healAmount);
    }

    /// <summary>
    /// Drains the specified amount of stamina
    /// </summary>
    /// <param name="amount">Amount of stamina to drain</param>
    public void drainStamina(decimal amount) {
        stamina.subtract(amount);
    }

    #endregion
}
