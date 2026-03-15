# Director's Eye - WebXR Version

Browser-based VR pre-visualization tool built with React, Three.js, and WebXR.

## Features

- **Voice Commands**: Speak scene descriptions to generate environments
- **WebXR Support**: Works with Quest, Pico, and other VR headsets in browser
- **Desktop Mode**: Full functionality without VR hardware
- **Actor Placement**: Click to place poseable actor mannequins
- **Virtual Cameras**: Place cameras with adjustable focal length and aspect ratio
- **Shot Capture**: Save storyboard frames with camera metadata
- **Scene Refinement**: Voice commands to modify the scene ("add fog", "make it sunset")

## Tech Stack

- **React 18** + TypeScript
- **Three.js** via @react-three/fiber
- **WebXR** via @react-three/xr
- **Zustand** for state management
- **Vite** for build tooling

## Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

## Environment Variables

Create a `.env` file:

```
VITE_API_URL=http://localhost:8000
```

## Usage

### Desktop Mode
1. Open http://localhost:5173
2. Type a scene description or click 🎤 for voice
3. Select mode: Actor 🎭 or Camera 📷
4. Click on the floor to place objects
5. Click 📸 to capture shots

### VR Mode
1. Open the URL in a WebXR-compatible browser
2. Click "Enter VR"
3. Use controllers to point and interact
4. Trigger to place/capture, Grip to grab

## Deployment

### Vercel (Recommended)
```bash
npm install -g vercel
vercel
```

### Netlify
```bash
npm run build
netlify deploy --prod --dir=dist
```

## Project Structure

```
src/
├── components/
│   ├── XRScene.tsx          # Main 3D canvas
│   ├── ActorModel.tsx       # Actor mannequin
│   ├── VirtualCamera.tsx    # Camera rig
│   ├── GeneratedEnvironment.tsx
│   └── UI/
│       ├── ControlPanel.tsx # Main UI
│       └── VRButton.tsx     # VR entry
├── hooks/
│   ├── useVoiceRecording.ts
│   ├── useSceneGeneration.ts
│   └── useShotCapture.ts
├── stores/
│   └── appStore.ts          # Zustand state
├── api/
│   └── backend.ts           # API client
├── types/
│   └── index.ts             # TypeScript types
├── App.tsx
└── main.tsx
```

## Requirements

- Node.js 18+
- Modern browser with WebXR support (for VR)
- Backend server running (see /backend)
