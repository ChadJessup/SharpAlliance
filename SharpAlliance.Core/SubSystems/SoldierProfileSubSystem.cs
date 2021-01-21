﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.SubSystems
{
    public class SoldierProfileSubSystem
    {
        private static class Constants
        {
            public const int NUM_TERRORISTS = 6;

            public const int NUM_PROFILES = 170;
            public const int MAX_ADDITIONAL_TERRORISTS = 4;
            public const int NUM_TERRORIST_POSSIBLE_LOCATIONS = 5;

            // A.I.M. is 0-39, M.E.R.C.s are 40-50
            public const int AIM_AND_MERC_MERCS = 51;
            public const int FIRST_RPC = 57;
            public const int FIRST_NPC = 75;
        }

        private readonly ILogger<SoldierProfileSubSystem> logger;
        private readonly FileManager fileManager;
        private readonly DialogControl dialogs;
        private readonly GameOptions gGameOptions;
        private readonly ItemSubSystem items;
        private int gubNumTerrorists = 0;
        private Random rnd;

        public SoldierProfileSubSystem(
            ILogger<SoldierProfileSubSystem> logger,
            FileManager fileManager,
            DialogControl dialogControl,
            GameOptions gameOptions,
            ItemSubSystem itemSubSystem)
        {
            this.logger = logger;
            this.fileManager = fileManager;
            this.dialogs = dialogControl;
            this.gGameOptions = gameOptions;
            this.rnd = new Random(DateTime.UtcNow.Millisecond);
            this.items = itemSubSystem;
        }

        public Dictionary<NPCIDs, MERCPROFILE> gMercProfiles { get; } = new();

        public bool LoadMercProfiles()
        {
            //	FILE *fptr;
            Stream fptr;
            string pFileName = "BINARYDATA\\Prof.dat";
            int uiLoop, uiLoop2, uiLoop3;
            Items usItem, usNewGun, usAmmo, usNewAmmo;
            int uiNumBytesRead;


            fptr = this.fileManager.FileOpen(pFileName, FileAccess.Read, fDeleteOnClose: false);
            //if (!fptr)
            //{
            //    this.logger.LogDebug(LoggingEventId.JA2, $"FAILED to LoadMercProfiles from file {pFileName}");
            //    return false;
            //}

            for (uiLoop = 0; uiLoop < Constants.NUM_PROFILES; uiLoop++)
            {
                var npcId = (NPCIDs)uiLoop;

                //if (this.fileManager.JA2EncryptedFileRead(fptr, gMercProfiles[npcId]))
                //{
                //    this.logger.LogDebug(LoggingEventId.JA2, $"FAILED to Read Merc Profiles from File {uiLoop} {pFileName}");
                //    this.fileManager.FileClose(fptr);
                //    return false;
                //}

                //if the Dialogue exists for the merc, allow the merc to be hired
                if (this.dialogs.DialogueDataFileExistsForProfile(uiLoop, 0, false, out var _))
                {
                    gMercProfiles[npcId].bMercStatus = 0;
                }
                else
                {
                    gMercProfiles[npcId].bMercStatus = MercStatus.MERC_HAS_NO_TEXT_FILE;
                }

                // if the merc has a medical deposit
                if (gMercProfiles[npcId].bMedicalDeposit > 0)
                {
                    var profile = gMercProfiles[npcId];
                    gMercProfiles[npcId].sMedicalDepositAmount = CalcMedicalDeposit(ref profile);
                }
                else
                {
                    gMercProfiles[npcId].sMedicalDepositAmount = 0;
                }

                // ATE: New, face display indipendent of ID num now
                // Setup face index value
                // Default is the ubCharNum
                gMercProfiles[npcId].ubFaceIndex = uiLoop;

                if (!gGameOptions.GunNut)
                {

                    // CJC: replace guns in profile if they aren't available
                    for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
                    {
                        usItem = gMercProfiles[npcId].inv[uiLoop2];

                        if ((Item[usItem].usItemClass.HasFlag(ItemSubTypes.IC_GUN)) && ExtendedGunListGun(usItem))
                        {
                            usNewGun = StandardGunListReplacement(usItem);
                            if (usNewGun != Items.NONE)
                            {
                                gMercProfiles[npcId].inv[uiLoop2] = usNewGun;

                                // must search through inventory and replace ammo accordingly
                                for (uiLoop3 = 0; uiLoop3 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop3++)
                                {
                                    usAmmo = gMercProfiles[npcId].inv[uiLoop3];
                                    if (Item[usAmmo].usItemClass.HasFlag(ItemSubTypes.IC_AMMO))
                                    {
                                        usNewAmmo = FindReplacementMagazineIfNecessary(usItem, usAmmo, usNewGun);
                                        if (usNewAmmo != Items.NONE)
                                        {
                                            // found a new magazine, replace...
                                            gMercProfiles[npcId].inv[uiLoop3] = usNewAmmo;
                                        }
                                    }
                                }
                            }
                        }
                    }
                } // end of if not gun nut

                //ATE: Calculate some inital attractiveness values for buddy's inital equipment...
                // Look for gun and armour
                gMercProfiles[npcId].bMainGunAttractiveness = -1;
                gMercProfiles[npcId].bArmourAttractiveness = -1;

                for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
                {
                    var npcId2 = (NPCIDs)uiLoop2;
                    usItem = gMercProfiles[npcId2].inv[uiLoop2];

                    if (usItem != Items.NONE)
                    {
                        // Check if it's a gun
                        if (Item[usItem].usItemClass.HasFlag(ItemSubTypes.IC_GUN))
                        {
                            gMercProfiles[npcId].bMainGunAttractiveness = Weapon[usItem].ubDeadliness;
                        }

                        // If it's armour
                        if (Item[usItem].usItemClass.HasFlag(ItemSubTypes.IC_ARMOUR))
                        {
                            gMercProfiles[npcId].bArmourAttractiveness = Armour[Item[usItem].ubClassIndex].ubProtection;
                        }
                    }
                }


                // OK, if we are a created slot, this will get overriden at some time..

                //add up the items the merc has for the usOptionalGearCost 
                gMercProfiles[npcId].usOptionalGearCost = 0;
                for (uiLoop2 = 0; uiLoop2 < (int)InventorySlot.NUM_INV_SLOTS; uiLoop2++)
                {
                    if (gMercProfiles[npcId].inv[uiLoop2] != Items.NONE)
                    {
                        //get the item
                        usItem = gMercProfiles[npcId].inv[uiLoop2];

                        //add the cost
                        gMercProfiles[npcId].usOptionalGearCost += Item[usItem].usPrice;
                    }
                }

                //These variables to get loaded in
                gMercProfiles[npcId].fUseProfileInsertionInfo = false;
                gMercProfiles[npcId].sGridNo = 0;

                // ARM: this is also being done inside the profile editor, but put it here too, so this project's code makes sense
                gMercProfiles[npcId].bHatedCount[0] = gMercProfiles[npcId].bHatedTime[0];
                gMercProfiles[npcId].bHatedCount[1] = gMercProfiles[npcId].bHatedTime[1];
                gMercProfiles[npcId].bLearnToHateCount = gMercProfiles[npcId].bLearnToHateTime;
                gMercProfiles[npcId].bLearnToLikeCount = gMercProfiles[npcId].bLearnToLikeTime;
            }

            // SET SOME DEFAULT LOCATIONS FOR STARTING NPCS

            this.fileManager.FileClose(fptr);

            // decide which terrorists are active
            DecideActiveTerrorists();

            // initialize mercs' status
            StartSomeMercsOnAssignment();

            // initial recruitable mercs' reputation in each town
            InitializeProfilesForTownReputation();

            EditScreen.gfProfileDataLoaded = true;

            // no better place..heh?.. will load faces for profiles that are 'extern'.....won't have soldiertype instances
            InitalizeStaticExternalNPCFaces();

            // car portrait values
            LoadCarPortraitValues();

            return true;
        }

        private void StartSomeMercsOnAssignment()
        {
            NPCIDs uiCnt;
            MERCPROFILE pProfile;
            int uiChance;

            // some randomly picked A.I.M. mercs will start off "on assignment" at the beginning of each new game
            for (uiCnt = 0; uiCnt < (NPCIDs)Constants.AIM_AND_MERC_MERCS; uiCnt++)
            {
                pProfile = gMercProfiles[uiCnt];

                // calc chance to start on assignment
                uiChance = 5 * pProfile.bExpLevel;

                if (this.rnd.GetRandom(100) < uiChance)
                {
                    pProfile.bMercStatus = MercStatus.MERC_WORKING_ELSEWHERE;

                    // 1-(6 to 11) days
                    pProfile.uiDayBecomesAvailable = 1 + this.rnd.Next(0, (6 + (pProfile.bExpLevel / 2)));
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

        private int CalcMedicalDeposit(ref MERCPROFILE profile)
        {
            int usDeposit;

            // this rounds off to the nearest hundred
            usDeposit = ((5 * CalcCompetence(ref profile)) + 50) / 100 * 100;

            return usDeposit;
        }

        private int CalcCompetence(ref MERCPROFILE profile)
        {
            int uiStats, uiSkills, uiActionPoints, uiSpecialSkills;
            int usCompetence;

            // count life twice 'cause it's also hit points
            // mental skills are halved 'cause they're actually not that important within the game
            uiStats = ((2 * profile.bLifeMax) + profile.bStrength + profile.bAgility + profile.bDexterity + ((profile.bLeadership + profile.bWisdom) / 2)) / 3;

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

        private void DecideActiveTerrorists()
        {
            int ubLoop, ubLoop2;
            NPCIDs ubTerrorist;
            int ubNumAdditionalTerrorists, ubNumTerroristsAdded = 0;
            int uiChance, uiLocationChoice;
            bool fFoundSpot;
            int[,] sTerroristPlacement = new int[Constants.MAX_ADDITIONAL_TERRORISTS, 2]
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
            switch (gGameOptions.DifficultyLevel)
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
            for (ubLoop = 0; ubLoop < (Constants.MAX_ADDITIONAL_TERRORISTS - 2); ubLoop++)
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
                while (ubNumTerroristsAdded < ubNumAdditionalTerrorists && gubTerrorists[ubLoop] != 0)
                {

                    ubTerrorist = gubTerrorists[ubLoop];

                    // random 40% chance of adding this terrorist if not yet placed
                    if ((gMercProfiles[(NPCIDs)ubTerrorist].sSectorX == 0) && (this.rnd.GetRandom(100) < 40))
                    {
                        fFoundSpot = false;
                        // Since there are 5 spots per terrorist and a maximum of 5 terrorists, we
                        // are guaranteed to be able to find a spot for each terrorist since there
                        // aren't enough other terrorists to use up all the spots for any one
                        // terrorist
                        do
                        {
                            // pick a random spot, see if it's already been used by another terrorist
                            uiLocationChoice = this.rnd.GetRandom(Constants.NUM_TERRORIST_POSSIBLE_LOCATIONS);
                            for (ubLoop2 = 0; ubLoop2 < ubNumTerroristsAdded; ubLoop2++)
                            {
                                if (sTerroristPlacement[ubLoop2, 0] == gsTerroristSector[ubLoop, uiLocationChoice, 0])
                                {
                                    if (sTerroristPlacement[ubLoop2, 1] == gsTerroristSector[ubLoop, uiLocationChoice, 1])
                                    {
                                        continue;
                                    }
                                }
                            }
                            fFoundSpot = true;
                        } while (!fFoundSpot);

                        // place terrorist!
                        gMercProfiles[(NPCIDs)ubTerrorist].sSectorX = gsTerroristSector[ubLoop, uiLocationChoice, 0];
                        gMercProfiles[(NPCIDs)ubTerrorist].sSectorY = gsTerroristSector[ubLoop, uiLocationChoice, 1];
                        gMercProfiles[(NPCIDs)ubTerrorist].bSectorZ = 0;
                        sTerroristPlacement[ubNumTerroristsAdded, 0] = gMercProfiles[(NPCIDs)ubTerrorist].sSectorX;
                        sTerroristPlacement[ubNumTerroristsAdded, 1] = gMercProfiles[(NPCIDs)ubTerrorist].sSectorY;
                        ubNumTerroristsAdded++;
                    }

                    ubLoop++;

                }

                // start over if necessary
            }

            // set total terrorists outstanding in Carmen's info byte
            gMercProfiles[(NPCIDs)78].bNPCData = 1 + ubNumAdditionalTerrorists;

            // store total terrorists
            gubNumTerrorists = 1 + ubNumAdditionalTerrorists;
        }

        private int[,,] gsTerroristSector = new int[Constants.NUM_TERRORISTS, Constants.NUM_TERRORIST_POSSIBLE_LOCATIONS, 2]
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
                { 9,    (int)MapRows.MAP_ROW_F },
                { 14,   (int)MapRows.MAP_ROW_I },
                { 1,    (int)MapRows.MAP_ROW_G },
                { 2,    (int)MapRows.MAP_ROW_G },
                { 8,    (int)MapRows.MAP_ROW_G }
            },
    	    // Matron
    	    {
                { 14,   (int)MapRows.MAP_ROW_I },
                { 6,    (int)MapRows.MAP_ROW_C },
                { 2,    (int)MapRows.MAP_ROW_B },
                { 11,   (int)MapRows.MAP_ROW_L },
                { 8,    (int)MapRows.MAP_ROW_G }
            },
    	    // Imposter
    	    {
                { 1,    (int)MapRows.MAP_ROW_G },
                { 9,    (int)MapRows.MAP_ROW_F },
                { 11,   (int)MapRows.MAP_ROW_L },
                { 8,    (int)MapRows.MAP_ROW_G },
                { 2,    (int)MapRows.MAP_ROW_G }
            },
    	    // Tiffany
    	    {
                { 14,   (int)MapRows.MAP_ROW_I },
                { 2,    (int)MapRows.MAP_ROW_G },
                { 14,   (int)MapRows.MAP_ROW_H },
                { 6,    (int)MapRows.MAP_ROW_C },
                { 2,    (int)MapRows.MAP_ROW_B }
            },
    	    // Rexall
    	    {
                { 9,    (int)MapRows.MAP_ROW_F },
                { 14,   (int)MapRows.MAP_ROW_H },
                { 2,    (int)MapRows.MAP_ROW_H },
                { 1,    (int)MapRows.MAP_ROW_G },
                { 2,    (int)MapRows.MAP_ROW_B }
            }
        };

        public NPCIDs[] gubTerrorists = new NPCIDs[]
        {
            NPCIDs.DRUGGIST,
            NPCIDs.SLAY,
            NPCIDs.ANNIE,
            NPCIDs.CHRIS,
            NPCIDs.TIFFANY,
            NPCIDs.T_REX,
            0
        };
    }
}
