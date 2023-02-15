using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using static SharpAlliance.Core.SubSystems.MERCPROFILE;

namespace SharpAlliance.Core.SubSystems
{
    public class DialogControl
    {
        private readonly SoldierProfileSubSystem soldiers;
        private readonly IFileManager fileManager;
        private readonly QuestEngine quests;
        private readonly InterfaceDialogSubSystem interfaceDialog;
        private readonly Faces faces;

        public DialogControl(
            IFileManager fileManager,
            SoldierProfileSubSystem soldiers,
            QuestEngine questEngine,
            InterfaceDialogSubSystem interfaceDialogSubSystem,
            Faces faces)
        {
            this.soldiers = soldiers;
            this.fileManager = fileManager;
            this.quests = questEngine;
            this.interfaceDialog = interfaceDialogSubSystem;
            this.faces = faces;

            // no better place..heh?.. will load faces for profiles that are 'extern'.....won't have soldiertype instances
            this.InitalizeStaticExternalNPCFaces();
        }


        private bool fExternFacesLoaded = false;
        private bool gfUseAlternateDialogueFile = false;

        public int[] uiExternalStaticNPCFaces = new int[(int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES];

        public int[] uiExternalFaceProfileIds = new int[(int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES]
        {
            97,
            106,
            148,
            156,
            157,
            158,
        };


        // Used to see if the dialog text file exists
        public bool DialogueDataFileExistsForProfile(int ubCharacterNum, int usQuoteNum, bool fWavFile, out string ppStr)
        {
            string pFilename = this.GetDialogueDataFilename(ubCharacterNum, usQuoteNum, fWavFile);

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
            if (this.gfUseAlternateDialogueFile)
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
                    (!this.soldiers.gMercProfiles[(NPCID)ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)
                    || this.interfaceDialog.ProfileCurrentlyTalkingInDialoguePanel(ubCharacterNum)
                    || this.soldiers.gMercProfiles[(NPCID)ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_FORCENPCQUOTE))
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

        public void EmptyDialogueQueue()
        {
        }

        public void InitalizeStaticExternalNPCFaces()
        {
            int iCounter = 0;
            // go and grab all external NPC faces that are needed for the game who won't exist as soldiertypes

            if (this.fExternFacesLoaded == true)
            {
                return;
            }

            this.fExternFacesLoaded = true;

            for (iCounter = 0; iCounter < (int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES; iCounter++)
            {
                this.uiExternalStaticNPCFaces[iCounter] = (int)this.faces.InitFace(this.uiExternalFaceProfileIds[iCounter], OverheadTypes.NOBODY, FaceFlags.FACE_FORCE_SMALL);
            }

            return;
        }

        public ValueTask<bool> InitalizeDialogueControl()
        {
            return ValueTask.FromResult(true);
        }
    }

    public enum ExternalFaces
    {
        SKYRIDER_EXTERNAL_FACE = 0,
        MINER_FRED_EXTERNAL_FACE,
        MINER_MATT_EXTERNAL_FACE,
        MINER_OSWALD_EXTERNAL_FACE,
        MINER_CALVIN_EXTERNAL_FACE,
        MINER_CARL_EXTERNAL_FACE,
        NUMBER_OF_EXTERNAL_NPC_FACES,
    };
}
