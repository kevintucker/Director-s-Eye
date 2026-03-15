# Director's Eye - Complete Build Guide

## Platform Options

| Option | Platform | Pros | Cons | Build Time |
|--------|----------|------|------|------------|
| **Option A** | Pico VR (Native) | Best performance, full hand tracking, immersive | Requires Pico headset, longer dev time | 40-48 hrs |
| **Option B** | WebXR (Browser) | Works on Quest/Pico/desktop, easy sharing, no app store | Lower performance, limited features | 30-36 hrs |
| **Option C** | Desktop App | Easy to demo, mouse/keyboard, quick iteration | Not spatial, loses core value prop | 20-24 hrs |
| **Option D** | Hybrid (Web + VR) | Best of both worlds, wider reach | More code to maintain | 48+ hrs |

---

# Option A: Pico VR Native App (Recommended for Hackathon)

## Prerequisites

### Hardware
- Pico 4 or Pico 4 Ultra headset
- USB-C cable for development
- Windows PC (recommended) or Mac

### Software
- Unity 2022.3 LTS
- Android SDK & NDK (via Unity Hub)
- Pico Unity Integration SDK
- JDK 11

---

## Step 1: Development Environment Setup

### 1.1 Install Unity

```bash
# Download Unity Hub from https://unity.com/download
# Install Unity 2022.3 LTS with these modules:
# - Android Build Support
# - Android SDK & NDK Tools
# - OpenJDK
```

### 1.2 Install Pico SDK

1. Download Pico Unity Integration SDK from https://developer.picoxr.com/resources/
2. Or via Unity Package Manager:
   - Window → Package Manager → + → Add package from git URL
   - `https://github.com/picoxr/PicoUnityIntegrationSDK.git`

### 1.3 Install Additional Packages

```
# Via Package Manager, install:
- TextMeshPro (built-in)
- XR Interaction Toolkit
- XR Plugin Management

# Via git URL or .unitypackage:
- GLTFUtility: https://github.com/Siccity/GLTFUtility.git
- Newtonsoft JSON: com.unity.nuget.newtonsoft-json
```

---

## Step 2: Unity Project Setup

### 2.1 Create Project

```bash
# Open Unity Hub
# New Project → 3D (URP) → Name: "DirectorsEye" → Create
```

### 2.2 Import Scripts

1. Copy all files from `unity/Assets/Scripts/` to your project's `Assets/Scripts/`
2. Unity will compile automatically

### 2.3 Configure Build Settings

1. File → Build Settings
2. Switch Platform to **Android**
3. Player Settings:
   ```
   Company Name: DirectorsEye
   Product Name: Director's Eye
   Package Name: com.directorseye.app
   Minimum API Level: Android 10 (API 29)
   Target API Level: Android 12 (API 32)
   Scripting Backend: IL2CPP
   Target Architectures: ARM64
   ```

### 2.4 Configure XR Settings

1. Edit → Project Settings → XR Plug-in Management
2. Enable **Pico** for Android tab
3. Under Pico settings:
   ```
   Stereo Rendering Mode: Multiview
   Display Refresh Rates: 72Hz, 90Hz
   Foveation Level: High
   ```

---

## Step 3: Scene Setup

### 3.1 Create Main Scene

1. Create new scene: `Assets/Scenes/MainScene.unity`
2. Delete default Main Camera

### 3.2 Add XR Rig

```
Hierarchy → Right-click → XR → XR Origin (VR)
```

Configure XR Origin:
- Add `VRInteractionManager.cs` component
- Add `VoiceInputManager.cs` component

### 3.3 Create Manager Objects

```
Create Empty → "GameManager"
  └─ Add: DirectorsEyeApp.cs
  └─ Add: SceneGenerationManager.cs
  └─ Add: ShotCaptureManager.cs

Create Empty → "GeneratedSceneRoot"
  └─ This holds generated environments
```

### 3.4 Create Prefabs

**Actor Prefab:**
```
1. Create → 3D Object → Capsule (body)
2. Add child Sphere (head)
3. Add child Quad (floor mark)
4. Add ActorPlaceholder.cs
5. Add GrabbableObject.cs
6. Add Rigidbody (Is Kinematic = true)
7. Save as Prefab: Assets/Prefabs/ActorPrefab.prefab
```

**Camera Prefab:**
```
1. Create Empty → "VirtualCameraRig"
2. Add child Camera (render to texture)
3. Add child Quad (viewfinder display)
4. Add VirtualCameraRig.cs
5. Add GrabbableObject.cs
6. Create RenderTexture (1920x1080)
7. Save as Prefab: Assets/Prefabs/CameraPrefab.prefab
```

### 3.5 Create VR UI

```
1. Create → UI → Canvas (World Space)
2. Set Render Mode: World Space
3. Scale: 0.001, 0.001, 0.001
4. Position in front of user
5. Add panels:
   - Status panel (top)
   - Tool palette (side)
   - Shot list (toggleable)
6. Add DirectorsEyeUI.cs to Canvas
```

---

## Step 4: Backend Setup

### 4.1 Local Development

```bash
cd backend

# Create virtual environment
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Configure environment
cp .env.example .env
# Edit .env with your API keys:
# MARBLE_API_KEY=your_key_here
# OPENAI_API_KEY=your_key_here

# Run server
uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

### 4.2 Test Backend

```bash
# Health check
curl http://localhost:8000/health

# Test generation (demo mode)
curl -X POST http://localhost:8000/api/generate \
  -H "Content-Type: application/json" \
  -d '{"prompt": "A dark alley at night", "session_id": "test"}'
```

### 4.3 Deploy Backend (for Pico to access)

**Option A: ngrok (Quick testing)**
```bash
ngrok http 8000
# Use the https URL in Unity
```

**Option B: Cloud deployment**
```bash
# Heroku
heroku create directors-eye-api
heroku config:set MARBLE_API_KEY=xxx OPENAI_API_KEY=xxx
git subtree push --prefix backend heroku main

# Railway.app
railway init
railway up

# Render.com
# Connect GitHub repo, set root directory to "backend"
```

---

## Step 5: Connect Unity to Backend

### 5.1 Configure Backend URL

In Unity Inspector, find `DirectorsEyeApp` and set:
```
Backend URL: https://your-deployed-backend.com
# Or for local testing via ngrok:
Backend URL: https://xxxx.ngrok.io
```

### 5.2 Test Connection

1. Enter Play Mode in Unity
2. Check Console for "Backend connected" message
3. Test voice command or manual API call

---

## Step 6: Build and Deploy to Pico

### 6.1 Enable Developer Mode on Pico

1. Settings → General → About → Software Version
2. Tap 7 times to enable Developer Mode
3. Settings → General → Developer → USB Debugging ON

### 6.2 Connect and Build

```bash
# Connect Pico via USB-C
# Allow USB debugging when prompted on headset

# In Unity:
File → Build Settings → Build and Run
# Select APK output location
# Wait for build and auto-install
```

### 6.3 Manual Install (if needed)

```bash
adb install -r DirectorsEye.apk
```

---

## Step 7: Testing Checklist

- [ ] App launches on Pico
- [ ] Controllers tracked correctly
- [ ] Voice recording works (A button)
- [ ] Scene generates from voice command
- [ ] Can place actors (trigger)
- [ ] Can grab and move actors (grip)
- [ ] Can place camera
- [ ] Camera viewfinder shows scene
- [ ] Shot capture works
- [ ] Scene refinement works

---

# Option B: WebXR Browser Version

## Why WebXR?

- Works on Quest, Pico, desktop browsers
- No app store approval needed
- Easy to share demo link
- Lower barrier for judges to try

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | Three.js + WebXR |
| UI | React + React-XR |
| Backend | Same FastAPI server |
| Hosting | Vercel / Netlify |

---

## Step 1: Project Setup

```bash
mkdir directors-eye-web
cd directors-eye-web

# Initialize project
npm create vite@latest . -- --template react-ts
npm install

# Install XR dependencies
npm install three @react-three/fiber @react-three/xr @react-three/drei
npm install zustand  # State management
```

---

## Step 2: Project Structure

```
directors-eye-web/
├── src/
│   ├── components/
│   │   ├── XRScene.tsx        # Main 3D scene
│   │   ├── VoiceInput.tsx     # Voice recording
│   │   ├── ActorModel.tsx     # Placeable actor
│   │   ├── VirtualCamera.tsx  # Camera rig
│   │   ├── UIPanel.tsx        # VR UI
│   │   └── GeneratedEnvironment.tsx
│   ├── hooks/
│   │   ├── useVoiceRecording.ts
│   │   ├── useSceneGeneration.ts
│   │   └── useShotCapture.ts
│   ├── stores/
│   │   └── appStore.ts        # Zustand state
│   ├── api/
│   │   └── backend.ts         # API client
│   ├── App.tsx
│   └── main.tsx
├── public/
└── package.json
```

---

## Step 3: Core Implementation

### 3.1 Main App Entry

```tsx
// src/App.tsx
import { Canvas } from '@react-three/fiber'
import { XR, Controllers, Hands } from '@react-three/xr'
import { XRScene } from './components/XRScene'

export default function App() {
  return (
    <div style={{ width: '100vw', height: '100vh' }}>
      <Canvas>
        <XR>
          <Controllers />
          <Hands />
          <XRScene />
        </XR>
      </Canvas>
    </div>
  )
}
```

### 3.2 XR Scene Component

```tsx
// src/components/XRScene.tsx
import { useXR } from '@react-three/xr'
import { useAppStore } from '../stores/appStore'
import { GeneratedEnvironment } from './GeneratedEnvironment'
import { ActorModel } from './ActorModel'
import { VirtualCamera } from './VirtualCamera'
import { UIPanel } from './UIPanel'

export function XRScene() {
  const { isPresenting } = useXR()
  const { environment, actors, cameras } = useAppStore()

  return (
    <>
      <ambientLight intensity={0.5} />
      <directionalLight position={[10, 10, 5]} />
      
      {environment && <GeneratedEnvironment data={environment} />}
      
      {actors.map((actor) => (
        <ActorModel key={actor.id} {...actor} />
      ))}
      
      {cameras.map((cam) => (
        <VirtualCamera key={cam.id} {...cam} />
      ))}
      
      {isPresenting && <UIPanel />}
    </>
  )
}
```

### 3.3 Voice Recording Hook

```tsx
// src/hooks/useVoiceRecording.ts
import { useState, useCallback } from 'react'
import { transcribeAudio } from '../api/backend'

export function useVoiceRecording() {
  const [isRecording, setIsRecording] = useState(false)
  const [mediaRecorder, setMediaRecorder] = useState<MediaRecorder | null>(null)

  const startRecording = useCallback(async () => {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    const recorder = new MediaRecorder(stream)
    const chunks: Blob[] = []

    recorder.ondataavailable = (e) => chunks.push(e.data)
    recorder.onstop = async () => {
      const blob = new Blob(chunks, { type: 'audio/wav' })
      const text = await transcribeAudio(blob)
      // Handle transcription...
    }

    recorder.start()
    setMediaRecorder(recorder)
    setIsRecording(true)
  }, [])

  const stopRecording = useCallback(() => {
    mediaRecorder?.stop()
    setIsRecording(false)
  }, [mediaRecorder])

  return { isRecording, startRecording, stopRecording }
}
```

### 3.4 API Client

```tsx
// src/api/backend.ts
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000'

export async function generateScene(prompt: string, sessionId: string) {
  const response = await fetch(`${API_URL}/api/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      prompt,
      session_id: sessionId,
      style_name: 'cinematic',
      output_format: 'glb'
    })
  })
  return response.json()
}

export async function transcribeAudio(audioBlob: Blob) {
  const formData = new FormData()
  formData.append('audio', audioBlob, 'recording.wav')
  
  const response = await fetch(`${API_URL}/api/transcribe`, {
    method: 'POST',
    body: formData
  })
  const data = await response.json()
  return data.text
}
```

### 3.5 State Management

```tsx
// src/stores/appStore.ts
import { create } from 'zustand'

interface Actor {
  id: string
  position: [number, number, number]
  rotation: [number, number, number]
  pose: string
}

interface AppState {
  environment: any | null
  actors: Actor[]
  cameras: any[]
  isGenerating: boolean
  
  setEnvironment: (env: any) => void
  addActor: (position: [number, number, number]) => void
  updateActor: (id: string, updates: Partial<Actor>) => void
  addCamera: (position: [number, number, number]) => void
}

export const useAppStore = create<AppState>((set) => ({
  environment: null,
  actors: [],
  cameras: [],
  isGenerating: false,

  setEnvironment: (env) => set({ environment: env }),
  
  addActor: (position) => set((state) => ({
    actors: [...state.actors, {
      id: `actor_${Date.now()}`,
      position,
      rotation: [0, 0, 0],
      pose: 'standing'
    }]
  })),

  updateActor: (id, updates) => set((state) => ({
    actors: state.actors.map((a) => 
      a.id === id ? { ...a, ...updates } : a
    )
  })),

  addCamera: (position) => set((state) => ({
    cameras: [...state.cameras, {
      id: `camera_${Date.now()}`,
      position,
      focalLength: 35
    }]
  }))
}))
```

---

## Step 4: Build and Deploy

### 4.1 Build for Production

```bash
npm run build
```

### 4.2 Deploy to Vercel

```bash
npm install -g vercel
vercel

# Set environment variables in Vercel dashboard:
# VITE_API_URL=https://your-backend.com
```

### 4.3 Deploy to Netlify

```bash
npm install -g netlify-cli
netlify deploy --prod --dir=dist
```

---

## Step 5: Access in VR

1. Open deployed URL in Quest/Pico browser
2. Click "Enter VR" button
3. Allow microphone access when prompted
4. Use controllers to interact

---

# Option C: Desktop App (Electron)

## Quick MVP for Demo

If you need a fast demo without VR hardware:

```bash
# Clone web version
# Add Electron wrapper

npm install electron electron-builder --save-dev
```

```js
// electron/main.js
const { app, BrowserWindow } = require('electron')

function createWindow() {
  const win = new BrowserWindow({
    width: 1920,
    height: 1080,
    webPreferences: {
      nodeIntegration: true
    }
  })
  win.loadURL('http://localhost:5173') // or production build
}

app.whenReady().then(createWindow)
```

Build:
```bash
npm run build
npx electron-builder --win --mac --linux
```

---

# Option D: Hybrid Approach (Recommended for Maximum Impact)

## Architecture

```
                    ┌─────────────────────┐
                    │   Shared Backend    │
                    │   (FastAPI)         │
                    │   - Marble API      │
                    │   - Whisper         │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
       ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
       │  Pico App   │  │  WebXR App  │  │  Desktop    │
       │  (Unity)    │  │  (Three.js) │  │  (Electron) │
       │  Best XP    │  │  Easy Share │  │  Quick Demo │
       └─────────────┘  └─────────────┘  └─────────────┘
```

## Build Order

1. **Day 1 (Hours 0-24):** Build WebXR version first (faster iteration)
2. **Day 2 (Hours 24-40):** Port to Unity/Pico for best experience
3. **Hours 40-48:** Polish, record demos from both platforms

## Benefits

- Web version for judges without VR hardware
- Pico version for immersive demo
- Same backend, shared API
- Redundancy if one platform has issues

---

# Quick Start Commands

## Fastest Path to Demo (WebXR)

```bash
# Terminal 1: Backend
cd backend
python -m venv venv && source venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload

# Terminal 2: Frontend
npx create-vite directors-eye-web --template react-ts
cd directors-eye-web
npm install three @react-three/fiber @react-three/xr @react-three/drei zustand
npm run dev

# Open http://localhost:5173 in browser
# Click "Enter VR" (or use keyboard for desktop testing)
```

## Full VR Experience (Pico)

```bash
# 1. Open Unity Hub → New Project → 3D URP
# 2. Import Pico SDK
# 3. Copy scripts from this repo
# 4. Build → Android → Build and Run
```

---

# Summary: Which Option to Choose?

| Scenario | Recommended Option |
|----------|-------------------|
| Hackathon with Pico provided | **Option A** (Pico Native) |
| Remote judging / easy sharing | **Option B** (WebXR) |
| No VR hardware available | **Option C** (Desktop) |
| Maximum flexibility & backup | **Option D** (Hybrid) |
| Limited time (< 24 hours) | **Option B** (WebXR) |

---

# Next Steps

1. Choose your option above
2. Follow the step-by-step guide
3. Get API keys (World Labs, OpenAI)
4. Start building!

For questions or issues, check the main README.md or open a GitHub issue.
