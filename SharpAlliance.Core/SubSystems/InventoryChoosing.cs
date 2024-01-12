using System;

namespace SharpAlliance.Core;

public class InventoryChoosing
{
    public static void InitArmyGunTypes()
    {
        ARMY_GUN_CHOICE_TYPE[] pGunChoiceTable;
        int uiGunLevel;
        int uiChoice;
        Items bItemNo;

        // depending on selection of the gun nut option
        if (gGameOptions.fGunNut)
        {
            // use table of extended gun choices
            pGunChoiceTable = (gExtendedArmyGunChoices);
        }
        else
        {
            // use table of regular gun choices
            pGunChoiceTable = (gRegularArmyGunChoices);
        }

        // for each gun category
        for (uiGunLevel = 0; uiGunLevel < ARMY_GUN_LEVELS; uiGunLevel++)
        {
            // choose one the of the possible gun choices to be used by the army for this game & store it
            uiChoice = Globals.Random.GetRandom(pGunChoiceTable[uiGunLevel].ubChoices);
            bItemNo = pGunChoiceTable[uiGunLevel].bItemNo[uiChoice];
            //AssertMsg(bItemNo != -1, "Invalid army gun choice in table");
            gStrategicStatus.ubStandardArmyGunIndex[uiGunLevel] = bItemNo;
        }

        // set all flags that track whether this weapon type has been dropped before to FALSE
        for (Items ubWeapon = 0; ubWeapon < Items.MAX_WEAPONS; ubWeapon++)
        {
            gStrategicStatus.fWeaponDroppedAlready[ubWeapon] = false;
        }

        // avoid auto-drops for the gun class with the crappiest guns in it
        MarkAllWeaponsOfSameGunClassAsDropped(Items.SW38);
    }

    private static void MarkAllWeaponsOfSameGunClassAsDropped(Items usWeapon)
    {
        int bGunClass;
        int uiLoop;


        // mark that item itself as dropped, whether or not it's part of a gun class
        gStrategicStatus.fWeaponDroppedAlready[usWeapon] = true;

        bGunClass = GetWeaponClass(usWeapon);

        // if the gun belongs to a gun class (mortars, GLs, LAWs, etc. do not and are handled independently)
        if (bGunClass != -1)
        {
            // then mark EVERY gun in that class as dropped
            for (uiLoop = 0; uiLoop < gExtendedArmyGunChoices[bGunClass].ubChoices; uiLoop++)
            {
                gStrategicStatus.fWeaponDroppedAlready[gExtendedArmyGunChoices[bGunClass].bItemNo[uiLoop]] = true;
            }
        }
    }

    private static int GetWeaponClass(Items usGun)
    {
        int uiGunLevel, uiLoop;

        // always use the extended list since it contains all guns...
        for (uiGunLevel = 0; uiGunLevel < ARMY_GUN_LEVELS; uiGunLevel++)
        {
            for (uiLoop = 0; uiLoop < gExtendedArmyGunChoices[uiGunLevel].ubChoices; uiLoop++)
            {
                if (gExtendedArmyGunChoices[uiGunLevel].bItemNo[uiLoop] == usGun)
                {
                    return (uiGunLevel);
                }
            }
        }

        return (-1);
    }

    private static ARMY_GUN_CHOICE_TYPE[] gRegularArmyGunChoices =
    {	// INDEX		CLASS				 #CHOICES
    	new(/* 0 - lo pistols			*/	2,  Items.SW38,         Items.DESERTEAGLE,      Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 1 - hi pistols			*/	2,  Items.GLOCK_17,     Items.BERETTA_93R,      Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 2 - lo SMG/shotgun	*/  	2,  Items.M870,         Items.MP5K,             Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 3 - lo rifles			*/	1,  Items.MINI14,       Items.UNSET,            Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 4 - hi SMGs				*/	2,  Items.MAC10,        Items.COMMANDO,         Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 5 - med rifles  		*/	    1,  Items.G41,          Items.UNSET,            Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 6 - sniper rifles	*/	    1,  Items.M24,          Items.UNSET,            Items.UNSET,    Items.UNSET, Items.UNSET),
        new(/* 7 - hi rifles			*/	2,  Items.M14,          Items.C7,               Items.UNSET,    Items.UNSET, Items.UNSET),
        new( /* 8 - best rifle			*/	1,  Items.FNFAL,        Items.UNSET,            Items.UNSET,    Items.UNSET, Items.UNSET),
        new( /* 9 - machine guns		*/	1,  Items.MINIMI,       Items.UNSET,            Items.UNSET,    Items.UNSET, Items.UNSET),
        new( /* 10- rocket rifle		*/	2,  Items.ROCKET_RIFLE, Items.MINIMI,           Items.UNSET,    Items.UNSET, Items.UNSET),
    };

    private static ARMY_GUN_CHOICE_TYPE[] gExtendedArmyGunChoices =
    {	// INDEX		CLASS				 #CHOICES
        new( /* 0 - lo pistols		*/	5,  Items.SW38,               Items.BARRACUDA,      Items.DESERTEAGLE,    Items.GLOCK_17, Items.M1911),
        new( /* 1 - hi pist/shtgn	*/	4,  Items.GLOCK_18,           Items.BERETTA_93R,    Items.BERETTA_92F,    Items.M870,     Items.UNSET),
        new( /* 2 - lo SMGs/shtgn	*/	5,  Items.TYPE85,             Items.THOMPSON,       Items.MP53,           Items.MP5K,     Items.SPAS15),
        new( /* 3 - lo rifles    	*/	2,  Items.MINI14,             Items.SKS,            Items.UNSET,          Items.UNSET,    Items.UNSET),
        new( /* 4 - hi SMGs		    */	3,  Items.MAC10,              Items.AKSU74,         Items.COMMANDO,       Items.UNSET,    Items.UNSET),
        new( /* 5 - med rifles 	    */	4,  Items.AKM,                Items.G3A3,           Items.G41,            Items.AK74,     Items.UNSET),
        new( /* 6 - sniper rifles	*/	2,  Items.DRAGUNOV,           Items.M24,            Items.UNSET,          Items.UNSET,    Items.UNSET),
        new(	/* 7 - hi rifles	*/	4,  Items.FAMAS,              Items.M14,            Items.AUG,            Items.C7,       Items.UNSET),
        new( /* 8 - best rifle		*/	1,  Items.FNFAL,              Items.UNSET,          Items.UNSET,          Items.UNSET,    Items.UNSET),
        new( /* 9 - machine guns	*/	3,  Items.MINIMI,             Items.RPK74,          Items.HK21E,          Items.UNSET,    Items.UNSET),
        new( /* 10- rocket rifle	*/	4,  Items.ROCKET_RIFLE,       Items.ROCKET_RIFLE,   Items.RPK74,          Items.HK21E,    Items.UNSET),
    };
}

    public class ARMY_GUN_CHOICE_TYPE
    {
        public ARMY_GUN_CHOICE_TYPE(int ubChoices, Items item1, Items item2, Items item3, Items item4, Items item5)
        {
            this.ubChoices = ubChoices;
            bItemNo[0] = item1;
            bItemNo[1] = item2;
            bItemNo[2] = item3;
            bItemNo[3] = item4;
            bItemNo[4] = item5;
        }

        public int ubChoices { get; }                        // how many valid choices there are in this category
        public Items[] bItemNo { get; } = new Items[5];           // room for up to 5 choices of gun in each category
    }

