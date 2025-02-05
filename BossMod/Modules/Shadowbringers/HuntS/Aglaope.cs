using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.HuntS.Aglaope
{
    public enum OID : uint
    {
        Boss = 0x281E, // R=2.4
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        FourfoldSuffering = 16819, // Boss->self, 5,0s cast, range 5-50 donut
        SeductiveSonata = 16824, // Boss->self, 3,0s cast, range 40 circle, applies Seduced for 6s (forced march towards boss at 1.7y/s)
        DeathlyVerse = 17074, // Boss->self, 5,0s cast, range 6 circle (right after Seductive Sonata, instant kill), 6*1.7 = 10.2 + 6 = 16.2y minimum distance to survive
        Tornado = 18040, // Boss->location, 3,0s cast, range 6 circle
        AncientAero = 16823, // Boss->self, 3,0s cast, range 40+R width 6 rect
        SongOfTorment = 16825, // Boss->self, 5,0s cast, range 50 circle, interruptible raidwide with bleed
        AncientAeroIII = 18056, // Boss->self, 3,0s cast, range 30 circle, knockback 10, away from source
    };

    public enum SID : uint
    {
        Seduced = 991, // Boss->player, extra=0x11
        Bleeding = 642, // Boss->player, extra=0x0
    };

    class SongOfTorment : Components.CastInterruptHint
    {
        public SongOfTorment() : base(ActionID.MakeSpell(AID.SongOfTorment), hintExtra: "(Raidwide + Bleed)") { }
    }

//TODO: ideally this AOE should just wait for Effect Results, since they can be delayed by over 2.1s, which would cause unknowning players and AI to run back into the death zone, 
//not sure how to do this though considering there can be anywhere from 0-32 targets with different time for effect results each
    class SeductiveSonata : Components.GenericAOEs
    {
        private DateTime _activation;
        private DateTime _time;
        private bool casting;
        private static readonly AOEShapeCircle circle = new(16.2f);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (casting || (_time != default && _time > module.WorldState.CurrentTime))
                yield return new(circle, module.PrimaryActor.Position, activation: _activation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SeductiveSonata)
            {
                casting = true;
                _activation = spell.NPCFinishAt;
                _time = spell.NPCFinishAt.AddSeconds(2.2f);
            }
        }

        public override void Update(BossModule module)
        {
            if (_time != default && _time < module.WorldState.CurrentTime)
            {
                _time = default;
                casting = false;
            }
        }
    }

    class DeathlyVerse : Components.SelfTargetedAOEs
    {
        public DeathlyVerse() : base(ActionID.MakeSpell(AID.DeathlyVerse), new AOEShapeCircle(6)) { }
    }

    class Tornado : Components.LocationTargetedAOEs
    {
        public Tornado() : base(ActionID.MakeSpell(AID.Tornado), 6) { }
    }

    class FourfoldSuffering : Components.SelfTargetedAOEs
    {
        public FourfoldSuffering() : base(ActionID.MakeSpell(AID.FourfoldSuffering), new AOEShapeDonut(5, 50)) { }
    }

    class AncientAero : Components.SelfTargetedAOEs
    {
        public AncientAero() : base(ActionID.MakeSpell(AID.AncientAero), new AOEShapeRect(42.4f, 3)) { }
    }

    class AncientAeroIII : Components.RaidwideCast
    {
        public AncientAeroIII() : base(ActionID.MakeSpell(AID.AncientAeroIII)) { }
    }

    class AncientAeroIIIKB : Components.KnockbackFromCastTarget
    {
        public AncientAeroIIIKB() : base(ActionID.MakeSpell(AID.AncientAeroIII), 10, shape: new AOEShapeCircle(30)) { }
    }

    class AglaopeStates : StateMachineBuilder
    {
        public AglaopeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SongOfTorment>()
                .ActivateOnEnter<SeductiveSonata>()
                .ActivateOnEnter<DeathlyVerse>()
                .ActivateOnEnter<Tornado>()
                .ActivateOnEnter<FourfoldSuffering>()
                .ActivateOnEnter<AncientAero>()
                .ActivateOnEnter<AncientAeroIII>()
                .ActivateOnEnter<AncientAeroIIIKB>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 131)]
    public class Aglaope : SimpleBossModule
    {
        public Aglaope(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
