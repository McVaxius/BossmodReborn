namespace BossMod.Shadowbringers.Savage.E4STitan.Phase2
{
    public enum OID : uint
    {
        Boss = 0x2990, // ???
        Actor1ea1a1 = 0x1EA1A1, // R2.000, x8, EventObj type
        Helper = 0x233C, // ???
        Exit = 0x1E850B, // R0.500, x1, EventObj type
        Titan = 0x298F, // R4.999, x1
    };

public enum AID : uint
{
        AutoAttack_Attack = 16702,      // Boss->player, no cast, single-target
        DualEarthenFists = 16693,       // Boss->location, 4.3s cast, range 10 circle
        DualEarthenFists2 = 16694,      // Helper->self, 5.0s cast, range 100 circle
        DualEarthenFists3 = 18055,      // Helper->self, 5.4s cast, range 10 circle
        EarthenAnguish = 16695,         // Helper->players, no cast, range 10 circle
        EarthenFist = 16688,            // Boss->self, 7.0s cast, range 42 width 0 rect
        EarthenFist2 = 16689,           // Boss->self, 7.0s cast, range 42 width 0 rect
        EarthenFist3 = 16690,           // Boss->self, 7.0s cast, range 42 width 0 rect
        EarthenFist4 = 16691,           // Boss->self, no cast, range 42 width 0 rect
        EarthenFist5 = 17354,           // Helper->self, 1.0s cast, range 42 width 22 rect
        EarthenFist6 = 17353,           // Helper->self, 1.0s cast, range 42 width 22 rect
        EarthenFury = 16676,            // Boss->self, 6.0s cast, single-target
        EarthenFury2 = 17384,           // Helper->self, 7.5s cast, range 100 circle
        ForceOfTheLand = 16647,         // Helper->self, no cast, ???
        Megalith = 16696,               // Boss->players, 5.0s cast, range 5 circle
        PulseOfTheLand = 16646,         // Helper->self, no cast, ???
        TectonicUplift = 16674,         // Boss->self, 6.0s cast, single-target
        TectonicUplift2 = 16675,        // Helper->self, 7.0s cast, range 20 width 20 rect
        WeightOfTheLand = 16648,        // Helper->self, 5.0s cast, range 10 width 10 rect
        WeightOfTheLand2 = 17616,       // Helper->self, 11.0s cast, range 10 width 10 rect
        WeightOfTheWorld = 16681,       // Helper->self, no cast, range 10 width 10 rect
        WeightOfTheWorld2 = 18118,      // Helper->self, no cast, range 10 width 10 rect
        unknownskill_1 = 17454,         // Boss->self, no cast, single-target
        unknownskill_2 = 17021,         // Helper->self, 7.0s cast, range 3 width 3 rect};
    };

    public enum SID : uint
    {
        BrinkOfDeath = 44, // none->player, extra=0x0
        Transcendent = 418, // none->player, extra=0x0
        Weakness = 43, // none->player, extra=0x0
        VulnerabilityUp = 202, // Helper->player, extra=0x1
        DamageDown = 696, // Helper->player, extra=0x0
        MagicVulnerabilityUp = 1138, // Helper->player, extra=0x0

    };
    public enum IconID : uint
    {
        Icon_187 = 187, // player
        Icon_93 = 93, // player
        Icon_185 = 185, // player
        Icon_186 = 186, // player
    };
    class E4STitan2States : StateMachineBuilder
        {
            public E4STitan2States(BossModule module) : base(module)
            {
            TrivialPhase();
            }
        }

    [ModuleInfo(CFCID = 716, NameID = 8349)]
    public class E4STitan2 : BossModule
    {
        public E4STitan2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
