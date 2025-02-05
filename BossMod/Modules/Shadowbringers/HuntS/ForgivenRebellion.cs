﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.HuntS.ForgivenRebellion
{
    public enum OID : uint
    {
        Boss = 0x28B6, // R=3.4
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        SanctifiedBlizzard = 17598, // Boss->self, 3,0s cast, range 40 45-degree cone
        RoyalDecree = 17597, // Boss->self, 4,0s cast, range 40 circle, raidwide
        SanctifiedBlizzardChain = 17628, // Boss->self, 5,0s cast, range 40 45-degree cone, seems to rotate 45° in a random direction, no AID or Icon to tell apart
        SanctifiedBlizzardChain2 = 17629, // Boss->self, 0,5s cast, range 40 45-degree cone
        SanctifiedBlizzardChain3 = 18080, // Boss->self, 0,5s cast, range 40 45-degree cone
        HeavenlyScythe = 17600, // Boss->self, 2,5s cast, range 10 circle
        Transference = 17611, // Boss->player, no cast, single-target, gap closer
        RotateCW = 18078, // Boss->self, 0,5s cast, single-target
        RotateCCW = 18079, // Boss->self, 0,5s cast, single-target
        HeavenlyCyclone = 18126, // Boss->self, 5,0s cast, range 28 180-degree cone
        HeavenlyCyclone1 = 18127, // Boss->self, 0,5s cast, range 28 180-degree cone
        HeavenlyCyclone2 = 18128, // Boss->self, 0,5s cast, range 28 180-degree cone
        Mindjack = 17599, // Boss->self, 4,0s cast, range 40 circle, applies forced march buffs
        RagingFire = 17601, // Boss->self, 5,0s cast, range 5-40 donut
        Interference = 17602, // Boss->self, 4,5s cast, range 28 180-degree cone
    };

    public enum SID : uint
    {
        AboutFace = 1959, // Boss->player, extra=0x0
        ForwardMarch = 1958, // Boss->player, extra=0x0
        RightFace = 1961, // Boss->player, extra=0x0
        LeftFace = 1960, // Boss->player, extra=0x0
        ForcedMarch = 1257, // Boss->player, extra=0x2/0x1/0x8/0x4
    };

    public enum IconID : uint
    {
        RotateCCW = 168, // Boss
        RotateCW = 167, // Boss
    };

    class SanctifiedBlizzardChain : Components.GenericRotatingAOE
    {
        private Angle _increment;
        private DateTime _activation;
        private Angle _rot1;
        private Angle _rot2;
        private static readonly AOEShapeCone _shape = new(40, 22.5f.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // direction seems to be server side until after first rotation
            if (_rot1 != default && Sequences.Count == 0 && NumCasts == 0)
            {
                yield return new(_shape, module.PrimaryActor.Position, _rot1, _activation, ImminentColor);
                yield return new(_shape, module.PrimaryActor.Position, _rot1 + 45.Degrees(), _activation, FutureColor);
                yield return new(_shape, module.PrimaryActor.Position, _rot1 - 45.Degrees(), _activation, FutureColor);
            }
            if (_rot1 != default && Sequences.Count == 0 && NumCasts == 1)
            {
                yield return new(_shape, module.PrimaryActor.Position, _rot1 + 45.Degrees(), _activation, ImminentColor);
                yield return new(_shape, module.PrimaryActor.Position, _rot1 - 45.Degrees(), _activation, ImminentColor);
            }
            foreach (var s in Sequences)
            {
                int num = Math.Min(s.NumRemainingCasts, s.MaxShownAOEs);
                var rot = s.Rotation;
                var time = s.NextActivation > module.WorldState.CurrentTime ? s.NextActivation : module.WorldState.CurrentTime;
                for (int i = 1; i < num; ++i)
                {
                    rot += s.Increment;
                    time = time.AddSeconds(s.SecondsBetweenActivations);
                    yield return new(s.Shape, s.Origin, rot, time, FutureColor);
                }
            }
            foreach (var s in Sequences)
                if (s.NumRemainingCasts > 0)
                    yield return new(s.Shape, s.Origin, s.Rotation, s.NextActivation, ImminentColor);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SanctifiedBlizzardChain)
            {
                _activation = spell.NPCFinishAt;
                _rot1 = spell.Rotation;
            }
            if ((AID)spell.Action.ID is AID.SanctifiedBlizzardChain2 or AID.SanctifiedBlizzardChain3)
            {
                if (NumCasts == 1)
                    _rot2 = spell.Rotation;
                if ((_rot1 - _rot2).Normalized().Rad > 0)
                    _increment = -45.Degrees();
                if ((_rot1 - _rot2).Normalized().Rad < 0)
                    _increment = 45.Degrees();
                if (Sequences.Count == 0)
                    Sequences.Add(new(_shape, module.PrimaryActor.Position, _rot2, _increment, _activation, 1.3f, 7));
            }
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SanctifiedBlizzardChain)
                ++NumCasts;
            if ((AID)spell.Action.ID is AID.SanctifiedBlizzardChain2 or AID.SanctifiedBlizzardChain3)
            {
                if (Sequences.Count > 0)
                    AdvanceSequence(0, module.WorldState.CurrentTime);
                if (NumCasts == 8)
                {
                    NumCasts = 0;
                    _rot1 = default;
                    _rot2 = default;
                }
            }
        }
    }

    class SanctifiedBlizzardChainHint : Components.RaidwideCast
    {
        public SanctifiedBlizzardChainHint() : base(ActionID.MakeSpell(AID.SanctifiedBlizzardChain), "Rotation direction undeterminable until start of the 2nd cast") { }
    }

    class HeavenlyCyclone : Components.GenericRotatingAOE
    {
        private Angle _increment;
        private Angle _rotation;
        private DateTime _activation;
        private static readonly AOEShapeCone _shape = new(28, 90.Degrees());

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var increment = (IconID)iconID switch
            {
                IconID.RotateCW => -90.Degrees(),
                IconID.RotateCCW => 90.Degrees(),
                _ => default
            };
            if (increment != default)
            {
                _increment = increment;
                InitIfReady(module, actor);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.RotateCW or AID.RotateCCW)
            {
                _rotation = spell.Rotation;
                _activation = spell.NPCFinishAt.AddSeconds(5.2f);
            }
            if (_rotation != default)
                InitIfReady(module, caster);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.HeavenlyCyclone or AID.HeavenlyCyclone1 or AID.HeavenlyCyclone2)
                AdvanceSequence(0, module.WorldState.CurrentTime);
        }

        private void InitIfReady(BossModule module, Actor source)
        {
            if (_rotation != default && _increment != default)
            {
                Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 1.7f, 6));
                _rotation = default;
                _increment = default;
            }
        }
    }

    class HeavenlyScythe : Components.SelfTargetedAOEs
    {
        public HeavenlyScythe() : base(ActionID.MakeSpell(AID.HeavenlyScythe), new AOEShapeCircle(10)) { }
    }

    class RagingFire : Components.SelfTargetedAOEs
    {
        public RagingFire() : base(ActionID.MakeSpell(AID.RagingFire), new AOEShapeDonut(5, 40)) { }
    }

    class Interference : Components.SelfTargetedAOEs
    {
        public Interference() : base(ActionID.MakeSpell(AID.Interference), new AOEShapeCone(28, 90.Degrees())) { }
    }

    class SanctifiedBlizzard : Components.SelfTargetedAOEs
    {
        public SanctifiedBlizzard() : base(ActionID.MakeSpell(AID.SanctifiedBlizzard), new AOEShapeCone(40, 22.5f.Degrees())) { }
    }

    class RoyalDecree : Components.RaidwideCast
    {
        public RoyalDecree() : base(ActionID.MakeSpell(AID.RoyalDecree)) { }
    }

   class MindJack : Components.StatusDrivenForcedMarch
    {
        public MindJack() : base(2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
        {
            if (module.FindComponent<Interference>() != null && module.FindComponent<Interference>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
                return true;
            if (module.FindComponent<RagingFire>() != null && module.FindComponent<RagingFire>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
                return true;
            else
                return false;
        }
    }

    class ForgivenRebellionStates : StateMachineBuilder
    {
        public ForgivenRebellionStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<MindJack>()
                .ActivateOnEnter<HeavenlyScythe>()
                .ActivateOnEnter<HeavenlyCyclone>()
                .ActivateOnEnter<RagingFire>()
                .ActivateOnEnter<Interference>()
                .ActivateOnEnter<SanctifiedBlizzard>()
                .ActivateOnEnter<SanctifiedBlizzardChain>()
                .ActivateOnEnter<SanctifiedBlizzardChainHint>()
                .ActivateOnEnter<RoyalDecree>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 146)]
    public class ForgivenRebellion : SimpleBossModule
    {
        public ForgivenRebellion(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
