using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public class AIInternals
{
}

public struct ATTACKTYPE
{
    public int ubPossible;            // is this attack form possible?  T/F
    public int ubOpponent;            // which soldier is the victim?
    public int ubAimTime;                            // how many extra APs to spend on aiming
    public int ubChanceToReallyHit;   // chance to hit * chance to get through cover
    public int iAttackValue;          // relative worthiness of this type of attack
    public int sTarget;                              // target gridno of this attack
    public int bTargetLevel;                  // target level of this attack
    public int ubAPCost;                         // how many APs the attack will use up
    public int bWeaponIn;							// the inv slot of the weapon in question
}

public enum URGENCY
{
    LOW = 0,
    MED,
    HIGH,
    NUM_URGENCY_STATES
};
