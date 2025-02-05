namespace BossMod.Shadowbringers.HuntS.Gunitt
{
    public enum OID : uint
    {
        Boss = 0x2852, // R=4.0
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 2852->player, no cast, single-target
        TheDeepSeeks = 17356, // 2852->player, 4,0s cast, single-target
        TheDeepReaches = 17357, // 2852->self, 4,0s cast, range 40 width 2 rect
        TheDeepBeckons = 17358, // 2852->self, 4,0s cast, range 40 circle
        Abordage = 17359, // 2852->players, no cast, width 8 rect charge, seems to target random player before stack marker, no telegraph?
        SwivelGun = 17361, // 2852->players, 5,0s cast, range 10 circle, stack marker, applies magic vuln up, 3 times in a row
        CoinToss = 17360, // 2852->self, 4,0s cast, range 40 circle, gaze, applies Seduced (forced march to boss)
        TheDeepRends = 17351, // 2852->self, 5,5s cast, range 20 60-degree cone
        TheDeepRends2 = 17352, // 2852->self, no cast, range 20 60-degree cone, seems to target 5 random players after first The Deep Rends, no telegraph?
    };

    public enum SID : uint
    {
        MagicVulnerabilityUp = 1138, // Boss->player, extra=0x0
        Seduced = 227, // Boss->player, extra=0x0
    };

    public enum IconID : uint
    {
        Stackmarker = 93, // player
    };


    class TheDeepSeeks : Components.SingleTargetCast
    {
        public TheDeepSeeks() : base(ActionID.MakeSpell(AID.TheDeepSeeks)) { }
    }

    class TheDeepReaches : Components.SelfTargetedAOEs
    {
        public TheDeepReaches() : base(ActionID.MakeSpell(AID.TheDeepReaches), new AOEShapeRect(40, 1)) { }
    }

    class TheDeepBeckons : Components.RaidwideCast
    {
        public TheDeepBeckons() : base(ActionID.MakeSpell(AID.TheDeepBeckons)) { }
    }

    class CoinToss : Components.CastGaze
    {
        public CoinToss() : base(ActionID.MakeSpell(AID.CoinToss)) { }
    }

    class TheDeepRends : Components.SelfTargetedAOEs
    {
        public TheDeepRends() : base(ActionID.MakeSpell(AID.TheDeepRends), new AOEShapeCone(20, 30.Degrees())) { }
    }

    class TheDeepRendsHint : Components.CastHint
    {
        public TheDeepRendsHint() : base(ActionID.MakeSpell(AID.TheDeepRends), "Targets 5 random players after initial hit") { }
    }

    class SwivelGun : Components.GenericStackSpread
    {
        private BitMask _forbidden;

        public override void Update(BossModule module)
        {
            if (Stacks.Count > 0) //updating forbiddenplayers because debuffs can be applied after new stack marker appears
            {
                var Forbidden = Stacks[0];
                Forbidden.ForbiddenPlayers = _forbidden;
                Stacks[0] = Forbidden;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Stackmarker)
                Stacks.Add(new(actor, 10, activation: module.WorldState.CurrentTime.AddSeconds(5), forbiddenPlayers: _forbidden));
        }
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.MagicVulnerabilityUp)
                _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.MagicVulnerabilityUp)
                _forbidden.Clear(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SwivelGun)
                Stacks.RemoveAt(0);
        }
    }

    class GunittStates : StateMachineBuilder
    {
        public GunittStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TheDeepBeckons>()
                .ActivateOnEnter<TheDeepReaches>()
                .ActivateOnEnter<TheDeepRends>()
                .ActivateOnEnter<TheDeepRendsHint>()
                .ActivateOnEnter<CoinToss>()
                .ActivateOnEnter<SwivelGun>()
                .ActivateOnEnter<TheDeepSeeks>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 141)]
    public class Gunitt : SimpleBossModule
    {
        public Gunitt(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
