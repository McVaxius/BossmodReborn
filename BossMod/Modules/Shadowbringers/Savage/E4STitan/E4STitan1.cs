using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.GeneratedSheets;
using static BossMod.ActorCastEvent;
using static BossMod.Components.GenericStackSpread;

//everything we need to understand the mechanics is in here
//https://docs.google.com/document/d/140rWqqHNBdbmBY-MzQAlRdpqYja8N2j5b4bvC-37caI/edit

namespace BossMod.Shadowbringers.Savage.E4STitan.Phase1
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */
    public enum OID : uint
    {
        Boss = 0x298F, // ???
        GraniteGaol = 0x2A4F, //spawned prefight 1 radius 36
        MassiveBoulder = 0x2992, //spawned prefight 0 radius 2.210
        BombBoulder = 0x2991, //spawned prefight 0 radius 1.300
        Actor1ea1a1 = 0x1EA1A1, // R2.000, x8, EventObj type
        Helper = 0x233C , // ???
        TitanMaximum = 0x2990, // R19.980, x1
        Exit = 0x1E850B, // R0.500, x1, EventObj type
    };

    public enum AID : uint
        {
            Aftershock = 16821, 			// Helper->self, no cast, range 10 width 10 rect
            Aftershock2 = 16653, 			// Helper->self, no cast, range 10 width 10 rect
            Attack2 = 16702, 			    // TitanMaximum->player, no cast, single-target
            AutoAttack_Attack = 16701, 		// Boss->player, no cast, single-target
            BombBoulders = 16649, 			// Boss->self, 5.0s cast, single-target
            Bury = 16706, 			        // BombBoulder->self, no cast, range 3+R circle
            CrumblingDown = 16654, 			// Boss->self, 5.0s cast, single-target
            CrumblingDown2 = 16655, 		//done. MassiveBoulder->self, 5.0s cast, range 60 circle
            DualEarthenFists = 16693, 		//done. TitanMaximum->location, 4.3s cast, range 10 circle
            DualEarthenFists2 = 16694, 		//done. Helper->self, 5.0s cast, range 100 circle
            DualEarthenFists3 = 18055, 		//done. Helper->self, 5.4s cast, range 10 circle
            EarthenAnguish = 16695, 		// Helper->players, no cast, range 10 circle
            EarthenArmor = 16617, 			// Boss->self, no cast, single-target
            EarthenArmor2 = 16615, 			// Boss->self, no cast, single-target
            EarthenFist = 16689, 			// TitanMaximum->self, 7.0s cast, range 42 width 0 rect //this is kind of telegraph for earthenfist4 so can think about it
            EarthenFist2 = 16690,           // TitanMaximum->self, 7.0s cast, range 42 width 0 rect //this is kind of telegraph for earthenfist4 so can think about it
            EarthenFist3 = 16688,           // TitanMaximum->self, 7.0s cast, range 42 width 0 rect //this is kind of telegraph for earthenfist4 so can think about it
            EarthenFist4 = 17354, 			//done. Helper->self, 1.0s cast, range 42 width 22 rect
            EarthenFist5 = 16691, 			// TitanMaximum->self, no cast, range 42 width 0 rect   // telegraph for earthenfist6?
            EarthenFist6 = 17353, 			//done. Helper->self, 1.0s cast, range 42 width 22 rect
            EarthenFury = 16676, 			// TitanMaximum->self, 6.0s cast, single-target
            EarthenFury2 = 17384, 			//done. Helper->self, 7.5s cast, range 100 circle
            EarthenGauntlets = 16614, 		// Boss->self, no cast, single-target
            EarthenWheels = 16616, 			// Boss->self, no cast, single-target
            EvilEarth = 16651, 			    // Boss->self, 4.1s cast, single-target
            EvilEarth2 = 16652, 			//done Helper->self, 5.0s cast, range 10 width 10 rect
            Explosion = 16650, 			    //done. BombBoulder->self, 5.0s cast, range 10 circle
            FaultLine1 = 16670, 			//done. Boss->player, 3.0s cast, width 8 rect charge
            FaultLine2 = 16671, 			// Boss->location, no cast, ???
            FaultLine3 = 16672, 			//done. Helper->self, no cast, range 42 width 52 rect
            ForceOfTheLand = 16647, 		// Helper->self, no cast, ???
            Geocrush = 16659,               //done. Boss->location, 5.0s cast, range 100 circle
                                            //Geocrush causes Titan to leap towards a random segment of the arena and blast the raid with massive knockback
                                            //(far greater than the normal version). Before leaping, a visual effect of outward travelling arrows from the point of
                                            //impact will give you an idea of where Titan will land and in what direction you will be punted to. Due to the tremendous knockback,
                                            //players must ensure that they are close to the point of impact and aim themselves so that they get knocked back into the farthest corner of the arena.
                                            //If standing too far away, or aimed incorrectly, players will be launched over the side to their doom. However, bear in mind that knockback immunity will prevent
                                            //this knockback entirely.
            Landslide = 16666, 			    // Boss->location, 4.4s cast, ???
            Landslide2 = 16667, 			//done. Helper->self, 5.0s cast, range 100 width 20 cross
            LeftwardLandslide = 16668,      //done. Boss->self, 3.0s cast, range 21 width 77 rect
            RightwardLandslide = 16669,     //done. Boss->self, 3.0s cast, range 21 width 77 rect
            Magnitude5 = 16673, 			//done. Boss->self, 3.0s cast, range ?-60 donut
            MassiveLandslide = 16665, 	    // Helper->self, no cast, range 42 width 22 rect
            MassiveLandslide2 = 16664, 		// Helper->self, no cast, range 42 width 22 rect
            MassiveLandslide3 = 16663, 		// Boss->self, no cast, single-target
            Megalith = 16696, 			    //done. TitanMaximum->players, 5.0s cast, range 5 circle
            Orogenesis = 17265, 			// Boss->self, no cast, range 100 circle
            PulseOfTheLand = 16646, 		// Helper->self, no cast, ???
            SeismicWave = 16656, 			//done. Boss->self, 4.0s cast, range 100 circle
            Stonecrusher = 16662, 			//done. Boss->players, 5.0s cast, range 5 circle
            Stonecrusher2 = 16707, 			// Boss->player, no cast, range 5 circle
            TectonicUplift = 16674,         //TitanMaximum->self, 6.0s cast, single-target      //WTF. Tectonic Uplift marks two diagonal quadrants of the arena (4 tiles each) and eventually elevates them (along with any players standing on the quadrant at the time). Those who find themselves on elevated platforms can jump down to a lower platform if required, though it is impossible to traverse from one raised platform to the other, or from one lowered platform to another.
            TectonicUplift2 = 16675, 		// Helper->self, 7.0s cast, range 20 width 20 rect  //part two of WTF
            VoiceOfTheLand = 16660, 		//done Boss->self, 4.0s cast, range 100 circle
            WeightOfTheLand = 16645, 		//done. Boss->self, 2.1s cast, single-target
            WeightOfTheLand2 = 16648, 		//done. Helper->self, 5.0s cast, range 10 width 10 rect
            WeightOfTheLand3 = 17616, 		//done. Helper->self, 11.0s cast, range 10 width 10 rect
            WeightOfTheWorld = 16681, 		// Helper->self, no cast, range 10 width 10 rect
            WeightOfTheWorld2 = 18118, 		// Helper->self, no cast, range 10 width 10 rect
            unknownskill_2 = 19021, 		// Boss->location, no cast, single-target
            unknownskill_3 = 17454, 		// TitanMaximum->self, no cast, single-target
            unknownskill_4 = 17021, 		//done. Helper->self, 7.0s cast, range 3 width 3 rect //is this mini cleave?
        };

    public enum SID : uint
    {
        PhysicalVulnerabilityUp = 695, // Boss->player, extra=0x0
        MagicVulnerabilityUp = 1138, // Helper->player, extra=0x0
        VulnerabilityUp = 202, // Helper/Boss/2991->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8
        DamageDown = 696, // Helper/Boss/2991->player, extra=0x0
        HealingMagicDown = 697, // Helper/Boss/2991->player, extra=0x0
        Weakness = 43, // none->player, extra=0x0
        Transcendent = 418, // none->player, extra=0x0
        BrinkOfDeath = 44, // none->player, extra=0x0
    };

    public enum IconID : uint
    {
        Icon_23 = 23, // lockon3_t0h ?????????
        Icon_93 = 93, // com_share1f  shared buster
        IconStackDorito = 185, // m0561tag_a0t  orange (stack) yellow (spread) or blue (KB) marker
        IconSpreadCube = 186, // m0561tag_b0t  orange (stack) yellow (spread) or blue (KB) marker
        IconBlueKb = 187, // m0561tag_c0t  orange (stack) yellow (spread) or blue (KB) marker
    };
    class FaultLine1 : Components.ChargeAOEs
    {
        public FaultLine1() : base(ActionID.MakeSpell(AID.FaultLine1),4) { }
    } 
    class Landslide2 : Components.SelfTargetedAOEs
    {
        public Landslide2() : base(ActionID.MakeSpell(AID.Landslide2), new AOEShapeCross(100,10)) { }
    } 
    class WeightOfTheLand : Components.SelfTargetedAOEs
    {
        public WeightOfTheLand() : base(ActionID.MakeSpell(AID.WeightOfTheLand), new AOEShapeRect(10,5)) { }
    } 
    class WeightOfTheLand2 : Components.SelfTargetedAOEs
    {
        public WeightOfTheLand2() : base(ActionID.MakeSpell(AID.WeightOfTheLand2), new AOEShapeRect(10,5)) { }
    } 
    class WeightOfTheLand3 : Components.SelfTargetedAOEs
    {
        public WeightOfTheLand3() : base(ActionID.MakeSpell(AID.WeightOfTheLand3), new AOEShapeRect(10,5)) { }
    } 
    class FaultLine3 : Components.SelfTargetedAOEs
    {
        public FaultLine3() : base(ActionID.MakeSpell(AID.FaultLine3), new AOEShapeRect(42, 26)) { }
    } 
    class EvilEarth2 : Components.SelfTargetedAOEs
    {
        public EvilEarth2() : base(ActionID.MakeSpell(AID.EvilEarth2), new AOEShapeRect(10,5)) { }
    }
    class unknownskill_4 : Components.SelfTargetedAOEs
    {
        public unknownskill_4() : base(ActionID.MakeSpell(AID.unknownskill_4), new AOEShapeRect(3,1.5f)) { }
    }
    class EarthenFist6 : Components.SelfTargetedAOEs
    {
        public EarthenFist6() : base(ActionID.MakeSpell(AID.EarthenFist6), new AOEShapeRect(42,11)) { }
    }
    class EarthenFist4 : Components.SelfTargetedAOEs
    {
        public EarthenFist4() : base(ActionID.MakeSpell(AID.EarthenFist4), new AOEShapeRect(42,11)) { }
    }
    class DualEarthenFists3 : Components.SelfTargetedAOEs
    {
        public DualEarthenFists3() : base(ActionID.MakeSpell(AID.DualEarthenFists3), new AOEShapeCircle(10)) { }
    }
    class DualEarthenFists : Components.LocationTargetedAOEs
    {
        public DualEarthenFists() : base(ActionID.MakeSpell(AID.DualEarthenFists), 10) { }
    }
    class Geocrush : Components.KnockbackFromCastTarget
    {
        public WPos safePosition = new WPos();
        public bool yeetProtection = false;
        WPos kbShenanigans = new WPos();
        public float kbdist = 15;
        public Geocrush() : base(ActionID.MakeSpell(AID.Geocrush), 15)
        {
            StopAtWall = true;
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            yeetProtection = false;
            if ((AID)spell.Action.ID == AID.Geocrush)
            {
                yeetProtection = true;
                kbShenanigans.X = caster.Position.X;
                kbShenanigans.Z = caster.Position.Z;
                safePosition = CalculateSafePosition(kbShenanigans.X, kbShenanigans.Z, 100f, 100f, 40f, 8f, kbdist); // Example center coordinates of the circle
            }
        }
        public static WPos CalculateSafePosition(float circleCenterX, float circleCenterY, float platformCenterX, float platformCenterY, float platformSize, float circleRadius, float knockbackLength)
        {
            WPos returnSafeHack = new WPos();
            // Calculate distance between circle center and platform center
            double distance = Math.Sqrt(Math.Pow(circleCenterX - platformCenterX, 2) + Math.Pow(circleCenterY - platformCenterY, 2));

            // Calculate angle between circle center and platform center
            double angle = Math.Atan2(circleCenterY - platformCenterY, circleCenterX - platformCenterX);

            // Calculate the safe position
            double safeX = platformCenterX + (platformSize / 2) * Math.Cos(angle);
            double safeY = platformCenterY + (platformSize / 2) * Math.Sin(angle);

            // If the distance between circle center and platform center is greater than the safe distance,
            // adjust the safe position to the intersection point between the circle and the platform boundary
            if (distance > knockbackLength + circleRadius)
            {
                safeX = platformCenterX + (knockbackLength + circleRadius) * Math.Cos(angle);
                safeY = platformCenterY + (knockbackLength + circleRadius) * Math.Sin(angle);
            }
            returnSafeHack.X = (float)safeX;
            returnSafeHack.Z = (float)safeY;
            return returnSafeHack;
        }
        public static WPos CalculateSafeEndpoint(float circleCenterX, float circleCenterY, float angle, float knockbackLength)
        {
            WPos endpoint = new WPos();

            // Calculate endpoint of the knockback using trigonometry
            float endpointX = circleCenterX + knockbackLength * (float)Math.Cos(angle);
            float endpointY = circleCenterY + knockbackLength * (float)Math.Sin(angle);

            endpoint.X = endpointX;
            endpoint.Z = endpointY;

            return endpoint;
        }
        public static float KBangle(float circleCenterX, float circleCenterY, float platformCenterX, float platformCenterY)
        {
            // Calculate distance between circle center and platform center
            double distance = Math.Sqrt(Math.Pow(circleCenterX - platformCenterX, 2) + Math.Pow(circleCenterY - platformCenterY, 2));

            // Calculate angle between circle center and platform center
            double angle = Math.Atan2(circleCenterY - platformCenterY, circleCenterX - platformCenterX);
            angle = 180 - angle;
            if (angle < 0)
            {
                angle += 360;
            }
            return (float)angle;
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (yeetProtection == true)
            {
                //if (Casters.Count > 0)
                hints.Add("Prepare to get yeeted! GET TO THE GREEN DOT");
            }
        }
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (yeetProtection == true)
            {
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.ArmsLength), actor, 1, false));
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Surecast), actor, 1, false));
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(safePosition, 1f));
            }
        }
        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            WPos kbendpoint = new WPos();
            float kbangle;
            if (yeetProtection == true)
            {
                arena.AddCircleFilled(new WPos(safePosition.X, safePosition.Z), 1f, ArenaColor.SafeFromAOE);
                // Calculate endpoint of the knockback using trigonometry

                kbangle = KBangle(kbShenanigans.X, kbShenanigans.Z, 100, 100);
                /*
                // Calculate endpoint of the knockback using trigonometry
                kbendpoint = CalculateSafeEndpoint(safePosition.X, safePosition.Z, kbangle, kbdist);

                // Draw a line from player position to the endpoint of the knockback
                //debug this is buggy af
                arena.AddLine(pc.Position, kbendpoint, ArenaColor.SafeFromAOE);
                */
            }
        }
    }
    class SeismicWave : Components.RaidwideCast
    {
        public SeismicWave() : base(ActionID.MakeSpell(AID.SeismicWave)) { }
    }
    class EarthenFury2 : Components.RaidwideCast
    {
        public EarthenFury2() : base(ActionID.MakeSpell(AID.EarthenFury2)) { }
    }
    class VoiceOfTheLand : Components.RaidwideCast
    {
        public VoiceOfTheLand() : base(ActionID.MakeSpell(AID.VoiceOfTheLand)) { }
    }
    class DualEarthenFists2 : Components.RaidwideCast
    {
        public DualEarthenFists2() : base(ActionID.MakeSpell(AID.DualEarthenFists2)) { }
    }
    class CrumblingDown2 : Components.RaidwideCast
    {
        public CrumblingDown2() : base(ActionID.MakeSpell(AID.CrumblingDown2)) { }
        //public CrumblingDown2() : base(ActionID.MakeSpell(AID.CrumblingDown2), new AOEShapeCircle(60)) { }
    }
    class IconEnjoyers : BossComponent
    {
        private UniformStackSpread iconSpread;
        private UniformStackSpread iconStack;

        public IconEnjoyers()
        {
            iconSpread = new UniformStackSpread(0, 5, 2, 8, true, false, true);
            iconStack = new UniformStackSpread(5, 0, 2, 8, true, false, true);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            base.OnEventIcon(module, actor, iconID);

            if ((IconID)iconID == IconID.IconStackDorito)
            {
                DateTime spreadTime = DateTime.Now.AddSeconds(8); // Assuming you want to spread after 8 seconds
                iconStack.AddSpread(actor, spreadTime);
            }
            else if ((IconID)iconID == IconID.IconSpreadCube)
            {
                DateTime spreadTime = DateTime.Now.AddSeconds(8); // Assuming you want to spread after 8 seconds
                iconSpread.AddSpread(actor, spreadTime);
            }
            else if ((IconID)iconID == IconID.IconBlueKb)
            {
                // Handle IconBlueKb
            }
        }
    }

    class Explosion : Components.SelfTargetedAOEs
    {
        public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(10)) { }
    }
     class LeftwardLandslide : Components.SelfTargetedAOEs //this is bad way to do it. it should be making the rectangles appear @ helpers????
    {
        public LeftwardLandslide() : base(ActionID.MakeSpell(AID.LeftwardLandslide), new AOEShapeRect(77, 11)) { }
    }
    class RightwardLandslide : Components.SelfTargetedAOEs //this is bad way to do it. it should be making the rectangles appear @ helpers????
    {
        public RightwardLandslide() : base(ActionID.MakeSpell(AID.RightwardLandslide), new AOEShapeRect(77, 11)) { }
    }   
    class Magnitude5 : Components.SelfTargetedAOEs
    {
        public Magnitude5() : base(ActionID.MakeSpell(AID.Magnitude5), new AOEShapeDonut(5, 60)) { }
    }
    class Megalith : Components.SpreadFromCastTargets
    {
        public Megalith() : base(ActionID.MakeSpell(AID.Megalith),8) { }
    }
    class Stonecrusher : Components.SpreadFromCastTargets
    {
        public Stonecrusher() : base(ActionID.MakeSpell(AID.Stonecrusher),8) { }
    }

    /*class Megalith : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (actor.Role != Role.Tank && actor.IsAlly)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 8));
        }
        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (pc.Role != Role.Tank && pc.IsAlly)
                arena.AddCircleFilled(module.PrimaryActor.Position, 8, ArenaColor.Danger);
        }
    }
    class Stonecrusher : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (actor.Role != Role.Tank && actor.IsAlly)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 8));
        }
        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (pc.Role != Role.Tank && pc.IsAlly)
                arena.AddCircleFilled(module.PrimaryActor.Position, 8, ArenaColor.Danger);
        }
    }*/
    /*
    class WeightOfTheLand : Components.GenericAOEs //this is bad way to do it. it should be making the squares appear @ helpers and not just always on when the mechanic fires.. but this is ok for now
    {        
        private DateTime _activation;
        private static readonly AOEShapeRect rect = new(5, 2.5f, 5);
         public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.WeightOfTheLand2)
                {
                _activation = module.WorldState.CurrentTime;
                NumCasts = 0;
            }
            if ((AID)spell.Action.ID == AID.WeightOfTheLand3)
                {
                _activation = module.WorldState.CurrentTime;
                NumCasts = 0;
                }
            if ((AID)spell.Action.ID == AID.WeightOfTheLand)
            {
                ++NumCasts;
            }
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (NumCasts > 0)
            {
                //yield return new(rect, caster.Position, _activation, color: ArenaColor.Danger);
                yield return new(rect, new(95, 115), default, _activation);
                yield return new(rect, new(95, 115), default, _activation);
                yield return new(rect, new(95, 115), default, _activation);
                yield return new(rect, new(105, 115), default, _activation);
                yield return new(rect, new(115, 115), default, _activation);
                yield return new(rect, new(95, 105), default, _activation);
                yield return new(rect, new(105, 105), default, _activation);
                yield return new(rect, new(115, 105), default, _activation);
                yield return new(rect, new(95, 95), default, _activation);
                yield return new(rect, new(105, 95), default, _activation);
                yield return new(rect, new(115, 95), default, _activation);
            }
        }
    }
    */

    /*  
        class VulcanBurst : Components.KnockbackFromCastTarget
        {
            public VulcanBurst() : base(ActionID.MakeSpell(AID.VulcanBurst), 15) { }

            public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
            {
                if (Casters.Count > 0)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.Bounds.Center, module.Bounds.HalfSize - Distance), Casters[0].CastInfo!.NPCFinishAt);
            }
        }
    */
    class E4STitan1States : StateMachineBuilder
    {
        public E4STitan1States(BossModule module) : base(module)
        {
            TrivialPhase()
           .ActivateOnEnter<FaultLine1>()
           .ActivateOnEnter<Landslide2>()
           .ActivateOnEnter<WeightOfTheLand>()
           .ActivateOnEnter<WeightOfTheLand2>()
           .ActivateOnEnter<WeightOfTheLand3>()
           .ActivateOnEnter<FaultLine3>()
           .ActivateOnEnter<EvilEarth2>()
           .ActivateOnEnter<unknownskill_4>()
           .ActivateOnEnter<EarthenFist6>()
           .ActivateOnEnter<EarthenFist4>()
           .ActivateOnEnter<DualEarthenFists3>()
           .ActivateOnEnter<DualEarthenFists>()
           .ActivateOnEnter<Geocrush>()
           .ActivateOnEnter<SeismicWave>()
           .ActivateOnEnter<EarthenFury2>()
           .ActivateOnEnter<VoiceOfTheLand>()
           .ActivateOnEnter<DualEarthenFists2>()
           .ActivateOnEnter<CrumblingDown2>()
           .ActivateOnEnter<IconEnjoyers>()
           .ActivateOnEnter<Explosion>()
           .ActivateOnEnter<LeftwardLandslide>()
           .ActivateOnEnter<RightwardLandslide>()
           .ActivateOnEnter<Megalith>()
           .ActivateOnEnter<Stonecrusher>()
           .ActivateOnEnter<Magnitude5>();
        }
    }

    [ModuleInfo(CFCID = 716, NameID = 8350)]
    public class E4STitan1 : BossModule
    {
        public E4STitan1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }
    }
}
