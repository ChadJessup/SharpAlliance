﻿using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static bool PTR_CIVILIAN(SOLDIERTYPE pSoldier) => (pSoldier.bTeam == CIV_TEAM);
    public static bool PTR_CROUCHED(SOLDIERTYPE pSoldier) => (gAnimControl[pSoldier.usAnimState].ubHeight == AnimationHeights.ANIM_CROUCH);
    public static bool PTR_STANDING(SOLDIERTYPE pSoldier) => (gAnimControl[pSoldier.usAnimState].ubHeight == AnimationHeights.ANIM_STAND);
    public static bool PTR_PRONE(SOLDIERTYPE pSoldier) => (gAnimControl[pSoldier.usAnimState].ubHeight == AnimationHeights.ANIM_PRONE);

    public static bool HAS_SKILL_TRAIT(SOLDIERTYPE s, SkillTrait t) => (s.ubSkillTrait1 == t || s.ubSkillTrait2 == t);
    public static int NUM_SKILL_TRAITS(SOLDIERTYPE s, SkillTrait t) => ((s.ubSkillTrait1 == t)
        ? ((s.ubSkillTrait2 == t) ? 2 : 1)
        : ((s.ubSkillTrait2 == t) ? 1 : 0));

    public static Dictionary<BATTLE_SOUND, BATTLESNDS_STRUCT> gBattleSndsData = new()
    {
        { BATTLE_SOUND.OK1, new() { zName = "ok1",    ubRandomVal = 2, fPreload =  true, fBadGuy = true, fDontAllowTwoInRow = true, fStopDialogue = 2 } },
        { BATTLE_SOUND.OK2, new() { zName = "ok2",    ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 2 } },
        { BATTLE_SOUND.COOL1 , new() { zName = "cool",   ubRandomVal = 0, fPreload =  true,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 0 } },
        { BATTLE_SOUND.CURSE1, new() { zName = "curse",  ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 0 } },
        { BATTLE_SOUND.HIT1, new() { zName = "hit1",   ubRandomVal = 2, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 1 } },
        { BATTLE_SOUND.HIT2, new() { zName = "hit2",   ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 1 } },
        { BATTLE_SOUND.LAUGH1, new() { zName = "laugh",  ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 0 } },
        { BATTLE_SOUND.ATTN1, new() { zName = "attn",   ubRandomVal = 0, fPreload =  true,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 0 } },
        { BATTLE_SOUND.DIE1, new() { zName = "dying",  ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 1 } },
        { BATTLE_SOUND.HUMM , new() { zName = "humm",   ubRandomVal = 0, fPreload = false,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 1 } },
        { BATTLE_SOUND.NOTHING, new() { zName = "noth",   ubRandomVal = 0, fPreload = false,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 1 } },
        { BATTLE_SOUND.GOTIT, new() { zName = "gotit",  ubRandomVal = 0, fPreload = false,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 1 } },
        { BATTLE_SOUND.LOWMARALE_OK1, new() { zName = "lmok1",  ubRandomVal = 2, fPreload =  true,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 2 } },
        { BATTLE_SOUND.LOWMARALE_OK2, new() { zName = "lmok2",  ubRandomVal = 0, fPreload =  true,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 2 } },
        { BATTLE_SOUND.LOWMARALE_ATTN1, new() { zName = "lmattn", ubRandomVal = 0, fPreload =  true,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 0 } },
        { BATTLE_SOUND.LOCKED, new() { zName = "locked", ubRandomVal = 0, fPreload = false,fBadGuy = false, fDontAllowTwoInRow = true, fStopDialogue = 0 } },
        { BATTLE_SOUND.ENEMY, new() { zName = "enem",   ubRandomVal = 0, fPreload =  true,fBadGuy = true, fDontAllowTwoInRow = true,  fStopDialogue = 0 } },
    };
}


public struct BATTLESNDS_STRUCT
{
    public string zName;// [20];
    public int ubRandomVal;
    public bool fPreload;
    public bool fBadGuy;
    public bool fDontAllowTwoInRow;
    public int fStopDialogue;
}