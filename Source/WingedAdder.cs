using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WingedHelper;

[CustomEntity("WingedHelper/WingedAdder")]
public class WingedAdder : Entity
{
    List<Entity> entities = new();
    EntityFilter filter;
    
    bool mustBeInArea;
    bool actorsOnly;
    bool collidablesOnly;

    bool disableCollisions;
    bool allowInteractions;
    
    bool heavyWings;
    float moveDelay;
    int upSpeed;
    FlyDirection direction;
    Vector2 leftWingOffset = new();
    Vector2 rightWingOffset = new();
    
    string leftColor;
    string rightColor;
    bool rainbow;
    bool flySounds;
    bool flapSounds;
    
    int dashsToActivate;
    string counterColor;
    Vector2 counterOffset = new();
    
    
    public WingedAdder(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        filter = EntityFilter.CreateFromData(data);
        mustBeInArea = data.Bool("inAreaRange");
        heavyWings = data.Bool("isHeavyWings");
        moveDelay = data.Float("moveDelay");
        upSpeed = data.Int("flySpeed");
        actorsOnly = data.Bool("actorsOnly");
        collidablesOnly = data.Bool("collidablesOnly");
        direction = data.Enum("direction", FlyDirection.Up);
        leftWingOffset.X = data.Int("leftWingXOffset");
        leftWingOffset.Y = data.Int("leftWingYOffset");
        rightWingOffset.X = data.Int("rightWingXOffset");
        rightWingOffset.Y = data.Int("rightWingYOffset");
        disableCollisions = data.Bool("disableCollisions");
        allowInteractions = data.Bool("allowInteractions");
        leftColor = data.Attr("leftWingTint", "FFFFFF");
        rightColor = data.Attr("rightWingTint", "FFFFFF");
        rainbow = data.Bool("rainbowWings");
        flySounds = data.Bool("flySounds");
        flapSounds = data.Bool("flapSounds");
        dashsToActivate = data.Int("dashsToActivate");
        counterColor = data.Attr("counterTintColor", "FFFFFF");
        counterOffset.X = data.Int("counterXOffset");
        counterOffset.Y = data.Int("counterYOffset");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        foreach (Entity entity in scene.Entities)
        {
            if (entity == this || entity.Get<WingComponent>() != null) continue;
            if ((!mustBeInArea || Collider.Collide(entity.Position)) && filter.Matches(entity))
            {
                entities.Add(entity);
            }
        }
        
        foreach (Entity entity in entities)
        {
            if (entity.Get<WingComponent>() != null) continue;
            bool isActor = entity is Actor;
            bool hasCollider = entity.Collider != null;
            if ((!actorsOnly || (isActor && actorsOnly)) && (!collidablesOnly || (hasCollider && collidablesOnly)))
            {
                Logger.Log(LogLevel.Info, "WingedHelper", $"Adding WingComponent to {entity}");
                WingComponent wingComp = new WingComponent(moveDelay, upSpeed, heavyWings, direction, isActor, hasCollider, leftWingOffset, rightWingOffset, disableCollisions, 
                    allowInteractions, leftColor, rightColor, rainbow, flySounds, flapSounds, dashsToActivate, counterColor, counterOffset);
                entity.Add(wingComp);
            }
        }
        
        RemoveSelf();
    }
}