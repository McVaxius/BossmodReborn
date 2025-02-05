using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.HuntS.Tarchia
{
    public enum OID : uint
    {
        Boss = 0x2873, // R=9.86
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        WakeUp = 18103, // Boss->self, no cast, single-target, visual for waking up from sleep
        WildHorn = 18026, // Boss->self, 3,0s cast, range 17 120-degree cone
        BafflementBulb = 18029, // Boss->self, 3,0s cast, range 40 circle, pull 50 between hitboxes, temporary misdirection
        ForestFire = 18030, // Boss->self, 5,0s cast, range 40 circle, damage fall off AOE, hard to tell optimal distance because logs are polluted by vuln stacks, guessing about 15
        MightySpin = 18028, // Boss->self, 3,0s cast, range 14 circle
        MightySpin2 = 18093, // Boss->self, no cast, range 14 circle, after 1s after boss wakes up and 4s after every Groundstorm
        Trounce = 18027, // Boss->self, 4,0s cast, range 40 60-degree cone
        MetamorphicBlast = 18031, // Boss->self, 4,0s cast, range 40 circle
        Groundstorm = 18023, // Boss->self, 5,0s cast, range 5-40 donut
    };

    class WildHorn : Components.SelfTargetedAOEs
    {
        public WildHorn() : base(ActionID.MakeSpell(AID.WildHorn), new AOEShapeCone(17, 60.Degrees())) { }
    }

    class Trounce : Components.SelfTargetedAOEs
    {
        public Trounce() : base(ActionID.MakeSpell(AID.Trounce), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class Groundstorm : Components.SelfTargetedAOEs
    {
        public Groundstorm() : base(ActionID.MakeSpell(AID.Groundstorm), new AOEShapeDonut(5, 40)) { }
    }

    class MightySpin : Components.SelfTargetedAOEs
    {
        public MightySpin() : base(ActionID.MakeSpell(AID.MightySpin), new AOEShapeCircle(14)) { }
    }

    class ForestFire : Components.SelfTargetedAOEs
    {
        public ForestFire() : base(ActionID.MakeSpell(AID.ForestFire), new AOEShapeCircle(15)) { }
    }

    class BafflementBulb : Components.CastHint
    {
        public BafflementBulb() : base(ActionID.MakeSpell(AID.BafflementBulb), "Pull + Temporary Misdirection -> Donut -> Out") { }
    }

    class MetamorphicBlast : Components.RaidwideCast
    {
        public MetamorphicBlast() : base(ActionID.MakeSpell(AID.MetamorphicBlast)) { }
    }

    class MightySpin2 : Components.GenericAOEs
    {
        private DateTime _activation;
        private static readonly AOEShapeCircle circle = new(14);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation != default || NumCasts == 0)
                yield return new(circle, module.PrimaryActor.Position, activation: _activation);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Groundstorm)
                _activation = spell.NPCFinishAt.AddSeconds(4);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.MightySpin2 or AID.Trounce or AID.AutoAttack or AID.MightySpin or AID.WildHorn or AID.Groundstorm or AID.BafflementBulb or AID.MetamorphicBlast or AID.Groundstorm) //everything but Mightyspin2 is a failsafe incase player joins fight/starts replay record late and Numcasts is 0 because of it
                ++NumCasts;
            if ((AID)spell.Action.ID == AID.MightySpin2)
                _activation = default;
        }
    }

    class TarchiaStates : StateMachineBuilder
    {
        public TarchiaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<MetamorphicBlast>()
                .ActivateOnEnter<MightySpin>()
                .ActivateOnEnter<WildHorn>()
                .ActivateOnEnter<Trounce>()
                .ActivateOnEnter<BafflementBulb>()
                .ActivateOnEnter<Groundstorm>()
                .ActivateOnEnter<ForestFire>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 126)]
    public class Tarchia : SimpleBossModule
    {
        public Tarchia(WorldState ws, Actor primary) : base(ws, primary) 
        {
            ActivateComponent<MightySpin2>();
        }
    }
}
