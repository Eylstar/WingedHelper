using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

//Credits to JaThePlayer and FrostHelper for (big) parts of the code

namespace Celeste.Mod.WingedHelper;

public class EntityFilter
{
    static readonly Type[] defaultBlackListTypes =
    [
        typeof(Player),
        typeof(SolidTiles),
        typeof(BackgroundTiles),
        typeof(SpeedrunTimerDisplay),
        typeof(StrawberriesCounter),
        typeof(Decal),
        typeof(ParticleSystem),
        typeof(SeekerEffectsController),
        typeof(WindController),
        typeof(TotalStrawberriesDisplay),
        typeof(GameplayStats),
        typeof(LightningRenderer),
        typeof(GrabbyIcon),
        typeof(FormationBackdrop),
        typeof(TrailManager),
        typeof(GlassBlockBg),
        typeof(MirrorSurface),
        typeof(MirrorSurfaces),
        typeof(WaterSurface),
        typeof(SeekerBarrierRenderer),
        typeof(DustEdges),
        typeof(DustEdge),
    ];

    HashSet<Type> types;
    bool isBlackList;

    public EntityFilter(HashSet<Type> types, bool bl)
    {
        this.types = types;
        isBlackList = bl;
    }

    public static EntityFilter CreateFromData(EntityData data, string typesKey = "selectedTypes", string blackListKey = "blacklistMode")
    {
        string str = data.Attr(typesKey, "");
        bool bl = data.Bool(blackListKey);
        HashSet<Type> hs = TypeHelper.GetTypesAsHashSet(str);
        if (bl)
        {
            foreach (Type t in defaultBlackListTypes)
                hs.Add(t);
        }

        return new EntityFilter(hs, bl);
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Matches(Entity entity)
    {
        return types.Contains(entity.GetType()) != isBlackList;
    }
}


public static class TypeHelper
{
    public static Dictionary<string, Type> typesDictionnary;
    
    public static HashSet<Type> GetTypesAsHashSet(string typeString) 
    {
        if (typeString == string.Empty) 
            return [];
        
        string[] split = typeString.Trim().Split(',');
        
        HashSet<Type> parsed = new HashSet<Type>();
        for (int i = 0; i < split.Length; i++) 
            parsed.Add(EntityNameToType(split[i].Trim()));
        
        return parsed;
    }
    
    public static Type EntityNameToType(string entityName) 
    {
        Type type = EntityNameToTypeSafe(entityName);
        if (type is not null) 
            return type;
        throw new Exception("Type not found: " + entityName);
    }

    public static Type EntityNameToTypeSafe(string entityName) 
    {
        if (typesDictionnary is null)
            CreateCache();

        if (typesDictionnary!.TryGetValue(entityName, out Type t))
            return t;

        if (FakeAssembly.GetFakeEntryAssembly().GetType(entityName, false, true) is { } csType) 
        {
            typesDictionnary[entityName] = csType;
            return csType;
        }
        
        return null;
    }

    private static void CreateCache()
    {
        typesDictionnary = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["checkpoint"] = typeof(Checkpoint),
            ["jumpThru"] = typeof(JumpthruPlatform),
            ["refill"] = typeof(Refill),
            ["infiniteStar"] = typeof(FlyFeather),
            ["strawberry"] = typeof(Strawberry),
            ["memorialTextController"] = typeof(Strawberry),
            ["goldenBerry"] = typeof(Strawberry),
            ["summitgem"] = typeof(SummitGem),
            ["blackGem"] = typeof(HeartGem),
            ["dreamHeartGem"] = typeof(DreamHeartGem),
            ["spring"] = typeof(Spring),
            ["wallSpringLeft"] = typeof(Spring),
            ["wallSpringRight"] = typeof(Spring),
            ["fallingBlock"] = typeof(FallingBlock),
            ["zipMover"] = typeof(ZipMover),
            ["crumbleBlock"] = typeof(CrumblePlatform),
            ["dreamBlock"] = typeof(DreamBlock),
            ["touchSwitch"] = typeof(TouchSwitch),
            ["switchGate"] = typeof(SwitchGate),
            ["negaBlock"] = typeof(NegaBlock),
            ["key"] = typeof(Key),
            ["lockBlock"] = typeof(LockBlock),
            ["movingPlatform"] = typeof(MovingPlatform),
            ["rotatingPlatforms"] = typeof(RotatingPlatform),
            ["blockField"] = typeof(BlockField),
            ["cloud"] = typeof(Cloud),
            ["booster"] = typeof(Booster),
            ["moveBlock"] = typeof(MoveBlock),
            ["light"] = typeof(PropLight),
            ["switchBlock"] = typeof(SwapBlock),
            ["swapBlock"] = typeof(SwapBlock),
            ["dashSwitchH"] = typeof(DashSwitch),
            ["dashSwitchV"] = typeof(DashSwitch),
            ["templeGate"] = typeof(TempleGate),
            ["torch"] = typeof(Torch),
            ["templeCrackedBlock"] = typeof(TempleCrackedBlock),
            ["seekerBarrier"] = typeof(SeekerBarrier),
            ["theoCrystal"] = typeof(TheoCrystal),
            ["glider"] = typeof(Glider),
            ["theoCrystalPedestal"] = typeof(TheoCrystalPedestal),
            ["badelineBoost"] = typeof(BadelineBoost),
            ["cassette"] = typeof(Cassette),
            ["cassetteBlock"] = typeof(CassetteBlock),
            ["wallBooster"] = typeof(WallBooster),
            ["bounceBlock"] = typeof(BounceBlock),
            ["coreModeToggle"] = typeof(CoreModeToggle),
            ["iceBlock"] = typeof(IceBlock),
            ["fireBarrier"] = typeof(FireBarrier),
            ["eyebomb"] = typeof(Puffer),
            ["flingBird"] = typeof(FlingBird),
            ["flingBirdIntro"] = typeof(FlingBirdIntro),
            ["birdPath"] = typeof(BirdPath),
            ["lightningBlock"] = typeof(LightningBreakerBox),
            ["spikesUp"] = typeof(Spikes),
            ["spikesDown"] = typeof(Spikes),
            ["spikesLeft"] = typeof(Spikes),
            ["spikesRight"] = typeof(Spikes),
            ["triggerSpikesUp"] = typeof(TriggerSpikes),
            ["triggerSpikesDown"] = typeof(TriggerSpikes),
            ["triggerSpikesRight"] = typeof(TriggerSpikes),
            ["triggerSpikesLeft"] = typeof(TriggerSpikes),
            ["darkChaser"] = typeof(BadelineOldsite),
            ["rotateSpinner"] = typeof(BladeRotateSpinner),
            ["trackSpinner"] = typeof(TrackSpinner),
            ["spinner"] = typeof(CrystalStaticSpinner),
            ["sinkingPlatform"] = typeof(SinkingPlatform),
            ["friendlyGhost"] = typeof(AngryOshiro),
            ["seeker"] = typeof(Seeker),
            ["seekerStatue"] = typeof(SeekerStatue),
            ["slider"] = typeof(Slider),
            ["templeBigEyeball"] = typeof(TempleBigEyeball),
            ["crushBlock"] = typeof(CrushBlock),
            ["bigSpinner"] = typeof(Bumper),
            ["starJumpBlock"] = typeof(StarJumpBlock),
            ["floatySpaceBlock"] = typeof(FloatySpaceBlock),
            ["glassBlock"] = typeof(GlassBlock),
            ["goldenBlock"] = typeof(GoldenBlock),
            ["fireBall"] = typeof(FireBall),
            ["risingLava"] = typeof(RisingLava),
            ["sandwichLava"] = typeof(SandwichLava),
            ["killbox"] = typeof(Killbox),
            ["fakeHeart"] = typeof(FakeHeart),
            ["lightning"] = typeof(Lightning),
            ["finalBoss"] = typeof(FinalBoss),
            ["finalBossFallingBlock"] = typeof(FallingBlock),
            ["finalBossMovingBlock"] = typeof(FinalBossMovingBlock),
            ["fakeWall"] = typeof(FakeWall),
            ["fakeBlock"] = typeof(FakeWall),
            ["dashBlock"] = typeof(DashBlock),
            ["invisibleBarrier"] = typeof(InvisibleBarrier),
            ["exitBlock"] = typeof(ExitBlock),
            ["conditionBlock"] = typeof(ExitBlock),
            ["coverupWall"] = typeof(CoverupWall),
            ["crumbleWallOnRumble"] = typeof(CrumbleWallOnRumble),
            ["ridgeGate"] = typeof(RidgeGate),
            ["tentacles"] = typeof(Tentacles),
            ["starClimbController"] = typeof(StarClimbGraphicsController),
            ["playerSeeker"] = typeof(PlayerSeeker),
            ["chaserBarrier"] = typeof(ChaserBarrier),
            ["introCrusher"] = typeof(IntroCrusher),
            ["bridge"] = typeof(Bridge),
            ["bridgeFixed"] = typeof(BridgeFixed),
            ["bird"] = typeof(BirdNPC),
            ["introCar"] = typeof(IntroCar),
            ["memorial"] = typeof(Memorial),
            ["wire"] = typeof(Wire),
            ["cobweb"] = typeof(Cobweb),
            ["lamp"] = typeof(Lamp),
            ["hanginglamp"] = typeof(HangingLamp),
            ["hahaha"] = typeof(Hahaha),
            ["bonfire"] = typeof(Bonfire),
            ["payphone"] = typeof(Payphone),
            ["colorSwitch"] = typeof(ClutterSwitch),
            ["clutterDoor"] = typeof(ClutterDoor),
            ["dreammirror"] = typeof(DreamMirror),
            ["resortmirror"] = typeof(ResortMirror),
            ["towerviewer"] = typeof(Lookout),
            ["picoconsole"] = typeof(PicoConsole),
            ["wavedashmachine"] = typeof(WaveDashTutorialMachine),
            ["yellowBlocks"] = typeof(ClutterBlockBase),
            ["redBlocks"] = typeof(ClutterBlockBase),
            ["greenBlocks"] = typeof(ClutterBlockBase),
            ["oshirodoor"] = typeof(MrOshiroDoor),
            ["templeMirrorPortal"] = typeof(TempleMirrorPortal),
            ["reflectionHeartStatue"] = typeof(ReflectionHeartStatue),
            ["resortRoofEnding"] = typeof(ResortRoofEnding),
            ["gondola"] = typeof(Gondola),
            ["birdForsakenCityGem"] = typeof(ForsakenCitySatellite),
            ["whiteblock"] = typeof(WhiteBlock),
            ["plateau"] = typeof(Plateau),
            ["soundSource"] = typeof(SoundSourceEntity),
            ["templeMirror"] = typeof(TempleMirror),
            ["templeEye"] = typeof(TempleEye),
            ["clutterCabinet"] = typeof(ClutterCabinet),
            ["floatingDebris"] = typeof(FloatingDebris),
            ["foregroundDebris"] = typeof(ForegroundDebris),
            ["moonCreature"] = typeof(MoonCreature),
            ["lightbeam"] = typeof(LightBeam),
            ["door"] = typeof(Door),
            ["trapdoor"] = typeof(Trapdoor),
            ["resortLantern"] = typeof(ResortLantern),
            ["water"] = typeof(Water),
            ["waterfall"] = typeof(WaterFall),
            ["bigWaterfall"] = typeof(BigWaterfall),
            ["clothesline"] = typeof(Clothesline),
            ["cliffflag"] = typeof(CliffFlags),
            ["cliffside_flag"] = typeof(CliffsideWindFlag),
            ["flutterbird"] = typeof(FlutterBird),
            ["SoundTest3d"] = typeof(_3dSoundTest),
            ["SummitBackgroundManager"] = typeof(AscendManager),
            ["summitGemManager"] = typeof(SummitGem),
            ["heartGemDoor"] = typeof(HeartGemDoor),
            ["summitcheckpoint"] = typeof(SummitCheckpoint),
            ["summitcloud"] = typeof(SummitCloud),
            ["coreMessage"] = typeof(CoreMessage),
            ["playbackTutorial"] = typeof(PlayerPlayback),
            ["playbackBillboard"] = typeof(PlaybackBillboard),
            ["cutsceneNode"] = typeof(CutsceneNode),
            ["kevins_pc"] = typeof(KevinsPC),
            ["powerSourceNumber"] = typeof(PowerSourceNumber),
            ["npc"] = typeof(NPC),
            ["eventTrigger"] = typeof(EventTrigger),
            ["musicFadeTrigger"] = typeof(MusicFadeTrigger),
            ["musicTrigger"] = typeof(MusicTrigger),
            ["altMusicTrigger"] = typeof(AltMusicTrigger),
            ["cameraOffsetTrigger"] = typeof(CameraOffsetTrigger),
            ["lightFadeTrigger"] = typeof(LightFadeTrigger),
            ["bloomFadeTrigger"] = typeof(BloomFadeTrigger),
            ["cameraTargetTrigger"] = typeof(CameraTargetTrigger),
            ["cameraAdvanceTargetTrigger"] = typeof(CameraAdvanceTargetTrigger),
            ["respawnTargetTrigger"] = typeof(RespawnTargetTrigger),
            ["changeRespawnTrigger"] = typeof(ChangeRespawnTrigger),
            ["windTrigger"] = typeof(WindTrigger),
            ["windAttackTrigger"] = typeof(WindAttackTrigger),
            ["minitextboxTrigger"] = typeof(MiniTextboxTrigger),
            ["oshiroTrigger"] = typeof(OshiroTrigger),
            ["interactTrigger"] = typeof(InteractTrigger),
            ["checkpointBlockerTrigger"] = typeof(CheckpointBlockerTrigger),
            ["lookoutBlocker"] = typeof(LookoutBlocker),
            ["stopBoostTrigger"] = typeof(StopBoostTrigger),
            ["noRefillTrigger"] = typeof(NoRefillTrigger),
            ["ambienceParamTrigger"] = typeof(AmbienceParamTrigger),
            ["creditsTrigger"] = typeof(CreditsTrigger),
            ["goldenBerryCollectTrigger"] = typeof(GoldBerryCollectTrigger),
            ["moonGlitchBackgroundTrigger"] = typeof(MoonGlitchBackgroundTrigger),
            ["blackholeStrength"] = typeof(BlackholeStrengthTrigger),
            ["rumbleTrigger"] = typeof(RumbleTrigger),
            ["birdPathTrigger"] = typeof(BirdPathTrigger),
            ["spawnFacingTrigger"] = typeof(SpawnFacingTrigger),
            ["detachFollowersTrigger"] = typeof(DetachStrawberryTrigger),
        };

        foreach (var type in FakeAssembly.GetFakeEntryAssembly().GetTypes())
        {
            foreach (CustomEntityAttribute customEntityAttribute in type.GetCustomAttributes<CustomEntityAttribute>()) 
            {
                foreach (string idFull in customEntityAttribute.IDs) 
                {
                    string id;
                    string[] split = idFull.Split('=');

                    if (split.Length == 1 || split.Length == 2) 
                        id = split[0];
                    else 
                        continue;

                    typesDictionnary[id.Trim()] = type;
                }
            }
        }
    }
}


public class PlutoniumText : Component
{
    public readonly Dictionary<char, int> Charset;
    public readonly List<MTexture> CharTextures;
    public Vector2 FontSize;
    public readonly string CharList;
    
    public PlutoniumText(string fontPath, string charList, Vector2 fontSize) : base(true, true)
    {
        FontSize = fontSize;
        Charset = new Dictionary<char, int>();
        CharTextures = new List<MTexture>();
        string characters = CharList = charList;

        for (int i = 0; i < characters.Length; i++)
        {
            Charset.Add(characters[i], i);
            CharTextures.Add(GFX.Game[fontPath].GetSubtexture(i * (int)fontSize.X, 0, (int)fontSize.X, (int)fontSize.Y)); // "PlutoniumHelper/PlutoniumText/font"
        }
    }
    
    public void PrintCentered(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0)
    {
        float stringlen = spacing * str.Length * scale;
        Print(pos - new Vector2((float)Math.Floor(stringlen / 2f), (float)Math.Floor(FontSize.Y / 2f)), str, shadow, spacing, mainColor, outlineColor, scale, id);
    }

    public void Print(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0)
    {
        SpriteEffects flip = SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (SaveData.Instance.Assists.MirrorMode) str = new string(str.Reverse().ToArray());
        
        pos = pos.Floor();

        int index = 0;

        if (outlineColor != Color.Transparent)
        {
            foreach (char c in str) //draw shadows
            {
                float offset = index * spacing * scale;
                Vector2 charpos = pos + Vector2.UnitX * offset;
                charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));

                int chr;
                if (!Charset.ContainsKey(c))
                    chr = 0;
                else
                    chr = Charset[c];
                
                if (shadow)
                    CharTextures[chr].Draw(charpos + Vector2.One * scale, Vector2.Zero, outlineColor, scale, 0, flip);

                index++;
                
            }
        }

        if (mainColor == Color.Transparent) return;

        index = 0;

        foreach (char c in str) //draw all characters
        {
            float offset = index * spacing * scale;
            Vector2 charpos = pos + Vector2.UnitX * offset;
            charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));
            int chr;
            if (!Charset.TryGetValue(c, out int value))
                chr = 0;
            else
                chr = value;

            CharTextures[chr].Draw(charpos, Vector2.Zero, mainColor, scale, 0, flip);
            index++;
        }

    }
}