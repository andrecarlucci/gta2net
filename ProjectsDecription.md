# Introduction #
This page serves to describe the project that make the solution.
This is also supposed to be a introduction to the whole solution.


# Details #
The actual architecture of the solution is composed by a server and a client, at the moment they only work in the same machine and in the same memory space(i.e the same executable) in the future it is supposed to be different executables and communicate by network packages. When this is archived we will probably have the multi-player part of the game working or it will be to slow and does not work... (time will tell).

## GTA2NET ##
As it was told before this project is the client, it's responsibilities are:
  * display the world, the graphics, sound and text.
  * receive player input.
  * communicate with the server, send the player input and display all the information receive for the server.

### More info ###
This project had been losing a lot of code, that was moved to GTA2NET Core, this code changes had been done to try to archive the client-server architecture, also because this changes this project becomes a lot more simple and a few more cleanups could be done in that mainly join BaseGame class with MainGame class.

The class MainGame contains the game loop.

## GTA2NET Content ##
This entry refers to 2 projects the Content and the Dummy.
The reason is this two project are linked one of them is a MonoGame content build placeholder and the other is the XNA content build.
At this time the purpose of this projects is to compile the effect files, files that have the HLSL code used to display the graphics.

When MonoGame finish their implementation of the content build, one of the projects will be deleted.

## GTA2NET Core ##
This is the server part, because of that is also the one that contains most of the code.
Even that at the first look it looks difficult and that is to much code, it is the other way around it is actually a simple project, there are two reason beyond that much classes, one of the reasons is that Alex in the beginning create almost all of entities of the game, most of them have little code and are not used at the moment, the second reason is the extensive use of inheritance that some parts of the game allow(look at the Block folder) this way we have less redundant code and allow us to extend the game in a easy way.

And now a brief description of the subparts of this project.
### Collision ###
In this folder are all the related parts of the physics engine, it's responsible for calculate collisions, and movement of cars and peds.
I don't have know must about this code since it was all created by Alex and I haven't look at it with much detail. At the moment there is a limitation in this code, at the moment all map entities are in the same plane, so this must be extended to support multiple planes (at least 7 or 8 to make have the same collisions as in the GTA2).

### Helper ###
It have a lot of small classes with helper function.

### Logic ###
This one contains the logic of the game.
The most interesting things in this folder are:
  * GameObject represents a object in the game, it is the base class for all the game objects that are represent in the map and the player could interact with.
  * ControlableGameObject this extends GameObject and is a base class to all object that are gonna be controlable by the player or by the AI.
  * Vehicle this extends ControlableGameObject and is the base class to implement vehicles.
  * pedestrian extends ControlableGameObject and is the base class to implement pedestrians.

### Map ###
Interesting things:
  * Map represent the game map. internally has all the things that conposes a GTA2 map e.g Blocks, Zones, MapObjects. At the moment only the Blocks are used.
  * Block is the base class that represent a Block.
  * DiagonalBlock and SlopeBlock the base class to create Diagonal Blocks and Slope Blocks, they have method thay make the creation of this blocks simpler.
  * BlockFace is the repesentation of each face of the block, this ones if exist in a Block creates a wall, and stores it's texture.