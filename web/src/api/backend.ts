import type { GenerateRequest, GenerateResponse, TranscriptionResponse } from '../types'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8000'

export async function generateScene(request: GenerateRequest): Promise<GenerateResponse> {
  const response = await fetch(`${API_URL}/api/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      prompt: request.prompt,
      style_name: request.style_name || 'cinematic',
      style_mood: request.style_mood || 'dramatic',
      style_lighting: request.style_lighting || 'natural',
      detail_level: request.detail_level || 0.8,
      output_format: request.output_format || 'glb',
      session_id: request.session_id
    })
  })

  if (!response.ok) {
    throw new Error(`API error: ${response.status}`)
  }

  return response.json()
}

export async function refineScene(
  refinementPrompt: string,
  sessionId: string,
  preserveLayout: boolean = true
): Promise<GenerateResponse> {
  const response = await fetch(`${API_URL}/api/refine`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      refinement_prompt: refinementPrompt,
      preserve_layout: preserveLayout,
      session_id: sessionId
    })
  })

  if (!response.ok) {
    throw new Error(`API error: ${response.status}`)
  }

  return response.json()
}

export async function transcribeAudio(audioBlob: Blob): Promise<TranscriptionResponse> {
  const formData = new FormData()
  formData.append('audio', audioBlob, 'recording.wav')

  const response = await fetch(`${API_URL}/api/transcribe`, {
    method: 'POST',
    body: formData
  })

  if (!response.ok) {
    throw new Error(`Transcription error: ${response.status}`)
  }

  return response.json()
}

export async function healthCheck(): Promise<boolean> {
  try {
    const response = await fetch(`${API_URL}/health`)
    return response.ok
  } catch {
    return false
  }
}
