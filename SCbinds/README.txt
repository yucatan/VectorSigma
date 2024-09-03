###########
# SCbinds #
###########

What can I say? I am not a programmer, but since I was planning to bring Sigma to the 'verse of Star Citizen, first I needed it to "detect" the keybinds from the game.

Inspired by bindED project, after days struggling with ChatGPT I decided to write the code in a "language" I am familiar with. So I wrote SCbinds.sh (yep, Shell Script). It kindda did 95% of what I needed. It only wouldn't create the variables in VoiceAttack. Then I asked ChatGPT 3.5 to port/convert my code to C#, in order to use it as an Inline Function in VA. And this is the initial work (of course I needed to do a few adjustments).

Feel free to use SCbinds as it is or, in case you are feeling generous and want to contribute to the project by optimizing it (yeah those 3 temporary files are not pretty), please let me know and I will be glad to "upgrade" the code.

Well, enough of this. Let me get back to working on Sigma-SC.


-------

* defaultProfile-3.24.0-live-9296942.xml = the defaultProfile.xml file (plus the game version number) that must be extracted from "GAME-PATH\LIVE\Data.p4k" (so yep, everytime CIG adds/changes the default keybinds I will have to extract it again.
* keyboard-map-en.txt = "borrowed" from bindED project (really thanks alterNERDtive)
* layout_Sigma_exported.xml = my own keybinds, exported/saved from the game
* SCbinds.sh = The original Shell Script
* SCbinds-1.1.cs = A copy of the original C# code, just with "version number"
