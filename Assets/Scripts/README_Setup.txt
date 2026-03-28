BattleBeasts Grid Prototype Setup
=================================

This is a first-pass 1v1 grid prototype.

What this prototype currently does
----------------------------------
- 17 x 35 total battlefield.
- Two 17 x 17 sides with a center line at x = 17.
- Player beast starts at global tile (8,8).
- Enemy beast starts at global tile (26,8).
- Left click a tile to move the player beast there.
- Press 1 to use the player's basic attack against the enemy's CURRENT tile.
- Press 2 to use the player's special attack against the enemy's CURRENT tile.
- Press Space to dodge away from the enemy.
- Hold Left Shift to slow time while choosing commands.
- Enemy uses simple AI: reposition, basic attack, special attack.
- Charging attacks show telegraph markers on affected tiles if you assign a tile marker prefab.

Recommended scene setup
-----------------------
1. Create a new empty GameObject called BattleManager.
2. Add:
   - BattleManager
   - BattleGridManager
   - AttackTelegraphSystem
   - BattleCommentaryLog (optional if you have a UI Text)
   - BattleUIManager (optional UI)
3. Create a plane or large quad to represent the battlefield.
   - Give it a Collider so the mouse raycast can hit it.
   - Put it on a layer included in BattleGridManager.gridRaycastMask.
4. Position the battlefield so that the bottom-left corner of tile (0,0) lines up with BattleRulesData.gridOrigin.
   With tile size 1, the whole grid occupies:
   - Width: 35 units
   - Height: 17 units
5. Create a BattleRulesData asset.
   Recommended defaults:
   - gridWidth = 35
   - gridHeight = 17
   - tileSize = 1
   - gridOrigin = (0,0,0)

Beast setup
-----------
1. Create two beast GameObjects in the scene:
   - PlayerBeast
   - EnemyBeast
2. Add to each:
   - BeastController
   - BeastGridMover
   - BeastActionExecutor
3. Add EnemyBeastAI to the enemy beast only.
4. Assign BeastData assets to each BeastController.
5. Set Team on BeastController:
   - PlayerBeast = Player
   - EnemyBeast = Enemy
6. Add visible meshes/cubes/capsules.
7. Drag those renderers into tintRenderers if you want auto team tinting.

Grid hover marker
-----------------
- Create a simple small flat marker object, for example:
  - a thin quad or cylinder
- Assign it to BattleGridManager.hoverMarker
- Disable it in the scene initially if you want
- The script will move and show/hide it automatically

Telegraph marker prefab
-----------------------
- Create a simple flat prefab (quad/cylinder)
- Scale it to about 0.85 x 0.02 x 0.85 for a tile marker
- Use a transparent material if possible
- Assign it to AttackTelegraphSystem.tileMarkerPrefab

UI setup
--------
You can leave UI references empty if you just want to test with Debug.Log.

If you want UI:
- Create Canvas
- Create Sliders and Text elements
- Assign them in BattleUIManager and BattleCommentaryLog

Input setup
-----------
- Add PlayerCommandInput to the BattleManager GameObject or another scene object.
- Assign your battle camera to PlayerCommandInput.battleCamera.
- Assign that PlayerCommandInput reference into BattleManager.playerInput.

Ability setup
-------------
Create AbilityData assets for basic and special moves.

Suggested starter values:

Basic attack:
- rangeInTiles = 4
- impactRadiusInTiles = 0.5 to 1
- windupTime = 0.35
- recoveryTime = 0.25
- cooldown = 1

Special attack:
- rangeInTiles = 12
- impactRadiusInTiles = 3 to 5
- windupTime = 1.2
- recoveryTime = 0.75
- cooldown = 4

Notes
-----
- The player beast can currently move to any non-center-line unoccupied tile.
- Abilities target the ENEMY'S CURRENT TILE at cast start.
- If the enemy moves away before impact, the attack can miss.
- This is intentional for early testing.
- There is no final menu yet; slow-mo is just a first-pass input aid.

Likely next improvements
------------------------
- Restrict move range per command instead of allowing full-map move clicks
- Add a proper command menu
- Add path previews and tile highlights
- Add local-side movement preferences / side restrictions
- Add proper facing/animation
- Add location-targeted abilities
- Add better enemy intent UI
