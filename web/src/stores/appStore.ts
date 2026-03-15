import { create } from 'zustand'
import { v4 as uuidv4 } from 'uuid'
import type { AppState, Actor, VirtualCamera, Shot, SceneData } from '../types'

const ACTOR_COLORS = ['#ef4444', '#3b82f6', '#22c55e', '#eab308', '#a855f7', '#ec4899']

export const useAppStore = create<AppState>((set, get) => ({
  environment: null,
  isGenerating: false,
  currentPrompt: '',
  sessionId: uuidv4(),

  actors: [],
  cameras: [],
  activeCamera: null,

  shots: [],

  mode: 'pointer',
  isRecording: false,
  transcription: '',

  setEnvironment: (env: SceneData | null) => set({ environment: env }),
  setIsGenerating: (val: boolean) => set({ isGenerating: val }),
  setCurrentPrompt: (prompt: string) => set({ currentPrompt: prompt }),

  addActor: (position: [number, number, number]) => {
    const actors = get().actors
    const newActor: Actor = {
      id: uuidv4(),
      position,
      rotation: [0, 0, 0],
      pose: 'standing',
      color: ACTOR_COLORS[actors.length % ACTOR_COLORS.length],
      label: `Actor ${actors.length + 1}`
    }
    set({ actors: [...actors, newActor] })
  },

  updateActor: (id: string, updates: Partial<Actor>) => {
    set({
      actors: get().actors.map((actor) =>
        actor.id === id ? { ...actor, ...updates } : actor
      )
    })
  },

  removeActor: (id: string) => {
    set({ actors: get().actors.filter((actor) => actor.id !== id) })
  },

  addCamera: (position: [number, number, number]) => {
    const cameras = get().cameras
    const newCamera: VirtualCamera = {
      id: uuidv4(),
      position,
      rotation: [0, 0, 0],
      focalLength: 35,
      aspectRatio: '16:9'
    }
    set({ 
      cameras: [...cameras, newCamera],
      activeCamera: newCamera.id
    })
  },

  updateCamera: (id: string, updates: Partial<VirtualCamera>) => {
    set({
      cameras: get().cameras.map((camera) =>
        camera.id === id ? { ...camera, ...updates } : camera
      )
    })
  },

  removeCamera: (id: string) => {
    const state = get()
    set({ 
      cameras: state.cameras.filter((camera) => camera.id !== id),
      activeCamera: state.activeCamera === id ? null : state.activeCamera
    })
  },

  setActiveCamera: (id: string | null) => set({ activeCamera: id }),

  addShot: (shot: Shot) => {
    set({ shots: [...get().shots, shot] })
  },

  removeShot: (id: string) => {
    set({ shots: get().shots.filter((shot) => shot.id !== id) })
  },

  clearShots: () => set({ shots: [] }),

  setMode: (mode) => set({ mode }),
  setIsRecording: (val: boolean) => set({ isRecording: val }),
  setTranscription: (text: string) => set({ transcription: text }),

  reset: () => set({
    environment: null,
    isGenerating: false,
    currentPrompt: '',
    actors: [],
    cameras: [],
    activeCamera: null,
    shots: [],
    mode: 'pointer',
    isRecording: false,
    transcription: ''
  })
}))
