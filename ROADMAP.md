# Director's Eye - Development Roadmap

## Project Timeline: 48-Hour Hackathon

---

## Phase 0: Pre-Hackathon Preparation (Before Event)
*Allowed: Mockups, design, API key acquisition - NO coding*

### Tasks
- [ ] Register for World Labs Marble API access
- [ ] Get OpenAI API key for Whisper
- [ ] Download and install Pico SDK
- [ ] Set up Unity 2022 LTS with Android build support
- [ ] Create Figma mockups for VR UI
- [ ] Storyboard the demo video flow
- [ ] Prepare pitch deck outline

### Deliverables
- API keys ready
- Development environment configured
- UI/UX mockups complete
- Demo script drafted

---

## Phase 1: Foundation (Hours 0-6)

### 1.1 Project Setup (Hours 0-2)
- [ ] Create Unity project with Pico SDK
- [ ] Configure Android build settings for Pico 4
- [ ] Import required packages:
  - Pico XR SDK
  - GLTFUtility (for mesh loading)
  - TextMeshPro
  - Newtonsoft JSON
- [ ] Set up basic VR scene with:
  - XR Origin (Pico)
  - Hand/controller models
  - Basic floor plane
- [ ] Create folder structure and assembly definitions

### 1.2 Backend Setup (Hours 2-4)
- [ ] Initialize Python virtual environment
- [ ] Install FastAPI dependencies
- [ ] Configure environment variables
- [ ] Test Marble API connection (or confirm demo mode works)
- [ ] Test Whisper API connection
- [ ] Deploy backend locally
- [ ] Verify Unity ↔ Backend communication

### 1.3 Core Architecture (Hours 4-6)
- [ ] Implement singleton managers in Unity
- [ ] Set up event system for component communication
- [ ] Create basic state machine for app flow
- [ ] Test basic VR interaction (grab, point, trigger)

### Milestone 1 Checklist
```
✓ VR app launches on Pico
✓ Backend server running
✓ API calls working (or demo mode)
✓ Basic VR controls functional
```

---

## Phase 2: Scene Generation (Hours 6-16)

### 2.1 Voice Input System (Hours 6-9)
- [ ] Implement microphone capture in Unity
- [ ] Add WAV encoding for audio data
- [ ] Create voice recording UI indicator
- [ ] Implement silence detection for auto-stop
- [ ] Send audio to backend Whisper endpoint
- [ ] Display transcription feedback to user
- [ ] Handle errors gracefully

### 2.2 Marble API Integration (Hours 9-13)
- [ ] Finalize prompt engineering for cinematic scenes
- [ ] Implement async generation request
- [ ] Handle polling for async results (if needed)
- [ ] Receive and decode mesh data
- [ ] Receive and apply textures
- [ ] Optimize assets for VR performance

### 2.3 Scene Loading (Hours 13-16)
- [ ] Implement GLB/GLTF mesh loading in Unity
- [ ] Apply materials and textures
- [ ] Generate mesh colliders for interaction
- [ ] Position user at appropriate spawn point
- [ ] Add loading indicator/animation
- [ ] Implement scene clearing for new generations

### Milestone 2 Checklist
```
✓ Voice command triggers scene generation
✓ 3D environment appears around user
✓ User can walk through generated scene
✓ Scene has proper collisions
```

---

## Phase 3: Spatial Direction Tools (Hours 16-28)

### 3.1 Actor System (Hours 16-19)
- [ ] Create actor mannequin prefab
  - Basic humanoid mesh
  - Color-coded materials
  - Floor mark indicator
  - Label display
- [ ] Implement actor spawning at raycast point
- [ ] Add grab and reposition functionality
- [ ] Implement pose presets (standing, sitting, etc.)
- [ ] Add actor numbering system
- [ ] Make actors look at camera on command

### 3.2 Virtual Camera System (Hours 19-23)
- [ ] Create camera rig prefab
  - Virtual camera with render texture
  - Viewfinder display mesh
  - Focal length indicator
  - Frame guides overlay
- [ ] Implement focal length adjustment (16mm - 200mm)
- [ ] Add aspect ratio presets (16:9, 2.39:1, etc.)
- [ ] Create camera frustum visualization
- [ ] Implement camera grab and positioning
- [ ] Add tripod/dolly snap points (optional)

### 3.3 Shot Capture System (Hours 23-26)
- [ ] Capture render texture to PNG
- [ ] Save frame to device storage
- [ ] Store camera metadata (position, focal length, etc.)
- [ ] Store actor positions and poses
- [ ] Generate shot ID and timestamp
- [ ] Display capture feedback

### 3.4 Scene Refinement (Hours 26-28)
- [ ] Implement refinement voice commands
- [ ] Parse refinement intent vs new scene
- [ ] Send refinement request to Marble API
- [ ] Update scene while preserving actor/camera positions
- [ ] Handle refinement failures gracefully

### Milestone 3 Checklist
```
✓ Spawn and position actors
✓ Place and adjust virtual cameras
✓ See through virtual camera viewfinder
✓ Capture and save shots
✓ Refine scene with voice ("add fog", "sunset lighting")
```

---

## Phase 4: Polish & Export (Hours 28-40)

### 4.1 User Interface (Hours 28-32)
- [ ] Create floating VR menu panel
- [ ] Add tool selection buttons (Actor, Camera, Marker)
- [ ] Display current mode indicator
- [ ] Show shot count and thumbnails
- [ ] Add voice recording indicator
- [ ] Create loading/generating overlay
- [ ] Implement settings panel (backend URL, etc.)

### 4.2 Shot List Management (Hours 32-35)
- [ ] Create shot list panel UI
- [ ] Display shot thumbnails in grid
- [ ] Allow shot selection and preview
- [ ] Implement shot deletion
- [ ] Add shot reordering (optional)

### 4.3 Export System (Hours 35-38)
- [ ] Export shot list as JSON
- [ ] Export shot list as CSV
- [ ] Generate PDF report with thumbnails (optional)
- [ ] Create ZIP archive of all frames
- [ ] Add export confirmation UI

### 4.4 Polish & Bug Fixes (Hours 38-40)
- [ ] Performance optimization
- [ ] Memory leak fixes
- [ ] Error handling improvements
- [ ] Visual polish (materials, lighting)
- [ ] Sound effects (optional)
- [ ] Haptic feedback for interactions

### Milestone 4 Checklist
```
✓ Intuitive VR UI
✓ Shot list viewable in VR
✓ Export functionality working
✓ No major bugs
✓ Smooth 72fps performance
```

---

## Phase 5: Demo & Submission (Hours 40-48)

### 5.1 Demo Video (Hours 40-44)
- [ ] Write demo script (see below)
- [ ] Set up screen recording (Pico mirror + OBS)
- [ ] Record demo takes
- [ ] Edit video (3 minutes max)
- [ ] Add captions/annotations
- [ ] Add intro/outro with branding
- [ ] Export in required format

### 5.2 Presentation Materials (Hours 44-46)
- [ ] Create pitch deck (5-7 slides)
  - Problem statement
  - Solution overview
  - Demo highlights
  - Technical architecture
  - Future vision
  - Team
- [ ] Write project description (500 words)
- [ ] Prepare live demo talking points
- [ ] Test live demo flow

### 5.3 Submission (Hours 46-48)
- [ ] Final build for Pico
- [ ] Upload to hackathon platform
- [ ] Submit GitHub repository
- [ ] Submit demo video
- [ ] Submit pitch deck
- [ ] Complete submission form
- [ ] Verify all materials accessible

### Milestone 5 Checklist
```
✓ Demo video complete (3 min)
✓ Pitch deck ready
✓ Project description written
✓ All files submitted
✓ Live demo tested
```

---

## Demo Script (3 Minutes)

| Time | Visual | Audio/Narration |
|------|--------|-----------------|
| 0:00-0:15 | Problem montage: expensive previs, 2D storyboards | "Pre-visualization costs $10K-$100K. Indie filmmakers can't afford it. And flat storyboards can't capture spatial relationships." |
| 0:15-0:25 | Director's Eye logo, put on headset | "Director's Eye changes that. Describe your vision, step inside it, direct from within." |
| 0:25-0:45 | Empty void, press button, speak | "Let's create a scene." *speaks* "A rooftop at night, neon city below, rain falling." |
| 0:45-1:15 | Environment generates around user | "World Labs Marble API generates the environment in real-time. I'm standing inside my scene." |
| 1:15-1:35 | Place two actors, adjust positions | "I'll block my actors. Place them facing each other. Grab to reposition." |
| 1:35-1:55 | Place camera, adjust focal length | "Now my camera. 50mm lens, slight low angle for tension." |
| 1:55-2:10 | Look through viewfinder, capture shot | "Check the frame in the viewfinder. Capture." |
| 2:10-2:25 | Speak refinement | "Add more fog, make the neon pink." *scene updates* |
| 2:25-2:40 | Show shot list, export | "My shot list saves automatically. Export to share with my crew." |
| 2:40-3:00 | Return to logo, team credits | "Director's Eye. Pre-visualization for everyone. Built with World Labs Marble API and Pico VR." |

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Marble API slow/unavailable | Demo mode with pre-generated scenes |
| Mesh loading issues | Fallback to primitive geometry |
| Voice recognition errors | Manual text input option |
| Performance issues | Reduce texture resolution, simplify meshes |
| Time overrun | Cut features in priority order (export → refinement → multiple cameras) |

---

## Feature Priority (If Time-Constrained)

### Must Have (MVP)
1. Voice-to-scene generation
2. Place 1 actor
3. Place 1 camera with viewfinder
4. Capture 1 shot

### Should Have
5. Multiple actors
6. Scene refinement
7. Shot list view
8. JSON export

### Nice to Have
9. PDF export
10. Actor poses
11. Multiple camera rigs
12. Aspect ratio presets

---

## Post-Hackathon Roadmap

### Version 1.1 (Month 1)
- Multi-user collaboration
- Actor animation library
- Camera movement paths (dolly, crane)
- Cloud save/sync

### Version 1.2 (Month 2)
- Export to Unreal Sequencer
- Export to professional previs tools
- AI actor performance suggestions
- Lighting control

### Version 2.0 (Month 3-6)
- Real-time collaboration (multiple directors)
- Integration with production databases
- AR mode for location scouting
- Mobile companion app

---

## Team Allocation (3-Person Team)

| Role | Primary Focus | Secondary |
|------|--------------|-----------|
| **Dev 1 (Unity/XR)** | VR interaction, Pico SDK, scene loading | UI implementation |
| **Dev 2 (Backend)** | Marble API, Whisper, FastAPI | Unity networking |
| **Dev 3 (Design/Demo)** | UI/UX, demo video, pitch deck | Actor/camera prefabs |

---

## Success Metrics

- [ ] Scene generates in < 30 seconds
- [ ] App runs at 72fps on Pico 4
- [ ] Voice commands recognized > 90% accuracy
- [ ] Demo video clearly shows all features
- [ ] Judges understand value proposition
