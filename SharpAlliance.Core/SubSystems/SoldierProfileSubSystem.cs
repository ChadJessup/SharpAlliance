using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class SoldierProfileSubSystem
{
    private readonly ILogger<SoldierProfileSubSystem> logger;
    private readonly IFileManager fileManager;
    private readonly DialogControl dialogs;

    private readonly TownReputations townReputations;
    private readonly Cars cars;
    private int gubNumTerrorists = 0;
    private Random rnd;

    public SoldierProfileSubSystem(
        ILogger<SoldierProfileSubSystem> logger,
        IFileManager fileManager,
        TownReputations townRep,
        Cars carPortraits)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.rnd = Globals.Random;
        this.townReputations = townRep;
        this.cars = carPortraits;
    }

    public static SOLDIERTYPE? FindSoldierByProfileID(NPCID ubProfileID, bool fPlayerMercsOnly)
    {
        int ubLoop, ubLoopLimit;
        SOLDIERTYPE? pSoldier;

        if (fPlayerMercsOnly)
        {
            ubLoopLimit = Globals.gTacticalStatus.Team[0].bLastID;
        }
        else
        {
            ubLoopLimit = Globals.MAX_NUM_SOLDIERS;
        }

        for (ubLoop = 0, pSoldier = Globals.MercPtrs[0]; ubLoop < ubLoopLimit; ubLoop++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.ubProfile == ubProfileID)
            {
                return (pSoldier);
            }
        }
        return (null);
    }

    public static int WhichHated(NPCID ubCharNum, MERCPROFILESTRUCT ubHated)
    {
        MERCPROFILESTRUCT? pProfile;
        int bLoop;

        pProfile = (gMercProfiles[ubCharNum]);

        for (bLoop = 0; bLoop < 3; bLoop++)
        {
            if (pProfile.bHated[bLoop] == ubHated)
            {
                return (bLoop);
            }
        }

        return (-1);
    }

    public bool LoadMercProfiles()
    {
        //	FILE *fptr;
        Stream fptr;
        string pFileName = "BINARYDATA\\Prof.dat";
        int uiLoop, uiLoop2, uiLoop3;
        Items usItem, usNewGun, usAmmo, usNewAmmo;
        int uiNumBytesRead;


        fptr = FileManager.FileOpen(pFileName, FileAccess.Read, fDeleteOnClose: false);
        //if (!fptr)
        //{
        //    this.logger.LogDebug(LoggingEventId.JA2, $"FAILED to LoadMercProfiles from file {pFileName}");
        //    return false;
        //}

        for (uiLoop = 0; uiLoop < NUM_PROFILES; uiLoop++)
        {
            var npcId = (NPCID)uiLoop;

            //if (this.fileManager.JA2EncryptedFileRead(fptr, gMercProfiles[npcId]))
            //{
            //    this.logger.LogDebug(LoggingEventId.JA2, $"FAILED to Read Merc Profiles from File {uiLoop} {pFileName}");
            //    this.fileManager.FileClose(fptr);
            //    return false;
            //}

            //if the Dialogue exists for the merc, allow the merc to be hired
            // TODO: figure out circular dependency
            //if (this.DialogueDataFileExistsForProfile(uiLoop, 0, false, out var _))
            //{
            //    gMercProfiles[npcId].bMercStatus = 0;
            //}
            //else
            //{
            //    gMercProfiles[npcId].bMercStatus = MercStatus.MERC_HAS_NO_TEXT_FILE;
            //}

            // if the merc has a medical deposit
            if (Globals.gMercProfiles[npcId].bMedicalDeposit > 0)
            {
                var profile = Globals.gMercProfiles[npcId];
                Globals.gMercProfiles[npcId].sMedicalDepositAmount = this.CalcMedicalDeposit(ref profile);
            }
            else
            {
                Globals.gMercProfiles[npcId].sMedicalDepositAmount = 0;
            }

            // ATE: New, face display indipendent of ID num now
            // Setup face index value
            // Default is the ubCharNum
            Globals.gMercProfiles[npcId].ubFaceIndex = uiLoop;

            if (!Globals.gGameOptions.GunNut)
            {
                // CJC: replace guns in profile if they aren't available
                for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
                {
                    usItem = Globals.gMercProfiles[npcId].inv[uiLoop2];

                    if (Globals.Item[usItem].usItemClass.HasFlag(IC.GUN) && ItemSubSystem.ExtendedGunListGun(usItem))
                    {
                        usNewGun = ItemSubSystem.StandardGunListReplacement(usItem);
                        if (usNewGun != Items.NONE)
                        {
                            Globals.gMercProfiles[npcId].inv[uiLoop2] = usNewGun;

                            // must search through inventory and replace ammo accordingly
                            for (uiLoop3 = 0; uiLoop3 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop3++)
                            {
                                usAmmo = Globals.gMercProfiles[npcId].inv[uiLoop3];
                                if (Globals.Item[usAmmo].usItemClass.HasFlag(IC.AMMO))
                                {
                                    usNewAmmo = ItemSubSystem.FindReplacementMagazineIfNecessary(usItem, usAmmo, usNewGun);
                                    if (usNewAmmo != Items.NONE)
                                    {
                                        // found a new magazine, replace...
                                        Globals.gMercProfiles[npcId].inv[uiLoop3] = usNewAmmo;
                                    }
                                }
                            }
                        }
                    }
                }
            } // end of if not gun nut

            //ATE: Calculate some inital attractiveness values for buddy's inital equipment...
            // Look for gun and armour
            Globals.gMercProfiles[npcId].bMainGunAttractiveness = -1;
            Globals.gMercProfiles[npcId].bArmourAttractiveness = -1;

            for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
            {
                var npcId2 = (NPCID)uiLoop2;
                usItem = Globals.gMercProfiles[npcId2].inv[uiLoop2];

                if (usItem != Items.NONE)
                {
                    // Check if it's a gun
                    if (Globals.Item[usItem].usItemClass.HasFlag(IC.GUN))
                    {
                        Globals.gMercProfiles[npcId].bMainGunAttractiveness = WeaponTypes.Weapon[usItem].ubDeadliness;
                    }

                    // If it's armour
                    if (Globals.Item[usItem].usItemClass.HasFlag(IC.ARMOUR))
                    {
                        Globals.gMercProfiles[npcId].bArmourAttractiveness = WeaponTypes.Armour[Globals.Item[usItem].ubClassIndex].ubProtection;
                    }
                }
            }


            // OK, if we are a created slot, this will get overriden at some time..

            //add up the items the merc has for the usOptionalGearCost 
            Globals.gMercProfiles[npcId].usOptionalGearCost = 0;
            for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
            {
                if (Globals.gMercProfiles[npcId].inv[uiLoop2] != Items.NONE)
                {
                    //get the item
                    usItem = Globals.gMercProfiles[npcId].inv[uiLoop2];

                    //add the cost
                    Globals.gMercProfiles[npcId].usOptionalGearCost += Globals.Item[usItem].usPrice;
                }
            }

            //These variables to get loaded in
            Globals.gMercProfiles[npcId].fUseProfileInsertionInfo = false;
            Globals.gMercProfiles[npcId].sGridNo = 0;

            // ARM: this is also being done inside the profile editor, but put it here too, so this project's code makes sense
            Globals.gMercProfiles[npcId].bHatedCount[0] = Globals.gMercProfiles[npcId].bHatedTime[0];
            Globals.gMercProfiles[npcId].bHatedCount[1] = Globals.gMercProfiles[npcId].bHatedTime[1];
            Globals.gMercProfiles[npcId].bLearnToHateCount = Globals.gMercProfiles[npcId].bLearnToHateTime;
            Globals.gMercProfiles[npcId].bLearnToLikeCount = Globals.gMercProfiles[npcId].bLearnToLikeTime;
        }

        // SET SOME DEFAULT LOCATIONS FOR STARTING NPCS

        FileManager.FileClose(fptr);

        // decide which terrorists are active
        this.DecideActiveTerrorists();

        // initialize mercs' status
        this.StartSomeMercsOnAssignment();

        // initial recruitable mercs' reputation in each town
        this.townReputations.InitializeProfilesForTownReputation();

        EditScreen.gfProfileDataLoaded = true;

        // car portrait values
        this.cars.LoadCarPortraitValues();

        return true;
    }

    private void StartSomeMercsOnAssignment()
    {
        NPCID uiCnt;
        MERCPROFILESTRUCT pProfile;
        int uiChance;

        // some randomly picked A.I.M. mercs will start off "on assignment" at the beginning of each new game
        for (uiCnt = 0; uiCnt < (NPCID)AIM_AND_MERC_MERCS; uiCnt++)
        {
            pProfile = Globals.gMercProfiles[uiCnt];

            // calc chance to start on assignment
            uiChance = 5 * pProfile.bExpLevel;

            if (this.rnd.GetRandom(100) < uiChance)
            {
                pProfile.bMercStatus = MercStatus.MERC_WORKING_ELSEWHERE;

                // 1-(6 to 11) days
                pProfile.uiDayBecomesAvailable = 1 + this.rnd.Next(0, 6 + (pProfile.bExpLevel / 2));
            }
            else
            {
                pProfile.bMercStatus = MercStatus.MERC_OK;
                pProfile.uiDayBecomesAvailable = 0;
            }

            pProfile.uiPrecedentQuoteSaid = 0;
            pProfile.ubDaysOfMoraleHangover = 0;
        }
    }

    private int CalcMedicalDeposit(ref MERCPROFILESTRUCT profile)
    {
        int usDeposit;

        // this rounds off to the nearest hundred
        usDeposit = ((5 * this.CalcCompetence(ref profile)) + 50) / 100 * 100;

        return usDeposit;
    }

    private int CalcCompetence(ref MERCPROFILESTRUCT profile)
    {
        int uiStats = 0, uiSkills, uiActionPoints, uiSpecialSkills;
        int usCompetence;

        // count life twice 'cause it's also hit points
        // mental skills are halved 'cause they're actually not that important within the game
//        uiStats = ((2 * profile.bLifeMax) + profile.bStrength + profile.bAgility + profile.bDexterity + ((profile.bLeadership + profile.bWisdom) / 2)) / 3;

        // marksmanship is very important, count it double
        uiSkills = (int)((2 * (Math.Pow(profile.bMarksmanship, 3) / 10000)) +
                            1.5 * (Math.Pow(profile.bMedical, 3) / 10000) +
                            (Math.Pow(profile.bMechanical, 3) / 10000) +
                            (Math.Pow(profile.bExplosive, 3) / 10000));

        // action points
        uiActionPoints = 5 + ((10 * profile.bExpLevel +
                              3 * profile.bAgility +
                              2 * profile.bLifeMax +
                              2 * profile.bDexterity + 20) / 40);


        // count how many he has, don't care what they are
        uiSpecialSkills = ((profile.bSkillTrait != 0) ? 1 : 0) + ((profile.bSkillTrait2 != 0) ? 1 : 0);

        usCompetence = (int)(Math.Pow(profile.bExpLevel, 0.2) * uiStats * uiSkills * (uiActionPoints - 6) * (1 + (0.05 * (float)uiSpecialSkills)) / 1000);

        // this currently varies from about 10 (Flo) to 1200 (Gus)
        return usCompetence;
    }
    
    public static SOLDIERTYPE? SwapLarrysProfiles(SOLDIERTYPE? pSoldier)
    {
        NPCID ubSrcProfile;
        NPCID ubDestProfile;
        MERCPROFILESTRUCT? pNewProfile;

        ubSrcProfile = pSoldier.ubProfile;
        if (ubSrcProfile == NPCID.LARRY_NORMAL)
        {
            ubDestProfile = NPCID.LARRY_DRUNK;
        }
        else if (ubSrcProfile == NPCID.LARRY_DRUNK)
        {
            ubDestProfile = NPCID.LARRY_NORMAL;
        }
        else
        {
            // I don't think so!
            return (pSoldier);
        }

        pNewProfile = gMercProfiles[ubDestProfile];
        pNewProfile.ubMiscFlags2 = gMercProfiles[ubSrcProfile].ubMiscFlags2;
        pNewProfile.ubMiscFlags = gMercProfiles[ubSrcProfile].ubMiscFlags;
        pNewProfile.sSectorX = gMercProfiles[ubSrcProfile].sSectorX;
        pNewProfile.sSectorY = gMercProfiles[ubSrcProfile].sSectorY;
        pNewProfile.uiDayBecomesAvailable = gMercProfiles[ubSrcProfile].uiDayBecomesAvailable;
        pNewProfile.usKills = gMercProfiles[ubSrcProfile].usKills;
        pNewProfile.usAssists = gMercProfiles[ubSrcProfile].usAssists;
        pNewProfile.usShotsFired = gMercProfiles[ubSrcProfile].usShotsFired;
        pNewProfile.usShotsHit = gMercProfiles[ubSrcProfile].usShotsHit;
        pNewProfile.usBattlesFought = gMercProfiles[ubSrcProfile].usBattlesFought;
        pNewProfile.usTimesWounded = gMercProfiles[ubSrcProfile].usTimesWounded;
        pNewProfile.usTotalDaysServed = gMercProfiles[ubSrcProfile].usTotalDaysServed;
        pNewProfile.bResigned = gMercProfiles[ubSrcProfile].bResigned;
        pNewProfile.bActive = gMercProfiles[ubSrcProfile].bActive;
        pNewProfile.fUseProfileInsertionInfo = gMercProfiles[ubSrcProfile].fUseProfileInsertionInfo;
        pNewProfile.sGridNo = gMercProfiles[ubSrcProfile].sGridNo;
        pNewProfile.ubQuoteActionID = gMercProfiles[ubSrcProfile].ubQuoteActionID;
        pNewProfile.ubLastQuoteSaid = gMercProfiles[ubSrcProfile].ubLastQuoteSaid;
        pNewProfile.ubStrategicInsertionCode = gMercProfiles[ubSrcProfile].ubStrategicInsertionCode;
        pNewProfile.bMercStatus = gMercProfiles[ubSrcProfile].bMercStatus;
        pNewProfile.bSectorZ = gMercProfiles[ubSrcProfile].bSectorZ;
        pNewProfile.usStrategicInsertionData = gMercProfiles[ubSrcProfile].usStrategicInsertionData;
        pNewProfile.sTrueSalary = gMercProfiles[ubSrcProfile].sTrueSalary;
        pNewProfile.ubMiscFlags3 = gMercProfiles[ubSrcProfile].ubMiscFlags3;
        pNewProfile.ubDaysOfMoraleHangover = gMercProfiles[ubSrcProfile].ubDaysOfMoraleHangover;
        pNewProfile.ubNumTimesDrugUseInLifetime = gMercProfiles[ubSrcProfile].ubNumTimesDrugUseInLifetime;
        pNewProfile.uiPrecedentQuoteSaid = gMercProfiles[ubSrcProfile].uiPrecedentQuoteSaid;
        pNewProfile.sPreCombatGridNo = gMercProfiles[ubSrcProfile].sPreCombatGridNo;

        // CJC: this is causing problems so just skip the transfer of exp...
        /*
            pNewProfile.sLifeGain = gMercProfiles[ ubSrcProfile ].sLifeGain;
            pNewProfile.sAgilityGain = gMercProfiles[ ubSrcProfile ].sAgilityGain;
            pNewProfile.sDexterityGain = gMercProfiles[ ubSrcProfile ].sDexterityGain;
            pNewProfile.sStrengthGain = gMercProfiles[ ubSrcProfile ].sStrengthGain;
            pNewProfile.sLeadershipGain = gMercProfiles[ ubSrcProfile ].sLeadershipGain;
            pNewProfile.sWisdomGain = gMercProfiles[ ubSrcProfile ].sWisdomGain;
            pNewProfile.sExpLevelGain = gMercProfiles[ ubSrcProfile ].sExpLevelGain;
            pNewProfile.sMarksmanshipGain = gMercProfiles[ ubSrcProfile ].sMarksmanshipGain;
            pNewProfile.sMedicalGain = gMercProfiles[ ubSrcProfile ].sMedicalGain;
            pNewProfile.sMechanicGain = gMercProfiles[ ubSrcProfile ].sMechanicGain;
            pNewProfile.sExplosivesGain = gMercProfiles[ ubSrcProfile ].sExplosivesGain;

            pNewProfile.bLifeDelta = gMercProfiles[ ubSrcProfile ].bLifeDelta;
            pNewProfile.bAgilityDelta = gMercProfiles[ ubSrcProfile ].bAgilityDelta;
            pNewProfile.bDexterityDelta = gMercProfiles[ ubSrcProfile ].bDexterityDelta;
            pNewProfile.bStrengthDelta = gMercProfiles[ ubSrcProfile ].bStrengthDelta;
            pNewProfile.bLeadershipDelta = gMercProfiles[ ubSrcProfile ].bLeadershipDelta;
            pNewProfile.bWisdomDelta = gMercProfiles[ ubSrcProfile ].bWisdomDelta;
            pNewProfile.bExpLevelDelta = gMercProfiles[ ubSrcProfile ].bExpLevelDelta;
            pNewProfile.bMarksmanshipDelta = gMercProfiles[ ubSrcProfile ].bMarksmanshipDelta;
            pNewProfile.bMedicalDelta = gMercProfiles[ ubSrcProfile ].bMedicalDelta;
            pNewProfile.bMechanicDelta = gMercProfiles[ ubSrcProfile ].bMechanicDelta;
            pNewProfile.bExplosivesDelta = gMercProfiles[ ubSrcProfile ].bExplosivesDelta;
            */

        //memcpy(pNewProfile.bInvStatus, gMercProfiles[ubSrcProfile].bInvStatus, sizeof(int) * 19);
        //memcpy(pNewProfile.bInvStatus, gMercProfiles[ubSrcProfile].bInvStatus, sizeof(int) * 19);
        //memcpy(pNewProfile.inv, gMercProfiles[ubSrcProfile].inv, sizeof(int) * 19);
        //memcpy(pNewProfile.bMercTownReputation, gMercProfiles[ubSrcProfile].bMercTownReputation, sizeof(int) * 20);

        // remove face
        Faces.DeleteSoldierFace(pSoldier);

        pSoldier.ubProfile = ubDestProfile;

        // create new face
//        pSoldier.iFaceIndex = InitSoldierFace(pSoldier);

        // replace profile in group
//        ReplaceSoldierProfileInPlayerGroup(pSoldier.ubGroupID, ubSrcProfile, ubDestProfile);

        pSoldier.bStrength = pNewProfile.bStrength + (uint)pNewProfile.bStrengthDelta;
        pSoldier.bDexterity = pNewProfile.bDexterity + pNewProfile.bDexterityDelta;
        pSoldier.bAgility = pNewProfile.bAgility + pNewProfile.bAgilityDelta;
        pSoldier.bWisdom = pNewProfile.bWisdom + pNewProfile.bWisdomDelta;
        pSoldier.bExpLevel = pNewProfile.bExpLevel + pNewProfile.bExpLevelDelta;
        pSoldier.bLeadership = pNewProfile.bLeadership + pNewProfile.bLeadershipDelta;

        pSoldier.bMarksmanship = pNewProfile.bMarksmanship + pNewProfile.bMarksmanshipDelta;
        pSoldier.bMechanical = pNewProfile.bMechanical + pNewProfile.bMechanicDelta;
        pSoldier.bMedical = pNewProfile.bMedical + pNewProfile.bMedicalDelta;
        pSoldier.bExplosive = pNewProfile.bExplosive + pNewProfile.bExplosivesDelta;

        if (pSoldier.ubProfile == NPCID.LARRY_DRUNK)
        {
            Facts.SetFactTrue(FACT.LARRY_CHANGED);
        }
        else
        {
            Facts.SetFactFalse(FACT.LARRY_CHANGED);
        }

        Interface.DirtyMercPanelInterface(pSoldier, DIRTYLEVEL2);

        return (pSoldier);
    }

    private void DecideActiveTerrorists()
    {
        int ubLoop, ubLoop2;
        NPCID ubTerrorist;
        int ubNumAdditionalTerrorists, ubNumTerroristsAdded = 0;
        int uiChance, uiLocationChoice;
        bool fFoundSpot;
        int[,] sTerroristPlacement = new int[MAX_ADDITIONAL_TERRORISTS, 2]
        {
            { 0, 0},
            { 0, 0},
            { 0, 0},
            { 0, 0}
        };

        // one terrorist will always be Elgin
        // determine how many more terrorists - 2 to 4 more

        // using this stochastic process(!), the chances for terrorists are:
        // EASY:		3, 9%			4, 42%		5, 49%
        // MEDIUM:	3, 25%		4, 50%		5, 25%
        // HARD:		3, 49%		4, 42%		5, 9%
        switch (Globals.gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                uiChance = 70;
                break;
            case DifficultyLevel.Hard:
                uiChance = 30;
                break;
            default:
                uiChance = 50;
                break;
        }

        // add at least 2 more
        ubNumAdditionalTerrorists = 2;
        for (ubLoop = 0; ubLoop < (MAX_ADDITIONAL_TERRORISTS - 2); ubLoop++)
        {
            if (this.rnd.GetRandom(100) < uiChance)
            {
                ubNumAdditionalTerrorists++;
            }
        }

        while (ubNumTerroristsAdded < ubNumAdditionalTerrorists)
        {

            ubLoop = 1; // start at beginning of array (well, after Elgin)

            // NB terrorist ID of 0 indicates end of array
            while (ubNumTerroristsAdded < ubNumAdditionalTerrorists && this.gubTerrorists[ubLoop] != 0)
            {

                ubTerrorist = this.gubTerrorists[ubLoop];

                // random 40% chance of adding this terrorist if not yet placed
                if ((Globals.gMercProfiles[ubTerrorist].sSectorX == 0) && (this.rnd.GetRandom(100) < 40))
                {
                    fFoundSpot = false;
                    // Since there are 5 spots per terrorist and a maximum of 5 terrorists, we
                    // are guaranteed to be able to find a spot for each terrorist since there
                    // aren't enough other terrorists to use up all the spots for any one
                    // terrorist
                    do
                    {
                        // pick a random spot, see if it's already been used by another terrorist
                        uiLocationChoice = this.rnd.GetRandom(NUM_TERRORIST_POSSIBLE_LOCATIONS);
                        for (ubLoop2 = 0; ubLoop2 < ubNumTerroristsAdded; ubLoop2++)
                        {
                            if (sTerroristPlacement[ubLoop2, 0] == this.gsTerroristSector[ubLoop, uiLocationChoice, 0])
                            {
                                if (sTerroristPlacement[ubLoop2, 1] == this.gsTerroristSector[ubLoop, uiLocationChoice, 1])
                                {
                                    continue;
                                }
                            }
                        }
                        fFoundSpot = true;
                    } while (!fFoundSpot);

                    // place terrorist!
                    Globals.gMercProfiles[ubTerrorist].sSectorX = this.gsTerroristSector[ubLoop, uiLocationChoice, 0];
                    Globals.gMercProfiles[ubTerrorist].sSectorY = (MAP_ROW)this.gsTerroristSector[ubLoop, uiLocationChoice, 1];
                    Globals.gMercProfiles[ubTerrorist].bSectorZ = 0;
                    sTerroristPlacement[ubNumTerroristsAdded, 0] = Globals.gMercProfiles[ubTerrorist].sSectorX;
                    sTerroristPlacement[ubNumTerroristsAdded, 1] = (int)Globals.gMercProfiles[ubTerrorist].sSectorY;
                    ubNumTerroristsAdded++;
                }

                ubLoop++;

            }

            // start over if necessary
        }

        // set total terrorists outstanding in Carmen's info byte
        Globals.gMercProfiles[(NPCID)78].bNPCData = 1 + ubNumAdditionalTerrorists;

        // store total terrorists
        this.gubNumTerrorists = 1 + ubNumAdditionalTerrorists;
    }

    private int[,,] gsTerroristSector = new int[NUM_TERRORISTS, NUM_TERRORIST_POSSIBLE_LOCATIONS, 2]
    {
    	// Elgin... preplaced
	        {
            { 0, 0 },
            { 0, 0 },
            { 0, 0 },
            { 0, 0 },
            { 0, 0 }
        },
	    // Slay
	    {
            { 9,    (int)MAP_ROW.F },
            { 14,   (int)MAP_ROW.I },
            { 1,    (int)MAP_ROW.G },
            { 2,    (int)MAP_ROW.G },
            { 8,    (int)MAP_ROW.G }
        },
	    // Matron
	    {
            { 14,   (int)MAP_ROW.I },
            { 6,    (int)MAP_ROW.C },
            { 2,    (int)MAP_ROW.B },
            { 11,   (int)MAP_ROW.L },
            { 8,    (int)MAP_ROW.G }
        },
	    // Imposter
	    {
            { 1,    (int)MAP_ROW.G },
            { 9,    (int)MAP_ROW.F },
            { 11,   (int)MAP_ROW.L },
            { 8,    (int)MAP_ROW.G },
            { 2,    (int)MAP_ROW.G }
        },
	    // Tiffany
	    {
            { 14,   (int)MAP_ROW.I },
            { 2,    (int)MAP_ROW.G },
            { 14,   (int)MAP_ROW.H },
            { 6,    (int)MAP_ROW.C },
            { 2,    (int)MAP_ROW.B }
        },
	    // Rexall
	    {
            { 9,    (int)MAP_ROW.F },
            { 14,   (int)MAP_ROW.H },
            { 2,    (int)MAP_ROW.H },
            { 1,    (int)MAP_ROW.G },
            { 2,    (int)MAP_ROW.B }
        }
    };

    public NPCID[] gubTerrorists = new NPCID[]
    {
        NPCID.DRUGGIST,
        NPCID.SLAY,
        NPCID.ANNIE,
        NPCID.CHRIS,
        NPCID.TIFFANY,
        NPCID.T_REX,
        0
    };
}


// training defines for evolution, no stat increase, stat decrease( de-evolve )
public enum CharacterEvolution
{
    NORMAL_EVOLUTION = 0,
    NO_EVOLUTION,
    DEVOLVE,
}

public enum ATT
{
    NORMAL = 0,
    FRIENDLY,
    LONER,
    OPTIMIST,
    PESSIMIST,
    AGGRESSIVE,
    ARROGANT,
    BIG_SHOT,
    ASSHOLE,
    COWARD,

    NUM_ATTITUDES,
}

public enum PersonalityTrait
{
    NO_PERSONALITYTRAIT = 0,
    HEAT_INTOLERANT,
    NERVOUS,
    CLAUSTROPHOBIC,
    NONSWIMMER,
    FEAR_OF_INSECTS,
    FORGETFUL,
    PSYCHO
}

// chad: Revamp this later
public enum Sexes
{
    MALE = 0,
    FEMALE
}

public enum SexistLevels
{
    NOT_SEXIST = 0,
    SOMEWHAT_SEXIST,
    VERY_SEXIST,
    GENTLEMAN
}

public enum SkillTrait
{
    NO_SKILLTRAIT = 0,
    LOCKPICKING,
    HANDTOHAND,
    ELECTRONICS,
    NIGHTOPS,
    THROWING,
    TEACHING,
    HEAVY_WEAPS,
    AUTO_WEAPS,
    STEALTHY,
    AMBIDEXT,
    THIEF,
    MARTIALARTS,
    KNIFING,
    ONROOF,
    CAMOUFLAGED,
    NUM_SKILLTRAITS
}
