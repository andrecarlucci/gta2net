**Help needed**
I would appreciate if some developers would join the team :)
And we need someone who could create a pretty icon for the application.

_Introduction_

As the original GTA2 client doesn't work very will in newer versions of Windows, I decided to create my own version of GTA2.

Primarily redo the multiplayer part of the game. Existing maps and graphics will be reused. But it is also planned to add addional features.

GTA2 assets (graphics, sounds, etc...) is not included in this project, the free version GTA2 version from Rockstar Games is required. The source of GTA was not decompiled. I just used the official documentation of the map and style files. These specifications are available [here](http://www.hiale.net/GTA2NET/gta2formats.zip).

GTA2NET is written in C# and ~~XNA~~ MonoGame.


---


_Frequently Asked Questions_

**Q**: Why do you create this remake?

**A**: There are several reasons:
  * To upgrade my game programming skills.
  * To pleasure my girlfriend when we play OUR game.
  * GTA2 multiplayer rocks!
  * I loved to play GTA2 with a good friend of mine who sadly deceased in 2001. I want to honor him.

**Q**: I can run GTA2 on a Pentium 200 Mhz and 32 MB RAM, why does GTA2NET run so slowly?

**A**: GTA was developed more than 10 years ago. They had to save resources everywhere. In the Map Format specification (page 6) they say:
> "The memory required by the uncompressed map is 256x256x8x12 bytes = 6MB . Clearly, this is too large to be practical in-game."
I believe nowadays it doesn't matter if a map takes 6 MB of system RAM. Apart from that, the managed nature of .NET/XNA has a small overhead. My computer (i7) runs the current version (see screenshot) at 2200+ frames per second. This number will decrease when more features are present, but should be enough for the game.

**Q**: Have you decompiled the game executable?

A: No, I just used the format specifications to reimplement GTA2. I discovered addional things by playing the original game.

**Q**: Will this included also the single player part?

A: I'd love to, but the script files are not documented. Missions are descriped in these files. So chances are low...

**Q**: Do I need to have GTA2 (original) installed?

**A**: You will need some data files fron GTA2, if you have these files somewhere, you are fine. I will publish a list of all needed files when the project is more advanced.

**Q**: Can I use the freeware version of GTA2 from http://www.rockstargames.com/classics/ ?

**A**: Absolutely! I also use this version to develop the game.

**Q**: When will you release the final game?

**A**: It's done when it's done!


---


_Screenshots_

![http://www.hiale.net/GTA2NET/Screenshot6.png](http://www.hiale.net/GTA2NET/Screenshot6.png)

More to come...