# 3D Multiplayer FPS Game

(Oct 2022 - Jan 2023)
A 3D multiplayer first-person shooter built in Unity using C# and Mirror Networking. Features custom networked player movement, direct IP server hosting/joining, and raycast-based gun mechanics.

![Gameplay Demo](demo.gif)

## Overview

The project focuses on networking logic, state synchronization, and custom UI/gameplay systems built on top of Mirror Networking. Key technical components include:

- **Networked Movement & Physics:** Player positions, actions and interactions synchronized using Mirror Networking library.
- **Weapon System:** Raycast hit detection with headshot multipliers, recoil timers, dynamic camera FOV zooming, and real-time HUD UI updates.
- **Combat & Health Loop:** ClientRpc/TargetRpc architecture for damage syncing, floating damage popups, dynamic kill/death stats, and death camera transitions.
- **Custom Connection Pipeline:** Low-overhead IP server pinging, direct IP join logic, and custom connection handling bypassing Mirror's default loop.
- **Server Management:** Local server list persistence using PlayerPrefs with UI controls for adding, editing, and refreshing server entries.

## Codebase

The core gameplay, networking, and UI logic is structured across the following C# scripts:

* [`Assets/Scripts/PlayerMovement.cs`](Assets/Scripts/PlayerMovement.cs) – Main client-server connection manager handling local player movement & camera, UI, death sequences, and weapon firing & replicating system.
* [`Assets/Scripts/Gun.cs`](Assets/Scripts/Gun.cs) – Controls weapon stats, recoil timers, zoom FOV interpolation, magazine reload loops, and UI indicators.
* [`Assets/Scripts/PlayerHealthManager.cs`](Assets/Scripts/PlayerHealthManager.cs) – Manages health states, attacker tracking, damage calculation, and damage data replication.
* [`Assets/Scripts/MenuManager.cs`](Assets/Scripts/MenuManager.cs) – Manages UI panels, main menu state transitions, and NetworkManager hosting/client connections.
* [`Assets/Scripts/ServerSearch.cs`](Assets/Scripts/ServerSearch.cs) – Handles server list persistence via PlayerPrefs, spawning UI entries for saved servers, and deletion logic.

## Requirements & Dependencies

* Unity 2021.3.6 (or newer)
* Mirror Networking Library
* Standard Unity Library
