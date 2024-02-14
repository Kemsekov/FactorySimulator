In this example we need to create a `bronze drill`, that consists of several other details that are also created from other details and so on.

Full recipes required can be seen into file `bronze_drill_recipes.json`.
Logic can be seen into file `Main.cs`

Results are following:


List of transformations that need to be done will be printed into console:

Top message means we need 100 crafting tables to produce 400 drills each 10 time units.

And lower messages are list of machines and amount of their copied that needed to be constructed and connected to machines above in order to produce required amount of drills.

Order shows production from the most complex detail to the most basic (up to raw resources)
```
-----------------

Amount 100 
Crafting table
[bronze drill head,1],[iron gear,2],[item pipe,1],[fluid pipe,1]
[bronze drill,4]
Time: 10
Cost: 5
-----------------

Amount 2
Crafting table
[copper rotor,2],[glass pane,2],[bronze curved plate,6]
[fluid pipe,16]
Time: 3
Cost: 7
-----------------

Amount 8
Crafting table
[copper blade,4],[copper bolt,4],[copper ring,1]
[copper rotor,1]
Time: 4
Cost: 6

//1 drill require 1 head so again 100
Amount 100
Crafting table
[bronze plate,1],[bronze curved plate,2],[bronze rod,1],[bronze gear,2],[soldering alloy,75]
[bronze drill head,1]
Time: 10
Cost: 8
-----------------

Amount 2
Crafting table
[copper curved plate,2],[copper rod,1]
[copper blade,4]
Cost: 2

Amount 20
Crafting table
[iron plate,4],[iron ring,1],[soldering alloy,100]
[iron gear,2]
Time: 2
Cost: 4

Amount 30
Crafting table
[bronze plate,4],[bronze ring,1],[soldering alloy,100]
[bronze gear,2]
Time: 3
Cost: 2
-----------------

Amount 2
Compressor
[copper rod,1]
[copper ring,1]

Amount 4
Cutting machine
[copper rod,1]
[copper bolt,2]

Amount 4
Compressor
[copper plate,1]
[copper curved plate,1]

Amount 10
Compressor
[bronze rod,1]
[bronze ring,1]

Amount 10
Compressor
[iron rod,1]
[iron ring,1]

Amount 24
Compressor
[bronze plate,1]
[bronze curved plate,1]
-----------------

Amount 1
Crafting table
[glass,6]
[glass pane,16]

Amount 4
Cutting machine
[copper ingot,1]
[copper rod,2]

Amount 4
Compressor
[copper ingot,1]
[copper plate,1]

Amount 5
Cutting machine
[iron ingot,1]
[iron rod,2]

Amount 10
Cutting machine
[bronze ingot,1]
[bronze rod,2]

Amount 40
Compressor
[iron ingot,1]
[iron plate,1]

Amount 74
Compressor
[bronze ingot,1]
[bronze plate,1]
-----------------

Amount 6
Raw resource
[glass,1]

Amount 8
Raw resource
[copper ingot,1]

Amount 45
Raw resource
[iron ingot,1]

Amount 84
Raw resource
[bronze ingot,1]

Amount 2750
Raw resource
[soldering alloy,1]
-----------------
Summary of machines needed:

Cutting machine 4
Raw resource 5
Crafting table 8
Compressor 8
-----------------
Recipies used: 25
Resource movements: 35
Total cost: 7770
```