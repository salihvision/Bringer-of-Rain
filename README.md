# Bringer of Rain

A Hollow Knight-inspired 2D Unity action-platformer set in a drought-stricken desert aqueduct. Developed during the OTGET Game Jam 3 (April 24-26, 2026) at Manisa Celal Bayar University.

## Gameplay Video

Watch the 2-minute gameplay demo: [Bringer of Rain Gameplay Demo](https://drive.google.com/file/d/1y1TwvweTThtAiSeVNvhSjQySl2N-HlBy/view?usp=sharing)

## Features

- Runtime-built 2D platforming level with three chapter-style areas
- Story prompts, readable signs, and a village elder NPC
- Water-themed player abilities, mana, projectiles, enemies, hazards, and checkpoints
- Desert tiles, villager sprites, full-screen story panels, rain effects, and a trophy moment
- 2-phase Boss fight

## Controls

- Move: `A/D` or arrow keys
- Jump: `Space`
- Read/interact: `W`
- Water whip: `LMB` / enter / `F`
- Burst: `S+LMB`
- Ice shard: `RMB` (Costs 1 mana. Water whip or Burst 3 times.)

## Setup

This project uses Unity `6000.4.3f1`.

1. Clone the repository.
2. Open the project folder in Unity Hub.
3. Use Unity `6000.4.3f1` or a compatible Unity 6 version.
4. Open `SampleScene`.
5. Press Play.

## Project Notes

The level is constructed mostly through runtime scripts instead of a hand-authored tilemap scene. Most gameplay setup lives in `Assets/Scripts/DesertAqueductBootstrap.cs`, with supporting scripts for player movement, combat, enemies, story UI, audio, and progression.

## Credits

Created as a game jam project. Development included gameplay programming, level scripting, story integration, UI polish, and asset integration.

<img width="467" height="621" alt="image" src="https://github.com/user-attachments/assets/8be1f35f-9b27-424f-8f78-d536f616dd57" />
