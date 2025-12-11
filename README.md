# Warsails Fire Damage Rebalance

This is primarily made because of what I percieve to be very silly balance decisions.

## Vanilla Fire Mechanics
In Vanilla, ships have two seperate HP bars, one for hull, and one specifically for fire, and by default they equal one another(60k hull HP means 60k Fire HP). Hull is visible to you at any time with ALT or the scoreboard, but Fire HP is hidden. Fire HP is damaged by firepots from the ballista, fire grapeshot, and fire arrows. The Fire pots do 2500HP, the grapeshots do 125 per shot, fire arrows are 16, fire bolts are 17.

Ships have a constant regeneration applied to the ship, equal to 0.5% maximum fire HP per second. In the extremes, this results in the droman(60k) regenerating 300 FHP/s, and the fishing ship (4k) regenerating 20FHP/s. This regeneration applies even if the ship is actively burning, and goes up to maximum HP, even if hull HP is lower. THe end result of this means fire is super obnoxious as an actual sink path, considering its damage values are similar to iron shot, and does not have a ram option. Fire arrows serve little purpose, and a droman with 80 archers can only realistically ignite a fishing ship.

## What This changes

This mod adjusts a few things at once, mainly targetting the defensive regeneration of ships, rather than buffing offense.

### FireHP can no longer exceed Hull HP. 
This means that the two options are a lot more comparable to each other, and hull damage will make ships easier to burn, just how hull damage makes ships easier to sink.

### Regeneration is paused for 8 seconds after sustaining any fire damage
This includes fire arrows, which are now effective at suppressing regen during active combat. 

### Regeneration power now scales with crew size, and current HP
The formula is pretty aggressive for crew size, but the less crew there is on the deck, the slower regen will be. At 100% crew, this is a 1.0x multiplier. At 50%, this is a 0.25x multiplier. At 25%, this is a 0.0625 multiplier. 
Regen also scales with current HP, and regens 0.5% of current HP per seconds. For instance, a Droman at 30k fire HP is 150 FHP/s, while after one full second, at 30,150, it will have increased to 150.75 FHP/s. This is to encourage bursts and sustained fire. Occasional chips will get shrugged off.

## Ultimate effect
I have played with these settings only a limited amount, but already fire is a viable sink option if you really focus on burning down a single ship. It is balanced to some degree, most of the time when a ship is burning, it is low crew anyway, but this seems to put it on par with hull damaging ballista, and fleshes out the use of fire arrows.
