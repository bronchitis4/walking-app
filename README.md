# Autumn Park — 2D online hangout

A small browser game: players join a shared autumn park, walk around as Dmytro or Yura, see each other's nicknames and chat with speech bubbles.
Built with Unity 6 (6000.3.10f1), URP 2D, Netcode for GameObjects and Unity Multiplayer Services (Lobby + Relay over secure WebSockets). Deployed as WebGL to GitHub Pages.

## Controls
| Key | Action |
|---|---|
| A / D or ← / → | Walk |
| Space / W / ↑ | Jump |
| Enter | Open chat / send message |
| Esc | Close chat |

## Run locally
1. Open the project in Unity **6000.3.10f1**.
2. Open `Assets/Scenes/AutumnPark.unity` and press Play.
3. Enter a nickname, pick a character, press **Create room**. The room code appears top-left.
4. To test with two players, either build WebGL and open it in two tabs, or use **Window → Multiplayer → Multiplayer Play Mode** to start a second virtual player, then **Join** with the code.

> If creating a room fails with "Burst failed to compile the function pointer", restart the Unity Editor once (happens after first installing the networking packages).

## Link Unity Cloud (needed for Lobby + Relay)
Relay and Lobby only work when the project is linked to a Unity Cloud project.
1. **Edit → Project Settings → Services**: sign in and link the project to your organization/project (the Project ID shows there).
2. In the [Unity Dashboard](https://cloud.unity.com) open that project and make sure **Relay** and **Lobby** are enabled (Multiplayer → Relay / Lobby → Get started).
3. Nothing else to configure: players sign in anonymously; each browser tab uses its own anonymous profile.

## Deploy to GitHub Pages
The workflow `.github/workflows/deploy.yml` builds WebGL with [game-ci/unity-builder](https://game.ci) on every push to `main` and publishes it to GitHub Pages.

1. Create a GitHub repository and push this project (the `.gitignore` excludes `Library/`, `Temp/`, `Builds/`, etc.).
2. Add repository secrets (**Settings → Secrets and variables → Actions**):
   - `UNITY_LICENSE` — contents of your `.ulf` license file (see game.ci docs: *Activation*)
   - `UNITY_EMAIL`, `UNITY_PASSWORD` — your Unity account
3. **Settings → Pages → Build and deployment → Source: GitHub Actions**.
4. Push to `main`. When the workflow finishes, the game is at `https://<user>.github.io/<repo>/`.

### Manual build
**File → Build Profiles → Web → Build** into `Builds/WebGL`. The folder contains `index.html` at its root and can be pushed to a `gh-pages` branch as is.

WebGL settings already applied: compression **Disabled** (GitHub Pages does not send `Content-Encoding` for `.br`/`.gz`), engine code stripping on, responsive template `Assets/WebGLTemplates/Responsive`.

## Project layout
| Path | What |
|---|---|
| `Assets/Scenes/AutumnPark.unity` | The game scene |
| `Assets/Scripts/` | Game code, namespace `RoomGame` |
| `Assets/Prefabs/Player.prefab` | Networked player |
| `Assets/Art/Characters/` | Dmytro and Yura sprite sheets, clips, `Player.controller` |
| `Assets/Art/Environment/Autumn/` | Park art (packed by `AutumnAtlas.spriteatlasv2`) |
| `Assets/Art/Fonts/` | Pixelify Sans (SIL OFL, see `PixelifySans_OFL.txt`) — supports Cyrillic |

## Networking notes
- The room creator is the host; everyone connects through Unity Relay (no dedicated server). On WebGL the transport uses WebSockets and the Relay connection is secure (wss), as required by an HTTPS page.
- Movement is owner-authoritative (`NetworkTransform`, owner authority). Animation state, facing, nickname and skin are owner-written `NetworkVariable`s.
- Chat is server-authoritative: client → server RPC (trimmed to 120 chars) → broadcast to everyone → bubble above the sender for ~4 s.
- If the host leaves, clients see a message and return to the connect screen.
