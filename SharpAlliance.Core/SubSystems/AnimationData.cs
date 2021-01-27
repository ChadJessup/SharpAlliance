using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class AnimationData
    {
        private readonly IFileManager files;
        private readonly StructureFile structure;

        public AnimationData(
            IFileManager fileManager,
            StructureFile structureFile)
        {
            this.files = fileManager;
            this.structure = structureFile;
        }

        public ValueTask<bool> InitAnimationSystem()
        {
            int cnt1, cnt2;
            string sFilename;
            STRUCTURE_FILE_REF? pStructureFileRef;

            LoadAnimationStateInstructions();

            InitAnimationSurfacesPerBodytype();

            if (!LoadAnimationProfiles())
            {
                //return (SET_ERROR("Problems initializing Animation Profiles"));
                return ValueTask.FromResult(false);
            }

            // OK, Load all animation structures.....
            for (cnt1 = 0; cnt1 < (int)SoldierBodyTypes.TOTALBODYTYPES; cnt1++)
            {
                for (cnt2 = 0; cnt2 < (int)StructData.NUM_STRUCT_IDS; cnt2++)
                {
                    sFilename = gAnimStructureDatabase[cnt1, cnt2].Filename;

                    if (this.files.FileExists(sFilename))
                    {
                        pStructureFileRef = this.structure.LoadStructureFile(sFilename);
                        if (pStructureFileRef == null)
                        {
                            // SET_ERROR("Animation structure file load failed - %s", sFilename);
                        }

                        gAnimStructureDatabase[cnt1, cnt2].pStructureFileRef = pStructureFileRef;
                    }
                }
            }

            return ValueTask.FromResult(true);
        }

        private bool LoadAnimationProfiles()
        {
            return true;
        }

        private void InitAnimationSurfacesPerBodytype()
        {
        }

        private void LoadAnimationStateInstructions()
        {
        }

        public AnimationStructureType[,] gAnimStructureDatabase = new AnimationStructureType[(int)SoldierBodyTypes.TOTALBODYTYPES, (int)StructData.NUM_STRUCT_IDS]
        {
            {
                // Normal Male
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Big male
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Stocky male
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Reg Female
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Adult female creature
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Adult male creature
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Young Adult female creature
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Young Adult male creature
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // larvea creature
                new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // infant creature
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),// default
            }, {
                // Queen creature
                new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
                new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
                new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
                new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
                new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
            }, {
                // Fat civ
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // man civ
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // miniskirt civ
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // dress civ
                new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // kid civ
                new("ANIMS\\STRUCTDATA\\K_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            }, {
                // hat kid civ
                new("ANIMS\\STRUCTDATA\\K_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            }, {
                // cripple civ
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
                new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // cow
                new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // crow
                new("ANIMS\\STRUCTDATA\\CR_STAND.JSD"),
                new("ANIMS\\STRUCTDATA\\CR_CROUCH.JSD"),
                new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // CAT
                new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                // ROBOT1
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
                new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"), // default
            }, {
                // vech 1
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
            }, {
                // tank 1
                new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
            }, {
                // tank 2
                new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
                new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
            }, {
                //ELDORADO
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
            }, {
                //ICECREAMTRUCK
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
            }, {
                //JEEP
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
                new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
            }
        };


        public enum SoldierBodyTypes
        {
            REGMALE = 0,
            BIGMALE,
            STOCKYMALE,
            REGFEMALE,
            ADULTFEMALEMONSTER,
            AM_MONSTER,
            YAF_MONSTER,
            YAM_MONSTER,
            LARVAE_MONSTER,
            INFANT_MONSTER,
            QUEENMONSTER,
            FATCIV,
            MANCIV,
            MINICIV,
            DRESSCIV,
            HATKIDCIV,
            KIDCIV,
            CRIPPLECIV,

            COW,
            CROW,
            BLOODCAT,

            ROBOTNOWEAPON,

            HUMVEE,
            TANK_NW,
            TANK_NE,
            ELDORADO,
            ICECREAMTRUCK,
            JEEP,

            TOTALBODYTYPES
        }

        public struct AnimationStructureType
        {
            public AnimationStructureType(string fileName)
            {
                this.Filename = fileName;
                this.pStructureFileRef = null;
            }

            public string Filename;
            public STRUCTURE_FILE_REF? pStructureFileRef;
        }

        // Enumerations for struct data
        public enum StructData
        {
            S_STRUCT,
            C_STRUCT,
            P_STRUCT,
            F_STRUCT,
            FB_STRUCT,
            DEFAULT_STRUCT,
            NUM_STRUCT_IDS,
            NO_STRUCT = 120,
        };
    }
}
