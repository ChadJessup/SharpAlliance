using System;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class SoundManager : ISoundManager
    {
        public const uint NO_SAMPLE = 0xffffffff;
        public const uint SOUND_ERROR = 0xffffffff;

        private readonly IFileManager files;

        public SoundManager(IFileManager fileManager) => this.files = fileManager;

        public bool IsInitialized { get; }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> InitSound()
        {
            uint uiCount;

            for (uiCount = 0; uiCount < (uint)SoundDefine.NUM_SAMPLES; uiCount++)
            {
                this.SoundLoadSample(szSoundEffects[uiCount]);
                this.SoundLockSample(szSoundEffects[uiCount]);
            }

            return ValueTask.FromResult(true);
        }

        private void SoundLoadSample(string sound)
        {
        }

        private void SoundLockSample(string sound)
        {
        }

        public void SoundStopAll()
        {
        }

        public void Dispose()
        {
        }

        public void SetSoundEffectsVolume(int iNewValue)
        {
        }

        public void SoundStop(uint uiOptionToggleSound)
        {
        }

        public int GetSoundEffectsVolume()
        {
            return 10;
        }

        public int GetSpeechVolume()
        {
            return 10;
        }

        public void SetSpeechVolume(int iNewValue)
        {
        }

        public bool SoundIsPlaying(uint soundID)
        {
            return true;
        }

        public static string[] szSoundEffects = new string[(int)SoundDefine.NUM_SAMPLES]
        {
            "SOUNDS\\RICOCHET 01.WAV",
            "SOUNDS\\RICOCHET 02.WAV",
            "SOUNDS\\RICOCHET 01.WAV",
            "SOUNDS\\RICOCHET 02.WAV",
            "SOUNDS\\RICOCHET 01.WAV",
            "SOUNDS\\RICOCHET 02.WAV",
            "SOUNDS\\RICOCHET 01.WAV",
            "SOUNDS\\RICOCHET 02.WAV",
            "SOUNDS\\DIRT IMPACT 01.WAV",
            "SOUNDS\\DIRT IMPACT 01.WAV",
            "SOUNDS\\KNIFE HIT GROUND.WAV",
            "SOUNDS\\FALL TO KNEES 01.WAV",
            "SOUNDS\\FALL TO KNEES 02.WAV",
            "SOUNDS\\KNEES TO DIRT 01.WAV",
            "SOUNDS\\KNEES TO DIRT 02.WAV",
            "SOUNDS\\KNEES TO DIRT 03.WAV",
            "SOUNDS\\HEAVY FALL 01.WAV",
            "SOUNDS\\BODY_SPLAT.WAV",
            "SOUNDS\\GLASS_BREAK1.WAV",
            "SOUNDS\\GLASS_BREAK2.WAV",

            "SOUNDS\\DOOR OPEN 01.WAV",
            "SOUNDS\\DOOR OPEN 02.WAV",
            "SOUNDS\\DOOR OPEN 03.WAV",
            "SOUNDS\\DOOR CLOSE 01.WAV",
            "SOUNDS\\DOOR CLOSE 02.WAV",
            "SOUNDS\\UNLOCK LOCK.WAV",
            "SOUNDS\\KICKIN LOCK.WAV",
            "SOUNDS\\BREAK LOCK.WAV",
            "SOUNDS\\PICKING LOCK.WAV",

            "SOUNDS\\GARAGE DOOR OPEN.WAV",
            "SOUNDS\\GARAGE DOOR CLOSE.WAV",
            "SOUNDS\\ELEVATOR DOOR OPEN.WAV",
            "SOUNDS\\ELEVATOR DOOR CLOSE.WAV",
            "SOUNDS\\HIGH TECH DOOR OPEN.WAV",
            "SOUNDS\\HIGH TECH DOOR CLOSE.WAV",
            "SOUNDS\\CURTAINS DOOR OPEN.WAV",
            "SOUNDS\\CURTAINS DOOR CLOSE.WAV",
            "SOUNDS\\METAL DOOR OPEN.WAV",
            "SOUNDS\\METAL DOOR CLOSE.WAV",

            "SOUNDS\\ftp gravel 01.WAV",
            "SOUNDS\\ftp gravel 02.WAV",
            "SOUNDS\\ftp gravel 03.WAV",
            "SOUNDS\\ftp gravel 04.WAV",
            "SOUNDS\\ftp gritty 01.WAV",
            "SOUNDS\\ftp gritty 02.WAV",
            "SOUNDS\\ftp gritty 03.WAV",
            "SOUNDS\\ftp gritty 04.WAV",
            "SOUNDS\\ftp leaves 01.WAV",
            "SOUNDS\\ftp leaves 02.WAV",
            "SOUNDS\\ftp leaves 03.WAV",
            "SOUNDS\\ftp leaves 04.WAV",

            "SOUNDS\\CRAWLING 01.WAV",
            "SOUNDS\\CRAWLING 02.WAV",
            "SOUNDS\\CRAWLING 03.WAV",
            "SOUNDS\\CRAWLING 04.WAV",
            "SOUNDS\\BEEP2.WAV",
            "SOUNDS\\ENDTURN.WAV",
            "SOUNDS\\JA2 DEATH HIT.WAV",
            "SOUNDS\\DOORCR_B.WAV",
            "SOUNDS\\HEAD EXPLODING 01.WAV",
            "SOUNDS\\BODY EXPLODING.WAV",
            "SOUNDS\\EXPLODE1.WAV",
            "SOUNDS\\CROW EXPLODING.WAV",
            "SOUNDS\\SMALL EXPLOSION 01.WAV",

            "SOUNDS\\HELI1.WAV",
            "SOUNDS\\BULLET IMPACT 01.WAV",
            "SOUNDS\\BULLET IMPACT 02.WAV",
            "SOUNDS\\BULLET IMPACT 02.WAV",

            "STSOUNDS\\BLAH.WAV",									// CREATURE ATTACK
		    
		    "SOUNDS\\STEP INTO WATER.WAV",
            "SOUNDS\\SPLASH FROM SHALLOW TO DEEP.WAV",

            "SOUNDS\\COW HIT.WAV",																	// COW HIT
		    "SOUNDS\\COW DIE.WAV",																	// COW DIE

		    // THREE COMPUTER VOICE SOUNDS FOR RG
		    "SOUNDS\\LINE 02 FX.WAV",
            "SOUNDS\\LINE 01 FX.WAV",
            "SOUNDS\\LINE 03 FX.WAV",

            "SOUNDS\\CAVE COLLAPSING.WAV",														// CAVE_COLLAPSE


		    "SOUNDS\\RAID WHISTLE.WAV",															// RAID
		    "SOUNDS\\RAID AMBIENT.WAV",
            "SOUNDS\\RAID DIVE.WAV",
            "SOUNDS\\RAID DIVE.WAV",
            "SOUNDS\\RAID WHISTLE.WAV",															// RAID

		    // VEHICLES
		    "SOUNDS\\DRIVING 01.WAV",																// DRIVING
		    "SOUNDS\\ENGINE START.WAV",															// ON
		    "SOUNDS\\ENGINE OFF.WAV",																// OFF
		    "SOUNDS\\INTO VEHICLE.WAV",															// INTO


		    "SOUNDS\\WEAPONS\\DRY FIRE 1.WAV",											// Dry fire sound ( for gun jam )

		    // IMPACTS
		    "SOUNDS\\WOOD IMPACT 01A.WAV",													 // S_WOOD_IMPACT1
		    "SOUNDS\\WOOD IMPACT 01B.WAV",
            "SOUNDS\\WOOD IMPACT 01A.WAV",
            "SOUNDS\\PORCELAIN IMPACT.WAV",
            "SOUNDS\\TIRE IMPACT 01.WAV",
            "SOUNDS\\STONE IMPACT 01.WAV",
            "SOUNDS\\WATER IMPACT 01.WAV",
            "SOUNDS\\VEG IMPACT 01.WAV",
            "SOUNDS\\METAL HIT 01.WAV",															 // S_METAL_HIT1	
		    "SOUNDS\\METAL HIT 01.WAV",
            "SOUNDS\\METAL HIT 01.WAV",

            "SOUNDS\\SLAP_IMPACT.WAV",

		    // FIREARM RELOAD
		    "SOUNDS\\WEAPONS\\REVOLVER RELOAD.WAV",										// REVOLVER 
		    "SOUNDS\\WEAPONS\\PISTOL RELOAD.WAV",											// PISTOL
		    "SOUNDS\\WEAPONS\\SMG RELOAD.WAV",												// SMG
		    "SOUNDS\\WEAPONS\\RIFLE RELOAD.WAV",											// RIFLE
 		    "SOUNDS\\WEAPONS\\SHOTGUN RELOAD.WAV",										// SHOTGUN
		    "SOUNDS\\WEAPONS\\LMG RELOAD.WAV",												// LMG

		    // FIREARM LOCKNLOAD
		    "SOUNDS\\WEAPONS\\REVOLVER LNL.WAV",											// REVOLVER 
		    "SOUNDS\\WEAPONS\\PISTOL LNL.WAV",												// PISTOL
		    "SOUNDS\\WEAPONS\\SMG LNL.WAV",														// SMG
		    "SOUNDS\\WEAPONS\\RIFLE LNL.WAV",													// RIFLE
 		    "SOUNDS\\WEAPONS\\SHOTGUN LNL.WAV",												// SHOTGUN
		    "SOUNDS\\WEAPONS\\LMG LNL.WAV",														// LMG

		    // ROCKET LAUCNHER
		    "SOUNDS\\WEAPONS\\SMALL ROCKET LAUNCHER.WAV",							// SMALL ROCKET LUANCHER
		    "SOUNDS\\WEAPONS\\MORTAR FIRE 01.WAV",										// GRENADE LAUNCHER
		    "SOUNDS\\WEAPONS\\MORTAR FIRE 01.WAV",										// UNDERSLUNG GRENADE LAUNCHER
		    "SOUNDS\\WEAPONS\\ROCKET LAUNCHER.WAV",
            "SOUNDS\\WEAPONS\\MORTAR FIRE 01.WAV",

		    // FIREARMS
		    "SOUNDS\\WEAPONS\\9mm SINGLE SHOT.WAV",										//	S_GLOCK17				9mm
		    "SOUNDS\\WEAPONS\\9mm SINGLE SHOT.WAV",										//	S_GLOCK18				9mm
		    "SOUNDS\\WEAPONS\\9mm SINGLE SHOT.WAV",										//	S_BERETTA92			9mm
		    "SOUNDS\\WEAPONS\\9mm SINGLE SHOT.WAV",										//	S_BERETTA93			9mm
		    "SOUNDS\\WEAPONS\\38 CALIBER.WAV",												//	S_SWSPECIAL			.38
		    "SOUNDS\\WEAPONS\\357 SINGLE SHOT.WAV",										//	S_BARRACUDA			.357
		    "SOUNDS\\WEAPONS\\357 SINGLE SHOT.WAV",										//	S_DESERTEAGLE		.357
		    "SOUNDS\\WEAPONS\\45 CALIBER SINGLE SHOT.WAV",						//	S_M1911					.45
		    "SOUNDS\\WEAPONS\\9mm SINGLE SHOT.WAV",										//	S_MP5K					9mm
		    "SOUNDS\\WEAPONS\\45 CALIBER SINGLE SHOT.WAV",						//	S_MAC10					.45
		    "SOUNDS\\WEAPONS\\45 CALIBER SINGLE SHOT.WAV",						//	S_THOMPSON			.45
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_COMMANDO			5.56
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_MP53					5.56?
		    "SOUNDS\\WEAPONS\\5,45 SINGLE SHOT.WAV",									//	S_AKSU74				5.45
		    "SOUNDS\\WEAPONS\\5,7 SINGLE SHOT.WAV",									//	S_P90						5.7
		    "SOUNDS\\WEAPONS\\7,62 WP SINGLE SHOT.WAV",							//	S_TYPE85				7.62 WP
		    "SOUNDS\\WEAPONS\\7,62 WP SINGLE SHOT.WAV",								//	S_SKS						7.62 WP
		    "SOUNDS\\WEAPONS\\7,62 WP SINGLE SHOT.WAV",								//	S_DRAGUNOV			7.62 WP
		    "SOUNDS\\WEAPONS\\7,62 NATO SINGLE SHOT.WAV",							//	S_M24						7.62 NATO
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_AUG						5.56mm
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_G41						5.56mm
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_RUGERMINI			5.56mm
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_C7						5.56mm
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_FAMAS					5.56mm
		    "SOUNDS\\WEAPONS\\5,45 SINGLE SHOT.WAV",									//	S_AK74					5.45mm
		    "SOUNDS\\WEAPONS\\7,62 WP SINGLE SHOT.WAV",								//	S_AKM						7.62mm WP
		    "SOUNDS\\WEAPONS\\7,62 NATO SINGLE SHOT.WAV",							//	S_M14						7.62mm NATO
		    "SOUNDS\\WEAPONS\\7,62 NATO SINGLE SHOT.WAV",							//	S_FNFAL					7.62mm NATO
		    "SOUNDS\\WEAPONS\\7,62 NATO SINGLE SHOT.WAV",							//	S_G3A3					7.62mm NATO
		    "SOUNDS\\WEAPONS\\4,7 SINGLE SHOT.WAV",										//	S_G11						4.7mm
		    "SOUNDS\\WEAPONS\\SHOTGUN SINGLE SHOT.WAV",								//	S_M870					SHOTGUN
		    "SOUNDS\\WEAPONS\\SHOTGUN SINGLE SHOT.WAV",								//	S_SPAS					SHOTGUN
		    "SOUNDS\\WEAPONS\\SHOTGUN SINGLE SHOT.WAV",								//	S_CAWS					SHOTGUN
		    "SOUNDS\\WEAPONS\\5,56 SINGLE SHOT.WAV",									//	S_FNMINI				5.56mm
		    "SOUNDS\\WEAPONS\\5,45 SINGLE SHOT.WAV",									//	S_RPK74					5.45mm
		    "SOUNDS\\WEAPONS\\7,62 WP SINGLE SHOT.WAV",								//	S_21E						7.62mm
		    "SOUNDS\\WEAPONS\\KNIFE THROW SWOOSH.WAV",												//	KNIFE THROW
		    "SOUNDS\\WEAPONS\\TANK_CANNON.WAV",
            "SOUNDS\\WEAPONS\\BURSTTYPE1.WAV",
            "SOUNDS\\WEAPONS\\AUTOMAG SINGLE.WAV",

            "SOUNDS\\WEAPONS\\SILENCER 02.WAV",
            "SOUNDS\\WEAPONS\\SILENCER 03.WAV",

            "SOUNDS\\SWOOSH 01.WAV",
            "SOUNDS\\SWOOSH 03.WAV",
            "SOUNDS\\SWOOSH 05.WAV",
            "SOUNDS\\SWOOSH 06.WAV",
            "SOUNDS\\SWOOSH 11.WAV",
            "SOUNDS\\SWOOSH 14.WAV",

		    // CREATURE_SOUNDS
		    "SOUNDS\\ADULT FALL 01.WAV",
            "SOUNDS\\ADULT STEP 01.WAV",
            "SOUNDS\\ADULT STEP 02.WAV",
            "SOUNDS\\ADULT SWIPE 01.WAV",
            "SOUNDS\\Eating_Flesh 01.WAV",
            "SOUNDS\\ADULT CRIPPLED.WAV",
            "SOUNDS\\ADULT DYING PART 1.WAV",
            "SOUNDS\\ADULT DYING PART 2.WAV",
            "SOUNDS\\ADULT LUNGE 01.WAV",
            "SOUNDS\\ADULT SMELLS THREAT.WAV",
            "SOUNDS\\ADULT SMELLS PREY.WAV",
            "SOUNDS\\ADULT SPIT.WAV",

		    // BABY
		    "SOUNDS\\BABY DYING 01.WAV",
            "SOUNDS\\BABY DRAGGING 01.WAV",
            "SOUNDS\\BABY SHRIEK 01.WAV",
            "SOUNDS\\BABY SPITTING 01.WAV",

		    // LARVAE
		    "SOUNDS\\LARVAE MOVEMENT 01.WAV",
            "SOUNDS\\LARVAE RUPTURE 01.WAV",

		    //QUEEN
		    "SOUNDS\\QUEEN SHRIEK 01.WAV",
            "SOUNDS\\QUEEN DYING 01.WAV",
            "SOUNDS\\QUEEN ENRAGED ATTACK.WAV",
            "SOUNDS\\QUEEN RUPTURING.WAV",
            "SOUNDS\\QUEEN CRIPPLED.WAV",
            "SOUNDS\\QUEEN SMELLS THREAT.WAV",
            "SOUNDS\\QUEEN WHIP ATTACK.WAV",

            "SOUNDS\\ROCK HIT 01.WAV",
            "SOUNDS\\ROCK HIT 02.WAV",

            "SOUNDS\\SCRATCH.WAV",
            "SOUNDS\\ARMPIT.WAV",
            "SOUNDS\\CRACKING BACK.WAV",

            "SOUNDS\\WEAPONS\\Auto Resolve Composite 02 (8-22).wav",							//  The FF sound in autoresolve interface

		    "SOUNDS\\Email Alert 01.wav",
            "SOUNDS\\Entering Text 02.wav",
            "SOUNDS\\Removing Text 02.wav",
            "SOUNDS\\Computer Beep 01 In.wav",
            "SOUNDS\\Computer Beep 01 Out.wav",
            "SOUNDS\\Computer Switch 01 In.wav",
            "SOUNDS\\Computer Switch 01 Out.wav",
            "SOUNDS\\Very Small Switch 01 In.wav",
            "SOUNDS\\Very Small Switch 01 Out.wav",
            "SOUNDS\\Very Small Switch 02 In.wav",
            "SOUNDS\\Very Small Switch 02 Out.wav",
            "SOUNDS\\Small Switch 01 In.wav",
            "SOUNDS\\Small Switch 01 Out.wav",
            "SOUNDS\\Small Switch 02 In.wav",
            "SOUNDS\\Small Switch 02 Out.wav",
            "SOUNDS\\Small Switch 03 In.wav",
            "SOUNDS\\Small Switch 03 Out.wav",
            "SOUNDS\\Big Switch 03 In.wav",
            "SOUNDS\\Big Switch 03 Out.wav",
            "SOUNDS\\Alarm.wav",
            "SOUNDS\\Fight Bell.wav",
            "SOUNDS\\Helicopter Crash Sequence.wav",
            "SOUNDS\\Attachment.wav",
            "SOUNDS\\Ceramic Armour Insert.wav",
            "SOUNDS\\Detonator Beep.wav",
            "SOUNDS\\Grab Roof.wav",
            "SOUNDS\\Land On Roof.wav",
            "SOUNDS\\Branch Snap 01.wav",
            "SOUNDS\\Branch Snap 02.wav",
            "SOUNDS\\Indoor Bump 01.wav",

            "SOUNDS\\Fridge Door Open.wav",
            "SOUNDS\\Fridge Door Close.wav",

            "SOUNDS\\Fire 03 Loop.wav",
            "SOUNDS\\GLASS_CRACK.wav",
            "SOUNDS\\SPIT RICOCHET.WAV",
            "SOUNDS\\TIGER HIT.WAV",
            "SOUNDS\\bloodcat dying 02.WAV",
            "SOUNDS\\SLAP.WAV",
            "SOUNDS\\ROBOT BEEP.WAV",
            "SOUNDS\\ELECTRICITY.WAV",
            "SOUNDS\\SWIMMING 01.WAV",
            "SOUNDS\\SWIMMING 02.WAV",
            "SOUNDS\\KEY FAILURE.WAV",
            "SOUNDS\\target cursor.WAV",
            "SOUNDS\\statue open.WAV",
            "SOUNDS\\remote activate.WAV",
            "SOUNDS\\wirecutters.WAV",
            "SOUNDS\\drink from canteen.WAV",
            "SOUNDS\\bloodcat attack.wav",
            "SOUNDS\\bloodcat loud roar.wav",
            "SOUNDS\\robot greeting.wav",
            "SOUNDS\\robot death.wav",
            "SOUNDS\\gas grenade explode.WAV",
            "SOUNDS\\air escaping.WAV",
            "SOUNDS\\drawer open.WAV",
            "SOUNDS\\drawer close.WAV",
            "SOUNDS\\locker door open.WAV",
            "SOUNDS\\locker door close.WAV",
            "SOUNDS\\wooden box open.WAV",
            "SOUNDS\\wooden box close.WAV",
            "SOUNDS\\robot stop moving.WAV",
            "SOUNDS\\water movement 01.wav",
            "SOUNDS\\water movement 02.wav",
            "SOUNDS\\water movement 03.wav",
            "SOUNDS\\water movement 04.wav",
            "SOUNDS\\PRONE TO CROUCH.WAV",
            "SOUNDS\\CROUCH TO PRONE.WAV",
            "SOUNDS\\CROUCH TO STAND.WAV",
            "SOUNDS\\STAND TO CROUCH.WAV",
            "SOUNDS\\picking something up.WAV",
            "SOUNDS\\cow falling.wav",
            "SOUNDS\\bloodcat_growl_01.wav",
            "SOUNDS\\bloodcat_growl_02.wav",
            "SOUNDS\\bloodcat_growl_03.wav",
            "SOUNDS\\bloodcat_growl_04.wav",
            "SOUNDS\\spit ricochet.wav",
            "SOUNDS\\ADULT crippled.WAV",
            "SOUNDS\\death disintegration.wav",
            "SOUNDS\\Queen Ambience.wav",
            "SOUNDS\\Alien Impact.wav",
            "SOUNDS\\crow pecking flesh 01.wav",
            "SOUNDS\\crow fly.wav",
            "SOUNDS\\slap 02.wav",
            "SOUNDS\\setting up mortar.wav",
            "SOUNDS\\mortar whistle.wav",
            "SOUNDS\\load mortar.wav",
            "SOUNDS\\tank turret a.wav",
            "SOUNDS\\tank turret b.wav",
            "SOUNDS\\cow falling b.wav",
            "SOUNDS\\stab into flesh.wav",
            "SOUNDS\\explosion 10.wav",
            "SOUNDS\\explosion 12.wav",
            "SOUNDS\\drink from canteen male.WAV",
            "SOUNDS\\x ray activated.WAV",
            "SOUNDS\\catch object.wav",
            "SOUNDS\\fence open.wav",
        };

        public static string[] szAmbientEffects = new string[(int)AmbientDefines.NUM_AMBIENTS]
        {
            "SOUNDS\\storm1.wav",
            "SOUNDS\\storm2.wav",
            "SOUNDS\\rain_loop_22k.wav",
            "SOUNDS\\bird1-22k.wav",
            "SOUNDS\\bird3-22k.wav",
            "SOUNDS\\crickety_loop.wav",
            "SOUNDS\\crickety_loop2.wav",
            "SOUNDS\\cricket1.wav",
            "SOUNDS\\cricket2.wav",
            "SOUNDS\\owl1.wav",
            "SOUNDS\\owl2.wav",
            "SOUNDS\\owl3.wav",
            "SOUNDS\\night_bird1.wav",
            "SOUNDS\\night_bird3.wav"
        };

        public static byte[] AmbientVols = new byte[(int)AmbientDefines.NUM_AMBIENTS]
        {
            25,		// lightning 1
        	25,		// lightning 2
        	10,		// rain 1
        	25,		// bird 1
        	25,		// bird 2
        	10,		// crickets 1
        	10,		// crickets 2
        	25,		// cricket 1
        	25,		// cricket 2
        	25,		// owl 1
        	25,		// owl 2
        	25,		// owl 3
        	25,		// night bird 1
        	25      // night bird 2
        };
    }

    public enum AmbientDefines
    {
        LIGHTNING_1 = 0,
        LIGHTNING_2,
        RAIN_1,
        BIRD_1,
        BIRD_2,
        CRICKETS_1,
        CRICKETS_2,
        CRICKET_1,
        CRICKET_2,
        OWL_1,
        OWL_2,
        OWL_3,
        NIGHT_BIRD_1,
        NIGHT_BIRD_2,

        NUM_AMBIENTS
    };

    [Flags]
    public enum SoundPans
    {
        FARLEFT = 0,
        LEFTSIDE = 48,
        MIDDLE = 64,
        MIDDLEPAN = 64,
        RIGHTSIDE = 80,
        FARRIGHT = 127,
    }

    // SOUNDS ENUMERATION
    public enum SoundDefine
    {
        MISS_1 = 0,
        MISS_2,
        MISS_3,
        MISS_4,
        MISS_5,
        MISS_6,
        MISS_7,
        MISS_8,
        MISS_G1,
        MISS_G2,
        MISS_KNIFE,
        FALL_1,
        FALL_2,
        FALL_TO_GROUND_1,
        FALL_TO_GROUND_2,
        FALL_TO_GROUND_3,
        HEAVY_FALL_1,
        BODY_SPLAT_1,
        GLASS_SHATTER1,
        GLASS_SHATTER2,
        DROPEN_1,
        DROPEN_2,
        DROPEN_3,
        DRCLOSE_1,
        DRCLOSE_2,
        UNLOCK_DOOR_1,
        KICKIN_DOOR,
        BREAK_LOCK,
        PICKING_LOCK,

        GARAGE_DOOR_OPEN,
        GARAGE_DOOR_CLOSE,
        ELEVATOR_DOOR_OPEN,
        ELEVATOR_DOOR_CLOSE,
        HITECH_DOOR_OPEN,
        HITECH_DOOR_CLOSE,
        CURTAINS_OPEN,
        CURTAINS_CLOSE,
        METAL_DOOR_OPEN,
        METAL_DOOR_CLOSE,

        WALK_LEFT_OUT,
        WALK_RIGHT_OUT,
        WALK_LEFT_OUT2,
        WALK_RIGHT_OUT2,
        WALK_LEFT_IN,
        WALK_RIGHT_IN,
        WALK_LEFT_IN2,
        WALK_RIGHT_IN2,
        WALK_LEFT_ROAD,
        WALK_RIGHT_ROAD,
        WALK_LEFT_ROAD2,
        WALK_RIGHT_ROAD2,
        CRAWL_1,
        CRAWL_2,
        CRAWL_3,
        CRAWL_4,
        TARG_REFINE_BEEP,
        ENDTURN_1,
        HEADCR_1,
        DOORCR_1,
        HEADSPLAT_1,
        BODY_EXPLODE_1,
        EXPLOSION_1,
        CROW_EXPLODE_1,
        SMALL_EXPLODE_1,
        HELI_1,
        BULLET_IMPACT_1,
        BULLET_IMPACT_2,
        BULLET_IMPACT_3,
        CREATURE_BATTLECRY_1,
        ENTER_WATER_1,
        ENTER_DEEP_WATER_1,

        COW_HIT_SND,
        COW_DIE_SND,

        // ROCKET GUN COMPUTER VOICE...
        RG_ID_IMPRINTED,
        RG_ID_INVALID,
        RG_TARGET_SELECTED,

        // CAVE COLLAPSE
        CAVE_COLLAPSE,

        // AIR RAID SOUNDS...
        S_RAID_WHISTLE,
        S_RAID_AMBIENT,
        S_RAID_DIVE,
        S_RAID_TB_DIVE,
        S_RAID_TB_BOMB,

        // VEHICLE SOUNDS
        S_VECH1_MOVE,
        S_VECH1_ON,
        S_VECH1_OFF,
        S_VECH1_INTO,

        S_DRYFIRE1,

        // IMPACT SOUNDS
        S_WOOD_IMPACT1,
        S_WOOD_IMPACT2,
        S_WOOD_IMPACT3,
        S_PORCELAIN_IMPACT1,
        S_RUBBER_IMPACT1,
        S_STONE_IMPACT1,
        S_WATER_IMPACT1,
        S_VEG_IMPACT1,
        S_METAL_IMPACT1,
        S_METAL_IMPACT2,
        S_METAL_IMPACT3,

        S_SLAP_IMPACT,

        // WEAPON RELOAD
        S_RELOAD_REVOLVER,
        S_RELOAD_PISTOL,
        S_RELOAD_SMG,
        S_RELOAD_RIFLE,
        S_RELOAD_SHOTGUN,
        S_RELOAD_LMG,

        // WEAPON LOCKNLOAD
        S_LNL_REVOLVER,
        S_LNL_PISTOL,
        S_LNL_SMG,
        S_LNL_RIFLE,
        S_LNL_SHOTGUN,
        S_LNL_LMG,

        //WEAPON SHOT SOUNDS
        S_SMALL_ROCKET_LAUNCHER,
        S_GLAUNCHER,
        S_UNDER_GLAUNCHER,
        S_ROCKET_LAUNCHER,
        S_MORTAR_SHOT,
        S_GLOCK17,
        S_GLOCK18,
        S_BERETTA92,
        S_BERETTA93,
        S_SWSPECIAL,
        S_BARRACUDA,
        S_DESERTEAGLE,
        S_M1911,
        S_MP5K,
        S_MAC10,
        S_THOMPSON,
        S_COMMANDO,
        S_MP53,
        S_AKSU74,
        S_P90,
        S_TYPE85,
        S_SKS,
        S_DRAGUNOV,
        S_M24,
        S_AUG,
        S_G41,
        S_RUGERMINI,
        S_C7,
        S_FAMAS,
        S_AK74,
        S_AKM,
        S_M14,
        S_FNFAL,
        S_G3A3,
        S_G11,
        S_M870,
        S_SPAS,
        S_CAWS,
        S_FNMINI,
        S_RPK74,
        S_21E,
        S_THROWKNIFE,
        S_TANK_CANNON,
        S_BURSTTYPE1,
        S_AUTOMAG,

        S_SILENCER_1,
        S_SILENCER_2,

        // SWOOSHES.
        SWOOSH_1,
        SWOOSH_2,
        SWOOSH_3,
        SWOOSH_4,
        SWOOSH_5,
        SWOOSH_6,

        // CREATURE SOUNDS....
        ACR_FALL_1,
        ACR_STEP_1,
        ACR_STEP_2,
        ACR_SWIPE,
        ACR_EATFLESH,
        ACR_CRIPPLED,
        ACR_DIE_PART1,
        ACR_DIE_PART2,
        ACR_LUNGE,
        ACR_SMELL_THREAT,
        ACR_SMEEL_PREY,
        ACR_SPIT,
        //BABY
        BCR_DYING,
        BCR_DRAGGING,
        BCR_SHRIEK,
        BCR_SPITTING,
        // LARVAE
        LCR_MOVEMENT,
        LCR_RUPTURE,
        // QUEEN
        LQ_SHRIEK,
        LQ_DYING,
        LQ_ENRAGED_ATTACK,
        LQ_RUPTURING,
        LQ_CRIPPLED,
        LQ_SMELLS_THREAT,
        LQ_WHIP_ATTACK,

        THROW_IMPACT_1,
        THROW_IMPACT_2,

        IDLE_SCRATCH,
        IDLE_ARMPIT,
        IDLE_BACKCRACK,

        AUTORESOLVE_FINISHFX,

        //Interface buttons, misc.
        EMAIL_ALERT,
        ENTERING_TEXT,
        REMOVING_TEXT,
        COMPUTER_BEEP2_IN,
        COMPUTER_BEEP2_OUT,
        COMPUTER_SWITCH1_IN,
        COMPUTER_SWITCH1_OUT,
        VSM_SWITCH1_IN,
        VSM_SWITCH1_OUT,
        VSM_SWITCH2_IN,
        VSM_SWITCH2_OUT,
        SM_SWITCH1_IN,
        SM_SWITCH1_OUT,
        SM_SWITCH2_IN,
        SM_SWITCH2_OUT,
        SM_SWITCH3_IN,
        SM_SWITCH3_OUT,
        BIG_SWITCH3_IN,
        BIG_SWITCH3_OUT,
        KLAXON_ALARM,
        BOXING_BELL,
        HELI_CRASH,
        ATTACH_TO_GUN,
        ATTACH_CERAMIC_PLATES,
        ATTACH_DETONATOR,

        GRAB_ROOF,
        LAND_ON_ROOF,

        UNSTEALTHY_OUTSIDE_1,
        UNSTEALTHY_OUTSIDE_2,
        UNSTEALTHY_INSIDE_1,

        OPEN_DEFAULT_OPENABLE,
        CLOSE_DEFAULT_OPENABLE,

        FIRE_ON_MERC,
        GLASS_CRACK,
        SPIT_RICOCHET,

        BLOODCAT_HIT_1,
        BLOODCAT_DIE_1,
        SLAP_1,

        ROBOT_BEEP,
        DOOR_ELECTRICITY,
        SWIM_1,
        SWIM_2,
        KEY_FAILURE,
        TARGET_OUT_OF_RANGE,
        OPEN_STATUE,
        USE_STATUE_REMOTE,
        USE_WIRE_CUTTERS,
        DRINK_CANTEEN_FEMALE,
        BLOODCAT_ATTACK,
        BLOODCAT_ROAR,
        ROBOT_GREETING,
        ROBOT_DEATH,
        GAS_EXPLODE_1,
        AIR_ESCAPING_1,

        OPEN_DRAWER,
        CLOSE_DRAWER,
        OPEN_LOCKER,
        CLOSE_LOCKER,
        OPEN_WOODEN_BOX,
        CLOSE_WOODEN_BOX,
        ROBOT_STOP,

        WATER_WALK1_IN,
        WATER_WALK1_OUT,
        WATER_WALK2_IN,
        WATER_WALK2_OUT,

        PRONE_UP_SOUND,
        PRONE_DOWN_SOUND,
        KNEEL_UP_SOUND,
        KNEEL_DOWN_SOUND,
        PICKING_SOMETHING_UP,

        COW_FALL,

        BLOODCAT_GROWL_1,
        BLOODCAT_GROWL_2,
        BLOODCAT_GROWL_3,
        BLOODCAT_GROWL_4,
        CREATURE_GAS_NOISE,
        CREATURE_FALL_PART_2,
        CREATURE_DISSOLVE_1,
        QUEEN_AMBIENT_NOISE,
        CREATURE_FALL,
        CROW_PECKING_AT_FLESH,
        CROW_FLYING_AWAY,
        SLAP_2,
        MORTAR_START,
        MORTAR_WHISTLE,
        MORTAR_LOAD,

        TURRET_MOVE,
        TURRET_STOP,
        COW_FALL_2,
        KNIFE_IMPACT,
        EXPLOSION_ALT_BLAST_1,
        EXPLOSION_BLAST_2,
        DRINK_CANTEEN_MALE,
        USE_X_RAY_MACHINE,
        CATCH_OBJECT,
        FENCE_OPEN,

        NUM_SAMPLES,

        NO_WEAPON_SOUND = 0,
    };
}
