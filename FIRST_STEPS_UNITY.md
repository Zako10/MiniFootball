# MiniFootball first playable setup

## Fast setup

After Unity finishes compiling scripts:

1. Open `Assets/Scenes/SampleScene`.
2. From the top menu, click `MiniFootball > Build First Playable Scene`.
3. Save the scene with `Ctrl + S`.
4. Press Play.

Controls:

- Move: `WASD` or arrow keys.
- Kick: just run into the ball.

If the menu item does not appear, wait for Unity to finish compiling. If the Console shows an error, send it here.

## Manual setup

This is the small first target:

1. One player controlled by keyboard.
2. One computer player chasing the ball.
3. A ball that can be kicked by collisions.
4. Two trigger areas that count goals.
5. A game manager that resets everyone after a goal.

## Scene setup in Unity

1. Open `Assets/Scenes/SampleScene`.
2. Drag these prefabs into the scene:
   - `Assets/Lightning Poly/Football Essentials 3D/Prefabs/Ground.prefab`
   - `Assets/Lightning Poly/Football Essentials 3D/Prefabs/Ball.prefab`
   - `Assets/Lightning Poly/Football Essentials 3D/Prefabs/Character.prefab` twice
   - `Assets/Lightning Poly/Football Essentials 3D/Prefabs/Goal.prefab` twice
3. Rename the two characters:
   - `Player`
   - `Computer`
4. Select the ball and set its tag to `Ball`.
5. Make sure the ball has a `Rigidbody` and a collider.
6. Make sure both characters have a `Rigidbody` and a collider.

## Scripts

1. Add `SimplePlayerController` to `Player`.
2. Add `SimpleComputerController` to `Computer`.
3. Add `SimpleFollowCamera` to `Main Camera`.
4. Create an empty GameObject called `GameManager`.
5. Add `MiniFootballGameManager` to `GameManager`.

## Inspector references

On `SimpleComputerController`:

1. Drag `Ball` into `Ball`.
2. Optional: create an empty `ComputerHome` and drag it into `Home Position`.

On `SimpleFollowCamera`:

1. Drag `Player` into `Target`.

On `MiniFootballGameManager`:

1. Drag `Player` into `Player`.
2. Drag `Computer` into `Computer`.
3. Drag `Ball` into `Ball`.
4. Create three empty objects:
   - `PlayerSpawn`
   - `ComputerSpawn`
   - `BallSpawn`
5. Put them where the round should start.
6. Drag them into the matching spawn fields.

## Goal triggers

Each goal needs an invisible trigger box:

1. Create a cube inside/behind each goal mouth.
2. Disable its Mesh Renderer.
3. Enable `Is Trigger` on its Box Collider.
4. Add `GoalDetector`.
5. Drag `GameManager` into `Game Manager`.
6. For the computer goal, set `Scoring Side` to `Player`.
7. For the player goal, set `Scoring Side` to `Computer`.

## Controls

- Move: `WASD` or arrow keys.
- Kick: just run into the ball.
