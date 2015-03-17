# Introduction #
This are a few things that from the top of my head are left to do and all the infrastruture to do them are in place.


# Details #

GTA2CORE:
- Create the game loop.

GRAFICS:
  * finish Textures.GetLittleTexture() easy to do but tedious.
  * Fix game render.
    * ATM the map textures and the "sprites" are loaded into two separated Texture2D, this makes dificult to show both images at the same time, possible solutions:
      * join all images into a single Texture2D
      * create a new "effect", is necessary some HLSL programing. <- this is probably the best option since it could be expanded in the future to support ligts.

physics:
  * Colisions only work on one plane, this is a limitation of the pysics engine.
  * some bugs in movement of cars.

MAP:
  * some of the blocks are not implemented. (Blocked by the GetLittleTexture)

BLOCKS:
  * face animations. (blocked by gameLoop)