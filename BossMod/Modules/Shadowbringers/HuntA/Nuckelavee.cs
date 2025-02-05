// CONTRIB: made by malediktus, not checked
using System.Collections.Generic;

namespace BossMod.Shadowbringers.HuntA.Nuckelavee
{
    public enum OID : uint
    {
        Boss = 0x288F, // R=3.6
    };

    public enum AID : uint
    {
        AutoAttack = 872, // 288F->player, no cast, single-target
        Torpedo = 16964, // 288F->player, 4,0s cast, single-target, tankbuster on cast event
        BogBody = 16965, // 288F->player, 5,0s cast, range 5 circle, spread, applies bleed that can be dispelled
        Gallop = 16967, // 288F->location, 4,5s cast, rushes to target and casts Spite
        Spite = 18037, // 288F->self, no cast, range 8 circle
    };

    class Torpedo : Components.SingleTargetCast
    { //Tankbuster resolves on cast event instead of cast finished
        private List<Actor> _casters = new();
        public new IReadOnlyList<Actor> Casters => _casters;
        public new bool Active => _casters.Count > 0;

        public Torpedo() : base(ActionID.MakeSpell(AID.Torpedo)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active)
                hints.Add("Tankbuster");
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)  { }
    }

    class BogBody : Components.SpreadFromCastTargets
    {
        public BogBody() : base(ActionID.MakeSpell(AID.BogBody), 5) { }
    }

    class Spite : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Gallop)
                _aoe = new(new AOEShapeCircle(8), spell.LocXZ, activation: spell.NPCFinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Spite)
                _aoe = null;
        }
    }

    class NuckelaveeStates : StateMachineBuilder
    {
        public NuckelaveeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Torpedo>()
                .ActivateOnEnter<BogBody>()
                .ActivateOnEnter<Spite>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 114)]
    public class Nuckelavee(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
