# 🐔 Chicken Rush

A colourful low-poly 3D chicken-crossing game inspired by **Crossy Road**, built with **Unity 6.3 LTS** and **C#**.

Everything on screen (chicken, cars, trees, roads, and UI) is made from Unity primitives and built-in UI, so there are **no paid assets and no external dependencies**.

---
## 🎮 Gameplay Demo

Download Vidoe:
[Watch Gameplay](video/demo.mp4)

Watch the gameplay demonstration of Chicken Rush:

[▶️ Watch Chicken Rush Gameplay](https://drive.google.com/file/d/14FGmT2y5ut3XL0ARDC2uNHmaISaG-c_5/view?usp=sharing)

---



---

## Game objective

Guide the chicken across grass and busy roads to the **gold finish line (row 30 by default)** without getting hit by a car.

## Game rules

- The chicken moves **one grid cell per key press** (you can tap quickly: one extra input is buffered mid-hop).
- Cars drive along road lanes at **different speeds and in different directions**. Touching a car = **Game Over**.
- **Trees block movement**, and the edges of the play area cannot be crossed.
- Your **score is the furthest row reached** (going backwards never lowers it).
- Reaching the finish row = **Victory**.
- Your **best score is saved locally** between sessions using Unity `PlayerPrefs`.
- Levels are **procedurally generated**, so every run has a different layout of roads, trees, and traffic.

## Controls

| Key | Action |
|---|---|
| `W` / `↑` | Move forward |
| `S` / `↓` | Move backward |
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `Space` | Hop in place |
| `Enter` | Start game (main menu) |
| `R` or `Enter` | Restart (Game Over / Victory screens) |

The on-screen **Start**, **Restart**, and **Quit** buttons also work with the mouse.

**Tip:** Wait for a gap, hop forward with `W`, and use `A`/`D` to line up around trees. Lanes next to each other usually travel in opposite directions.

---

## Run from source

### Requirements

- **Unity 6.3 LTS (6000.3.8f1)** or a compatible Unity version.
- Unity Hub.
- Windows, macOS, or Linux computer.
- Unity 3D project using the Built-in or Universal Render Pipeline (URP).

### Steps

1. Clone this repository from GitHub.
2. Open Unity Hub.
3. Click **Add → Add project from disk**.
4. Select the cloned `ChickenRush` project folder.
5. Open the project using Unity.
6. Wait for Unity to import assets and compile scripts.
7. Open `Assets/Scenes/Main.unity`.
8. Press **Play** to start the game.

If `Assets/Scenes/Main.unity` does not exist, the scene can be generated using:

**Chicken Rush → 1. Generate Game Scene**

This generates the materials, scene, lighting, camera, UI, and build settings.

### Unity Editor: Scene and Game Views

When opening the project, you can use both **Scene** and **Game** views to inspect the game.

- **Scene View:** Displays the game environment and Unity GameObjects, allowing you to inspect the chicken, roads, trees, cars, camera, and lighting.
- **Game View:** Displays the game from the player's camera perspective.

To display both views:

1. Open `Assets/Scenes/Main.unity`.
2. Go to **Window → General → Scene**.
3. Go to **Window → General → Game**.
4. Drag the Game tab next to the Scene tab and dock them side by side.
5. Adjust the panel sizes to see both views.
6. Go to **Layout → Save Layout** to save your preferred editor arrangement locally.

**Note:** Unity Editor layouts are saved as local editor preferences and are not guaranteed to transfer when another person clones the repository. The `Main.unity` scene must be included in GitHub so the teacher can open the actual game scene.

---

## Play it (build download)

1. Download the build from the repository's **Releases** page or the provided download link.
2. **Windows:** Unzip the downloaded file and run `ChickenRush.exe`.
3. **macOS:** Unzip the downloaded file and open `ChickenRush.app` (right-click → Open the first time if macOS blocks it).
4. Press **Enter** or click **Start**.

No installation or extra software is needed. Default window size is 1280×720.

---

## Making a standalone build

Use the Unity menu:

- **Chicken Rush → 2. Build Windows 64-bit**
- **Chicken Rush → 3. Build macOS**

Build output goes to `Builds/` (ignored by Git, so upload the zipped build to Releases or Google Drive).

The matching build-support module must be installed in Unity Hub.

Batch mode (no editor UI):

```bash
Unity -batchmode -quit -projectPath . \
  -executeMethod ChickenRush.EditorTools.ChickenRushSceneBuilder.BuildSceneBatch
```

---

## Project structure

```text
ChickenRush/
├── README.md
├── .gitignore
└── Assets/
    ├── Editor/
    │   └── ChickenRushSceneBuilder.cs
    │       Builds materials, scene, UI, build settings and build menu
    ├── Scripts/
    │   ├── GameManager.cs
    │   │   State machine: Menu → Playing → GameOver / Victory → Restart
    │   ├── ScoreManager.cs
    │   │   Score, best score (PlayerPrefs), target row
    │   ├── PlayerController.cs
    │   │   Grid movement, kinematic Rigidbody hop, death
    │   ├── CarController.cs
    │   │   Lane movement, wrap-around recycling, trigger hit detection
    │   ├── TrafficSpawner.cs
    │   │   Creates the cars for each road lane
    │   ├── EnvironmentGenerator.cs
    │   │   Procedural rows: grass, trees, roads, markings, finish line
    │   ├── CameraFollow.cs
    │   │   Fixed-angle smooth follow camera
    │   ├── UIManager.cs
    │   │   Menu / HUD / Game Over / Victory panels and buttons
    │   ├── GamePalette.cs
    │   │   Material references
    │   ├── PrimitiveFactory.cs
    │   │   Builds the chicken and cars from Unity primitives
    │   └── InputReader.cs
    │       Keyboard input (old Input Manager + new Input System)
    ├── Materials/
    │   └── Generated materials
    └── Scenes/
        └── Main.unity
            Generated main game scene
```

---

## How the code works

- **Game flow:** `GameManager` owns the state (`Menu`, `Playing`, `GameOver`, `Victory`) and raises a `StateChanged` event. `UIManager` listens and shows the correct panel; `TrafficSpawner` pauses cars outside Menu/Playing.
- **Level generation:** `EnvironmentGenerator.Generate()` lays out rows from behind the start to past the finish. It places grass and road groups, trees (kept off a guaranteed walkable corridor), lane markings, and the finish line. For each road row it picks a direction (alternating for adjacent lanes) and a random speed (about 2–5.5 units/s), then asks `TrafficSpawner` for cars.
- **Movement:** `PlayerController` reads input in `Update` and performs the hop in `FixedUpdate` via a kinematic `Rigidbody.MovePosition`, which keeps trigger collisions with cars reliable. Trees and map edges are checked with `InBounds` / `IsBlocked`.
- **Traffic:** Each car has a kinematic `Rigidbody` and a trigger `BoxCollider`. `CarController` moves it along the lane, teleports it to the other side when it leaves the area, and calls `PlayerController.Die()` on contact.
- **Scoring:** `ScoreManager.ReportRow()` only accepts forward progress. The best score is committed on Game Over / Victory.
- **Setup automation:** `ChickenRushSceneBuilder` (Editor-only) creates the whole scene so the project works from the Assets folder.

---

## Tweaking the game

Most values are exposed in the Inspector on the generated scene's objects.

| Setting | Where | Default |
|---|---|---|
| Target (finish) row | `ScoreManager.targetScore` | 30 |
| Tree density | `EnvironmentGenerator.treeDensity` | 0.18 |
| Play area half-width | `EnvironmentGenerator.playHalfWidth` | 5 |
| Fixed or random level | `EnvironmentGenerator.randomSeed` / `seed` | Random |
| Cars per lane | `TrafficSpawner.minCarsPerLane` / `maxCarsPerLane` | 2–4 |
| Hop speed / height | `PlayerController.hopDuration` / `hopHeight` | 0.14 s / 0.4 |
| Camera angle and distance | `CameraFollow.pitch` / `yaw` / `distance` | 48° / −18° / 14 |


## External assets, tutorials and AI use

- **Assets:** None. Only Unity primitives and Unity's built-in UI are used.
- **Tutorials / references:** `<List references or write "None">`
- **AI assistance:** The initial code and scene-generation tooling were drafted with Claude (Anthropic). The team should review, understand, test, and document any changes made to the generated code in accordance with the course's AI-use policy.

---

## Testing checklist

- [ ] Project opens with no compile errors in the Console.
- [ ] `Main.unity` exists and loads.
- [ ] Scene View displays the game environment.
- [ ] Game View displays the game from the camera perspective.
- [ ] Menu shows; Start button and `Enter` both start the game.
- [ ] Chicken moves one cell per key (WASD and arrows).
- [ ] Trees and map edges block movement.
- [ ] Cars move in both directions at different speeds and wrap around.
- [ ] Being hit shows Game Over with score and best score.
- [ ] Reaching the finish row shows Victory.
- [ ] Restart (button and `R`) starts a fresh run.
- [ ] Standalone build runs outside the Unity Editor.
- [ ] Video and build links open without logging in.

---

## Known limitations

- Keyboard only (no touch or gamepad support).
- No audio.
- Unity Editor layout is not automatically shared between computers.
- The repository must include the generated `Main.unity` scene for the teacher to open it directly.
