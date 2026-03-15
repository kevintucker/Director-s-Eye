import { useCallback } from 'react'
import { generateScene, refineScene } from '../api/backend'
import { useAppStore } from '../stores/appStore'
import type { SceneData } from '../types'

export function useSceneGeneration() {
  const {
    sessionId,
    isGenerating,
    setIsGenerating,
    setEnvironment,
    setCurrentPrompt,
    environment
  } = useAppStore()

  const generate = useCallback(async (prompt: string) => {
    if (isGenerating) return

    setIsGenerating(true)
    setCurrentPrompt(prompt)

    try {
      const response = await generateScene({
        prompt,
        session_id: sessionId,
        style_name: 'cinematic',
        style_mood: 'dramatic',
        style_lighting: 'natural'
      })

      if (response.success) {
        const sceneData: SceneData = {
          meshData: response.scene_data_base64 || null,
          textureData: response.texture_data_base64 || null,
          metadata: response.metadata ? JSON.parse(response.metadata) : {}
        }
        setEnvironment(sceneData)
        return sceneData
      } else {
        throw new Error(response.error || 'Generation failed')
      }
    } catch (error) {
      console.error('Scene generation error:', error)
      throw error
    } finally {
      setIsGenerating(false)
    }
  }, [sessionId, isGenerating, setIsGenerating, setEnvironment, setCurrentPrompt])

  const refine = useCallback(async (refinementPrompt: string) => {
    if (isGenerating || !environment) return

    setIsGenerating(true)

    try {
      const response = await refineScene(refinementPrompt, sessionId, true)

      if (response.success) {
        const sceneData: SceneData = {
          meshData: response.scene_data_base64 || null,
          textureData: response.texture_data_base64 || null,
          metadata: response.metadata ? JSON.parse(response.metadata) : {}
        }
        setEnvironment(sceneData)
        return sceneData
      } else {
        throw new Error(response.error || 'Refinement failed')
      }
    } catch (error) {
      console.error('Scene refinement error:', error)
      throw error
    } finally {
      setIsGenerating(false)
    }
  }, [sessionId, isGenerating, environment, setIsGenerating, setEnvironment])

  const parseAndExecuteCommand = useCallback(async (text: string) => {
    const lower = text.toLowerCase().trim()

    // Check if this is a refinement command
    const refinementKeywords = ['make it', 'add', 'change', 'more', 'less', 'remove', 'darker', 'lighter', 'warmer', 'cooler']
    const isRefinement = environment && refinementKeywords.some(kw => lower.startsWith(kw))

    if (isRefinement) {
      return refine(text)
    } else {
      return generate(text)
    }
  }, [environment, generate, refine])

  return {
    generate,
    refine,
    parseAndExecuteCommand,
    isGenerating
  }
}
