using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
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

    Sprite sprite;
    Sprite leftWing;
    Sprite rightWing;
    Vector2 leftWingOffset;
    Vector2 rightWingOffset;
    
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
    float horizontalForce;
    float initialHorizontalForce = 400;
    float maxXtobeAdded = 50f;
    float XtobeAdded = 0;
    
    bool canBeGrabbed;
    bool isGrabbed;
    bool justThrowed;
    public Holdable holdable;
    Vector2 playerSpeedOnGrab;
    
    Tween flyUpTween;

    Vector2 wingActivationPositionStart;
    float wingActivationTime;
    
    Color leftWingColor;
    Color rightWingColor;
    
    bool rainbowWings;
    float hue = 0f;
    
    
    public WingComponent(float delay = 1.0f, int upSpeed = 10, bool heavy = false, FlyDirection dir = FlyDirection.Up, bool act = false, bool col = false, Vector2 leftOffset = new(), Vector2 rightOffset = new(), 
        bool disableCol = false, bool allowInter = true, string leftColor = "FFFFFF", string rightColor = "FFFFFF", bool rainbow = false) : base(true, false)
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
        rainbowWings = rainbow;
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        
        player = Scene.Tracker.GetEntity<Player>();
        
        if (isOnAnActor) actor = (Actor) entity;
        
        canBeGrabbed = entity.Get<Holdable>() != null;
        if (canBeGrabbed)
        {
            holdable = entity.Get<Holdable>();
            holdable.OnPickup += OnPickup;
            holdable.OnRelease += OnRelease;
        }
        
        SetupSprites();
        SetupTween();
        
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

            if (canBeGrabbed && justThrowed && wingsActivated && !isGrabbed)
            {
                float horizontalForceAbs = Math.Abs(horizontalForce);
                float factor = 1 - (horizontalForceAbs / initialHorizontalForce);
                XtobeAdded = (float)(maxXtobeAdded / (1 + Math.Exp(-6 * (factor - 0.5f)))) * Math.Sign(horizontalForce);
                horizontalForce *= 0.95f;
                if (Math.Abs(horizontalForce) <= 1f)
                {
                    horizontalForce = 0;
                    XtobeAdded = (int)Math.Round(XtobeAdded);
                    justThrowed = false;
                }
            }

            Vector2 to = flyDirection switch
            {
                FlyDirection.Up => wingActivationPositionStart + new Vector2(XtobeAdded, -flyingUpSpeed * flewTime),
                FlyDirection.Down => wingActivationPositionStart + new Vector2(XtobeAdded, flyingUpSpeed * flewTime),
                FlyDirection.Left => new Vector2(wingActivationPositionStart.X + (-flyingUpSpeed * flewTime) + XtobeAdded, wingActivationPositionStart.Y),
                FlyDirection.Right => new Vector2(wingActivationPositionStart.X + (flyingUpSpeed * flewTime) + XtobeAdded, wingActivationPositionStart.Y),
                _  => new Vector2()
            };

            if (heavyWings && isGrabbed && playerSpeedOnGrab.Length() >= 0.1f)
            {
                playerSpeedOnGrab *= 0.9f;
                to.X += playerSpeedOnGrab.X;
                XtobeAdded += playerSpeedOnGrab.X;
            }

            if (hasACollider && !collisionDisabled)
            {
                if (flyDirection == FlyDirection.Up || flyDirection == FlyDirection.Down)
                    CheckColl(Entity.Position, to);
                
                else if (flyDirection == FlyDirection.Left || flyDirection == FlyDirection.Right)
                    CheckColl(to, Entity.Position);
   
                void CheckColl(Vector2 a, Vector2 b)
                {
                    if (Entity.CollideCheck<Solid>(new Vector2(a.X, b.Y)))
                        wingActivationTime += Engine.DeltaTime;
                    
                    if (Entity.CollideCheck<Solid>(new Vector2(b.X, a.Y)))
                    {
                        horizontalForce = 0;
                        XtobeAdded = (int)Math.Round(XtobeAdded);
                        justThrowed = false;
                    }
                }
            }
            
            ApplyEntityPosition(to);
            
            if (heavyWings && isGrabbed)
            {
                if (flyDirection == FlyDirection.Left || flyDirection == FlyDirection.Right)
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
            {
                actor.MoveH(to.X - actor.X);
            }
            actor.MoveV(to.Y - (int)actor.Y);
        }
        else 
            Entity.Position = new Vector2((int)to.X, (int)to.Y);
    }
    
    void SetupSprites()
    {
        sprite = Entity.Get<Sprite>();

        leftWing = GFX.SpriteBank.Create("WingedHelper_wing");
        rightWing = GFX.SpriteBank.Create("WingedHelper_wing");
        rightWing.FlipX = true;

        if (hasACollider)
        {
            Collider collider = Entity.Collider;
            leftWing.Position = new Vector2((collider.Left - leftWing.Width / 2) + 1, canBeGrabbed ? collider.Top : collider.Top + collider.Height / 4);
            rightWing.Position = new Vector2((collider.Right + rightWing.Width / 2) - 1, canBeGrabbed ? collider.Top : collider.Top + collider.Height / 4);
            leftWing.Position += leftWingOffset;
            rightWing.Position += rightWingOffset;
        }
        else
        {
            leftWing.Position = new Vector2(Entity.Left - leftWing.Width / 2 + 1, Entity.Right - leftWing.Width / 2);
            rightWing.Position = new Vector2(Entity.Right + rightWing.Width / 2 - 1, Entity.Right - rightWing.Width / 2);
        }
        
        leftWing.Color = leftWingColor;
        rightWing.Color = rightWingColor;
        
        Entity.Add(leftWing);
        Entity.Add(rightWing);
        leftWing.Play("flap");
        rightWing.Play("flap");
    }
    
    void OnPickup()
    {
        isGrabbed = true;
        if (heavyWings && flying)
        {
            playerSpeedOnGrab = player.Speed / 40;
            playerSpeedOnGrab.Y /= 2;
        }
        
        if (heavyWings)
            return;
        justThrowed = false;
        flying = false;
    }
    
    void OnRelease(Vector2 force)
    {
        isGrabbed = false;
        if (wingsActivated)
        {
            flying = true;
            XtobeAdded = 0;
            wingActivationPositionStart = new Vector2((int)Entity.X, (int)Entity.Y);
            wingActivationTime = Engine.Scene.TimeActive;
            if (force.X != 0)
            {
                justThrowed = true;
                horizontalForce = force.X * 300;
                initialHorizontalForce = Math.Abs(horizontalForce);
            }
        }
    }

    void OnDash(Vector2 dir)
    {
        if (flying || wingsActivated) return;
        Entity.Add(new Coroutine(FlyAwayRoutine()));
    }

    public override void Update()
    {
        base.Update();
        if (rainbowWings)
        {
            hue += Engine.DeltaTime * 0.4f;
            if (hue >= 1f) hue -= 1f;
            leftWing.Color = Calc.HsvToColor(hue, 0.75f,0.75f);
            rightWing.Color = Calc.HsvToColor(hue, 0.75f, 0.75f);
        }
        if (!flying) return;
        flyUpTween.Update();
    }

    IEnumerator FlyAwayRoutine()
    {
        rotateWiggler.Start();
        yield return 0.1f;
        Audio.Play("event:/game/general/strawberry_laugh", Entity.Position);
        yield return 0.2f;
        Audio.Play("event:/game/general/strawberry_flyaway", Entity.Position);
        yield return flyDelay - 0.3f;
        StartFlying();
    }
    
    void StartFlying()
    {
        flying = true;
        wingsActivated = true;
        wingActivationPositionStart = Entity.Position;
        wingActivationTime = Engine.Scene.TimeActive;
        if (collisionDisabled)
        {
            Entity.Collider = null;
        }

        if (!interactionsAllowed)
        {
            Entity.Collidable = false;
            if (canBeGrabbed)
            {
                holdable.Entity.Collidable = false;
            }
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