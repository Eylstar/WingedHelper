# WINGED HELPER
This mod allows you to add wings to almost any entity.

There is the CS code of it, if you search the mod download it's here : [GameBanana Page](https://gamebanana.com/mods/573885)


------




## How to use


In Lönn, just add a *WingedAdder* Entity in your room.

Then you can tweak it's filter options to target the entities you want to add wings to, the behaviour of wings, positions and many more things :

![image](https://github.com/user-attachments/assets/aa1257ad-4cd5-4d6c-bb56-88109f7957e8)


### Filter Options :
- **SelectedTypes** : this is your list of entities that the adder will target. Includes an auto-complete search for modded and wierd entities class names
- **InAreaRange** : check this if you want the adder to only compute entities that overlaps his box in Lönn
- **BlackList** : if toggled, the SelectedTypes filter list will turn into a blacklist filter, selecting every entity except the specified
- **Actors Only** & **Collidables Only** : in case of wild entity selection, lets you select only entites that have a Collider component, or that inherits from the Actor class

### Wings and Entity behaviour :
- **Direction** : specify the direction in wich the entity will fly (Up, Down, Left or Right)
- **FlySpeed** & **FlyDelay** : specify the speed at wich you entity will fly, after how many seconds since the player dashed
- **Disable Collisions** : check this if you want your entity to not detect any collisions after the wings are activated (passing through walls e.g)
- **Allow Interactions** : for holdables objects only, specify if the player can grab the object once it begins to fly
- **IsHeavyWings** : for holdables only, change the behaviour of the holdable once his wings are activated. It then lifts the player with him instead of being lifted by the player

### Customisation :
- **Wings Offset** : allows you to place each wing independently in the Y and X axis
- **Wing Tint** : allows you to customize the color of each wing
- **Rainbow Wings** : because why not
