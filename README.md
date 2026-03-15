# Director's Eye

**VR Pre-Visualization Tool for Filmmakers**

Describe your vision. Step inside it. Direct from within.

## Overview

Director's Eye is a spatial filmmaking tool that lets directors:
1. Speak or type a scene description → AI generates a 3D environment
2. Step inside the scene on Pico VR headset
3. Block shots spatially - place virtual actors, set camera positions
4. Iterate instantly - refine the environment with natural language
5. Export shot lists with camera angles and reference frames

## Tech Stack

- **VR Platform**: Pico 4 / Pico 4 Ultra
- **Engine**: Unity 2022 LTS + Pico SDK
- **World Generation**: World Labs Marble API
- **Voice Input**: OpenAI Whisper
- **Backend**: Python FastAPI

## Project Structure

```
directors-eye/
├── unity/
│   └── Assets/
│       ├── Scripts/
│       │   ├── Core/           # Main systems
│       │   │   ├── DirectorsEyeApp.cs
│       │   │   ├── SceneGenerationManager.cs
│       │   │   ├── MarbleAPIClient.cs
│       │   │   ├── VoiceInputManager.cs
│       │   │   └── ShotCaptureManager.cs
│       │   ├── VR/             # VR interaction
│       │   │   ├── VRInteractionManager.cs
│       │   │   ├── VirtualCameraRig.cs
│       │   │   └── ActorPlaceholder.cs
│       │   └── UI/             # User interface
│       │       └── DirectorsEyeUI.cs
│       ├── Prefabs/
│       ├── Scenes/
│       └── Materials/
├── backend/
│   ├── app/
│   │   ├── main.py             # FastAPI server
│   │   └── marble_client.py    # Marble API client
│   ├── requirements.txt
│   └── .env.example
└── docs/
```

## Setup

### Backend

```bash
cd backend
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt

# Copy and configure environment
cp .env.example .env
# Edit .env with your API keys

# Run server
python -m uvicorn app.main:app --reload
```

### Unity

1. Open Unity Hub → Add project from disk → Select `unity/` folder
2. Install Pico SDK via Package Manager
3. Install GLTFUtility for mesh loading
4. Configure build settings for Android (Pico)
5. Set backend URL in DirectorsEyeApp inspector

## Usage

1. Put on Pico headset and launch app
2. Press A button to start voice recording
3. Describe your scene: "A rooftop at night, neon city below, rain falling"
4. Wait for environment to generate around you
5. Use B button to cycle modes: Pointer → Place Actor → Place Camera → Place Marker
6. Aim and press trigger to place objects
7. Grab actors/cameras with grip to reposition
8. Press trigger in Pointer mode to capture shot
9. Say "Add more fog" or "Make it sunset" to refine

## API Keys Required

- **World Labs Marble API**: https://worldlabs.ai
- **OpenAI API** (for Whisper): https://platform.openai.com

## Voice Commands

| Command | Action |
|---------|--------|
| Scene description | Generate new environment |
| "Make it..." / "Add..." / "Change..." | Refine current scene |
| "Place actor" | Switch to actor placement mode |
| "Place camera" | Switch to camera placement mode |
| "Capture" / "Take shot" | Save current camera frame |

## Hackathon Info

Built for **Worlds in Action Hack - San Francisco**
Category: Best Filmmaking, Entertainment App

### Requirements Met
- ✅ World Labs Marble API for environment generation
- ✅ Pico VR headset support
- ✅ Unity integration
- ✅ World model-generated environment as core feature
