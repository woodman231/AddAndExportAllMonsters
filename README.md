# Monster Sanctuary Add and Export All Monsters

This is a BepInEx plugin for the Monster Sanctuary Video Game.

This has only been tested on Windows, and not WINE.

If you want to run this plugin you must first install BepInEx 5 following this [guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

After you have installed and ran BepInEx at least once take the following actions:

1. Create the following folder structure:
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\Buffs
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\Debuffs
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\Enums
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\Items
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\Monsters
  - GameDirectory\BepInEx\plugins\AddAndExportAllMonsters\SpecialBuffs

2. Build this project in your Visual Studio.

   If you have problems building you may need to set up your development environment as described in this [guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/1_setup.html)

3. In your project directories bin\Debug\net46\ folder copy the contents to the GameDirectory\BepInEx\plugins\AddAndExportAllMonsters folder

4. Launch your game

5. Start a new game with the Spectral Wolf (this is a must)

6. After the standard introduction and you are able to move your character around Save and Quit the game.

7. Continue your game with the save slot where you just created your game.

8. You should now have every monster with every shift.

9. You will also have the data in the folders that you created earlier populated with files.

## Some things to note:
- This takes advantage of Newtonsoft.Json.
- When I first built this application it would complain that the following DLLs could be loaded:
  * System.Data.dll
  * System.Numerics.dll
  * System.Runtime.Serialization.dll
  * System.Runtime.Serialization.Primatives.dll

- As a result I copied them to this projects directory and set them to copy always. These were random versions that I found on my computer. I couldn't figure out a way to ensure that the same ones Newtonsoft.Json wanted / were using to compile in to the project so I went that route for now.

- Once upon a time I thought it would be a good idea to export each shift of the monster to it's own file, but I changed my mind on that. However, I still left in the code that would add all monsters of all shifts just to play around with builds.

- Using monsters that were added to the player instead of monsters from the prefab allowed me to take advantage of the GetToolTip features of skills to get the best possible descriptions. A couple of examples were the skill IDs and names are the same, but the tooltips / descriptions are different are for the Health Plus skill. It increases the rating by 1, but how much health it gives the monster is different.

- To prevent reference loops I chose to either get the game objects referenceable id(s) or sometimes I would get the game object's properties. It was a little inconsistant and a judgement call on my part. So you may want to adjust things like ParentAction for skills, or UpgradesTo for items to better suite your specific needs / use case.

- I used allot of List<object> and Dictionary<string, object> types so that I wouldn't have to know / analyze the properties of each type of PassiveSkill and just loop over their fields and try to get the gameObject as that type, then adhock created the Diction<string, object> and added the results of it to the List<object>

- I would agree that it would have been better to start with some model classes that represented the data that I wanted: however, I didn't know what I needed to know. So this is how I found out. I may back track and create classes, but as you can tell the objects are pretty complex.

- This was inspired by [https://github.com/Eradev/MonsterSanctuaryMods](https://github.com/Eradev/MonsterSanctuaryMods)