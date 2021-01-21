using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.SubSystems.MERCPROFILE;

namespace SharpAlliance.Core.SubSystems
{
    public class DialogControl
    {
        private readonly SoldierProfileSubSystem soldiers;
        private readonly FileManager fileManager;
        private readonly QuestEngine quests;

        public DialogControl(
            FileManager fileManager, 
            SoldierProfileSubSystem soldiers,
            QuestEngine questEngine)
        {
            this.soldiers = soldiers;
            this.fileManager = fileManager;
            this.quests = questEngine;
        }

        private bool gfUseAlternateDialogueFile = false;

        // Used to see if the dialog text file exists
        public bool DialogueDataFileExistsForProfile(int ubCharacterNum, int usQuoteNum, bool fWavFile, out string ppStr)
        {
            string pFilename = GetDialogueDataFilename(ubCharacterNum, usQuoteNum, fWavFile);

                ppStr = pFilename;

            return this.fileManager.FileExists(pFilename);
        }
        public const int FIRST_RPC = 57;
        private string GetDialogueDataFilename(int ubCharacterNum, int usQuoteNum, bool fWavFile)
        {
            string zFileName = string.Empty;
            int ubFileNumID;

            // Are we an NPC OR an RPC that has not been recruited?
            // ATE: Did the || clause here to allow ANY RPC that talks while the talking menu is up to use an npc quote file
            if (gfUseAlternateDialogueFile)
            {
                if (fWavFile)
                {
                    // build name of wav file (characternum + quotenum)
                    // sprintf(zFileName, "NPC_SPEECH\\d_%03d_%03d.wav", ubCharacterNum, usQuoteNum);
                }
                else
                {
                    // assume EDT files are in EDT directory on HARD DRIVE
                    // sprintf(zFileName, "NPCDATA\\d_%03d.EDT", ubCharacterNum);
                }
            }
            else if (ubCharacterNum >= FIRST_RPC &&
                    (!this.soldiers.gMercProfiles[(NPCIDs)ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)
                    || ProfileCurrentlyTalkingInDialoguePanel(ubCharacterNum)
                    || this.soldiers.gMercProfiles[(NPCIDs)ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_FORCENPCQUOTE))
                    )
            {
                ubFileNumID = ubCharacterNum;

                // ATE: If we are merc profile ID #151-154, all use 151's data....
                if (ubCharacterNum >= 151 && ubCharacterNum <= 154)
                {
                    ubFileNumID = 151;
                }

                // If we are character #155, check fact!
                if (ubCharacterNum == 155 && this.quests.gubFact[220] == 0)
                {
                    ubFileNumID = 155;
                }


                if (fWavFile)
                {
                    // sprintf(zFileName, "NPC_SPEECH\\%03d_%03d.wav", ubFileNumID, usQuoteNum);
                }
                else
                {
                    // assume EDT files are in EDT directory on HARD DRIVE
                    // sprintf(zFileName, "NPCDATA\\%03d.EDT", ubFileNumID);
                }
            }
            else
            {
                if (fWavFile)
                {
                    // build name of wav file (characternum + quotenum)
                    // sprintf(zFileName, "SPEECH\\%03d_%03d.wav", ubCharacterNum, usQuoteNum);
                }
                else
                {
                    // assume EDT files are in EDT directory on HARD DRIVE
                    // sprintf(zFileName, "MERCEDT\\%03d.EDT", ubCharacterNum);
                }
            }

            return zFileName;
        }
    }
}
