export interface Actor {
  id: string
  position: [number, number, number]
  rotation: [number, number, number]
  pose: ActorPose
  color: string
  label: string
}

export type ActorPose = 'standing' | 'sitting' | 'crouching' | 'walking'

export interface VirtualCamera {
  id: string
  position: [number, number, number]
  rotation: [number, number, number]
  focalLength: number
  aspectRatio: AspectRatio
}

export type AspectRatio = '16:9' | '2.39:1' | '1.85:1' | '1:1'

export interface Shot {
  id: string
  number: number
  timestamp: string
  cameraPosition: [number, number, number]
  cameraRotation: [number, number, number]
  focalLength: number
  aspectRatio: string
  actors: Actor[]
  imageDataUrl?: string
}

export interface SceneData {
  meshData: string | null
  textureData: string | null
  metadata: Record<string, unknown>
}

export interface GenerateRequest {
  prompt: string
  style_name?: string
  style_mood?: string
  style_lighting?: string
  detail_level?: number
  output_format?: string
  session_id: string
}

export interface GenerateResponse {
  success: boolean
  error?: string
  scene_data_base64?: string
  texture_data_base64?: string
  metadata?: string
}

export interface TranscriptionResponse {
  text: string
  confidence: number
}

export type InteractionMode = 'pointer' | 'placeActor' | 'placeCamera' | 'grab'

export interface AppState {
  environment: SceneData | null
  isGenerating: boolean
  currentPrompt: string
  sessionId: string

  actors: Actor[]
  cameras: VirtualCamera[]
  activeCamera: string | null

  shots: Shot[]
  
  mode: InteractionMode
  isRecording: boolean
  transcription: string

  setEnvironment: (env: SceneData | null) => void
  setIsGenerating: (val: boolean) => void
  setCurrentPrompt: (prompt: string) => void
  
  addActor: (position: [number, number, number]) => void
  updateActor: (id: string, updates: Partial<Actor>) => void
  removeActor: (id: string) => void
  
  addCamera: (position: [number, number, number]) => void
  updateCamera: (id: string, updates: Partial<VirtualCamera>) => void
  removeCamera: (id: string) => void
  setActiveCamera: (id: string | null) => void
  
  addShot: (shot: Shot) => void
  removeShot: (id: string) => void
  clearShots: () => void
  
  setMode: (mode: InteractionMode) => void
  setIsRecording: (val: boolean) => void
  setTranscription: (text: string) => void
  
  reset: () => void
}
