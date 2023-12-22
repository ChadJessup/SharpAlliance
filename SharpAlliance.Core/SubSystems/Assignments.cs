using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public enum Assignments
{
    SQUAD_1 = 0,
    SQUAD_2,
    SQUAD_3,
    SQUAD_4,
    SQUAD_5,
    SQUAD_6,
    SQUAD_7,
    SQUAD_8,
    SQUAD_9,
    SQUAD_10,
    SQUAD_11,
    SQUAD_12,
    SQUAD_13,
    SQUAD_14,
    SQUAD_15,
    SQUAD_16,
    SQUAD_17,
    SQUAD_18,
    SQUAD_19,
    SQUAD_20,
    ON_DUTY,
    DOCTOR,
    PATIENT,
    VEHICLE,
    IN_TRANSIT,
    REPAIR,
    TRAIN_SELF,
    TRAIN_TOWN,
    TRAIN_TEAMMATE,
    TRAIN_BY_OTHER,
    ASSIGNMENT_DEAD,
    ASSIGNMENT_UNCONCIOUS,          // unused
    ASSIGNMENT_POW,
    ASSIGNMENT_HOSPITAL,
    ASSIGNMENT_EMPTY,
    NO_ASSIGNMENT = 127, //used when no pSoldier.ubDesiredSquad

    SLEEPING = 172, // chad: added because original code intermixed Assignments and AnimationStates.
};


