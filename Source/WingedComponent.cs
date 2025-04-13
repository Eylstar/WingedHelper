using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.WingedHelper;

public enum FlyDirection
{
    Up,
    Down,
    Left,
    Right
}

public class WingComponent : Component
{
    Player player;
    
    Actor actor;
    bool isOnAnActor;
    bool hasACollider;

    bool routineAdded = false;

    Sprite sprite;
    Sprite leftWing;
    Sprite rightWing;
    Vector2 leftWingOffset;
    Vector2 rightWingOffset;
    
    Vector2 positionLastFrame;
    
    DashListener dashListener;
    Wiggler rotateWiggler;
    
    bool flying;
    bool wingsActivated;
    
    bool collisionDisabled;
    bool interactionsAllowed;
    
    bool heavyWings;
    float flyDelay;
    int flyingUpSpeed;
    FlyDirection flyDirection;
    
    bool canBeGrabbed;
    bool isGrabbed;
    public Holdable holdable;
    Vector2 playerSpeedOnGrab;
    
    Tween flyUpTween;

    Vector2 wingMovementOrigin;
    float wingActivationTime;
    
    Color leftWingColor;
    Color rightWingColor;
    Color counterColor;
    
    bool rainbowWings;
    float hue = 0f;
    
    bool flySounds;
    bool flapSounds;
    
    int dashsToActivate;
    
    PlutoniumText counter;
    Vector2 counterOffset;
    
    
    public WingComponent(float delay = 1.0f, int upSpeed = 10, bool heavy = false, FlyDirection dir = FlyDirection.Up, bool act = false, bool col = false, Vector2 leftOffset = new(), Vector2 rightOffset = new(), 
        bool disableCol = false, bool allowInter = true, string leftColor = "FFFFFF", string rightColor = "FFFFFF", bool rainbow = false, 
        bool flyS = true, bool flapS = false, int dashs = 1, string counterCol = "FFFFFF", Vector2 counterOff = new()) : base(true, false)
    {
        heavyWings = heavy;
        flyDelay = delay;
        flyingUpSpeed = upSpeed;
        flyDirection = dir;
        isOnAnActor = act;
        hasACollider = col;
        leftWingOffset = leftOffset;
        rightWingOffset = rightOffset;
        collisionDisabled = disableCol;
        interactionsAllowed = allowInter;
        leftWingColor = Calc.HexToColor(leftColor);
        rightWingColor = Calc.HexToColor(rightColor);
        counterColor = Calc.HexToColor(counterCol);
        rainbowWings = rainbow;
        flySounds = flyS;
        flapSounds = flapS;
        dashsToActivate = dashs;
        counterOffset = counterOff;
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        
        player = Scene.Tracker.GetEntity<Player>();
        if (isOnAnActor) 
            actor = (Actor) entity;
        
        canBeGrabbed = entity.Get<Holdable>() != null;
        if (canBeGrabbed)
        {
            holdable = entity.Get<Holdable>();
            holdable.OnPickup += OnPickup;
            holdable.OnRelease += OnRelease;
        }
        
        SetupSprites();
        AddComponents();
        SetupTween();
    }
    
    void SetupSprites()
    {
        Visible = true;
        
        sprite = Entity.Get<Sprite>();

        leftWing = GFX.SpriteBank.Create("WingedHelper_wing");
        rightWing = GFX.SpriteBank.Create("WingedHelper_wing");
        rightWing.FlipX = true;

        if (hasACollider)
        {
            Collider collider = Entity.Collider;
            leftWing.Position = new Vector2((collider.Left - leftWing.Width / 2) + 1, canBeGrabbed ? collider.Top : collider.Top + collider.Height / 4);
            rightWing.Position = new Vector2((collider.Right + rightWing.Width / 2) - 1, canBeGrabbed ? collider.Top : collider.Top + collider.Height / 4);
        }
        else
        {
            leftWing.Position = new Vector2(Entity.Left - leftWing.Width / 2 + 1, Entity.Right - leftWing.Width / 2);
            rightWing.Position = new Vector2(Entity.Right + rightWing.Width / 2 - 1, Entity.Right - rightWing.Width / 2);
        }
        
        leftWing.Position += leftWingOffset;
        rightWing.Position += rightWingOffset;
        leftWing.Color = leftWingColor;
        rightWing.Color = rightWingColor;
        
        Entity.Add(leftWing);
        Entity.Add(rightWing);
        leftWing.Play("flap");
        rightWing.Play("flap");
    }
    
    void AddComponents()
    {
        dashListener = new DashListener();
        dashListener.OnDash = OnDash;
        Entity.Add(dashListener);
        
        rotateWiggler = Wiggler.Create(0.7f, 5f, delegate(float v)
        {
            if (sprite != null) sprite.Rotation = v * 35f * (float) Math.PI / 180f;
            leftWing.Rotation = v * 35f * (float) Math.PI / 180f;
            rightWing.Rotation = -v * 35f * (float) Math.PI / 180f;
        });
        Entity.Add(rotateWiggler);
        
        counter = new PlutoniumText("WingedHelper/numbers", "0123456789", new Vector2(4, 6));
        Entity.Add(counter);
    }
    
    void SetupTween()
    {
        flyUpTween = Tween.Create(Tween.TweenMode.Persist, Ease.Linear);
        
        flyUpTween.OnUpdate = tw =>
        {
            if (isGrabbed && !heavyWings)
            {
                flying = false;
                return;
            }
            
            float flewTime = (float) Math.Round(Engine.Scene.TimeActive - wingActivationTime, 2);
            
            Vector2 GetDirection(FlyDirection direction) => direction switch
            {
                FlyDirection.Up => new Vector2(0, -1),
                FlyDirection.Down => new Vector2(0, 1),
                FlyDirection.Left => new Vector2(-1, 0),
                FlyDirection.Right => new Vector2(1, 0),
                _ => Vector2.Zero
            };
            
            Vector2 to = wingMovementOrigin + GetDirection(flyDirection) * flyingUpSpeed * flewTime;

            if (heavyWings && isGrabbed && playerSpeedOnGrab.Length() >= 0.1f && IsFlyVertical())
            {
                playerSpeedOnGrab *= 0.9f;
                wingMovementOrigin += playerSpeedOnGrab;
            }

            if (hasACollider && !collisionDisabled)
            {
                if (IsFlyVertical())
                    CheckColl(Entity.Position, to);
                
                else if (!IsFlyVertical())
                    CheckColl(to, Entity.Position);
   
                void CheckColl(Vector2 a, Vector2 b)
                {
                    if (Entity.CollideCheck<Solid>(new Vector2(a.X, b.Y)))
                        wingActivationTime += Engine.DeltaTime;
                }
            }
            
            ApplyEntityPosition(to);
            
            if (heavyWings && isGrabbed)
            {
                if (!IsFlyVertical())
                {
                    player.Position.X = Entity.Position.X - player.carryOffset.X;
                }
                else
                    player.Position = Entity.Position - player.carryOffset;
            }
        };
        Entity.Add(flyUpTween);
    }
    
    void ApplyEntityPosition(Vector2 to)
    {
        if (Entity is Platform platform)
            platform.MoveTo(to);
        else if(isOnAnActor)
        {
            if (Math.Abs(actor.X - to.X) >= 0.5f)
                actor.MoveH(to.X - actor.X);
            actor.MoveV(to.Y - (int)actor.Y);
        }
        else 
            Entity.Position = new Vector2((int)to.X, (int)to.Y);
        positionLastFrame = Entity.Position;
        
        //if the entity moved during the rest of the frame, add his movement to the wing movement
    }
    
    void OnPickup()
    {
        isGrabbed = true;
        if (heavyWings && flying)
        {
            playerSpeedOnGrab = player.Speed / 40;
            playerSpeedOnGrab.Y /= 2;
        }

        if (heavyWings) return;

        flying = true;
    }

    void OnRelease(Vector2 force)
    {
        if (wingsActivated)
        {
            flying = true;
            wingMovementOrigin = new Vector2((int)Entity.X, (int)Entity.Y);
            wingActivationTime = Engine.Scene.TimeActive;
            positionLastFrame = Entity.Position;
        }
        isGrabbed = false;
    }

    void OnDash(Vector2 dir)
    {
        if (flying || wingsActivated) return;
        
        dashsToActivate = Math.Max(0, dashsToActivate - 1);
        if (dashsToActivate == 0 && !routineAdded)
        {
            Entity.Add(new Coroutine(FlyAwayRoutine()));
            routineAdded = true;
        }
    }

    public override void Update()
    {
        base.Update();
        UpdateWings();

        if (!flying)
            positionLastFrame = Entity.Position;

        if (isOnAnActor && flying && positionLastFrame.X != Entity.Position.X && !isGrabbed)
        {
            if (IsFlyVertical())
                wingMovementOrigin.X = Entity.Position.X;
            else
                wingMovementOrigin.X += Entity.Position.X - positionLastFrame.X;
        }

        if (IsFlyVertical() && Math.Abs(Input.MoveX) > 0.1f && isGrabbed && heavyWings)
        {
            wingMovementOrigin.X += Input.MoveX * 0.5f;
        }
        
        if (!flying) return;
        flyUpTween.Update();
    }

    void UpdateWings()
    {
        if (rainbowWings)
        {
            hue += Engine.DeltaTime * 0.4f;
            if (hue >= 1f) hue -= 1f;
            leftWing.Color = Calc.HsvToColor(hue, 0.75f,0.75f);
            rightWing.Color = Calc.HsvToColor(hue, 0.75f, 0.75f);
        }
        
        if(leftWing.CurrentAnimationFrame % 9 == 4 && flapSounds) 
            Audio.Play("event:/game/general/strawberry_wingflap", Entity.Position);
    }

    public override void Render()
    {
        base.Render();
        counter.PrintCentered(Entity.Position + counterOffset, dashsToActivate.ToString(),  true, 5, counterColor, Color.Black, 1f);
    }

    IEnumerator FlyAwayRoutine()
    {
        rotateWiggler.Start();
        yield return 0.1f;
        if (flySounds) Audio.Play("event:/game/general/strawberry_laugh", Entity.Position);
        yield return 0.2f;
        if (flySounds) Audio.Play("event:/game/general/strawberry_flyaway", Entity.Position);
        yield return flyDelay - 0.3f;
        StartFlying();
    }
    
    void StartFlying()
    {
        flying = true;
        wingsActivated = true;
        wingMovementOrigin = Entity.Position;
        wingActivationTime = Engine.Scene.TimeActive;
        
        if (collisionDisabled)
            Entity.Collider = null;

        if (!interactionsAllowed)
        {
            Entity.Collidable = false;
            if (canBeGrabbed)
                holdable.Entity.Collidable = false;
        }
    }
    
    public override void Removed(Entity entity)
    {
        base.Removed(entity);
        if (canBeGrabbed)
        {
            holdable.OnPickup -= OnPickup;
            holdable.OnRelease -= OnRelease;
        }
    }
    
    bool IsFlyVertical()
    {
        return flyDirection == FlyDirection.Up || flyDirection == FlyDirection.Down;
    }
    
    static bool IsEntityFlying(Entity entity)
    {
        return entity.Get<WingComponent>()?.flying ?? false;
    }
    
    public static void onBumperWiggle(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self) 
    {
        if (IsEntityFlying(self))
            return;
        else
            orig(self);
    }
}