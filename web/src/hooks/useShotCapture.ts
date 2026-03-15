import { useCallback, useRef } from 'react'
import { v4 as uuidv4 } from 'uuid'
import { useAppStore } from '../stores/appStore'
import type { Shot } from '../types'

export function useShotCapture() {
  const { actors, cameras, activeCamera, shots, addShot } = useAppStore()
  const canvasRef = useRef<HTMLCanvasElement | null>(null)

  const setCanvasRef = useCallback((canvas: HTMLCanvasElement | null) => {
    canvasRef.current = canvas
  }, [])

  const captureShot = useCallback(() => {
    const camera = cameras.find(c => c.id === activeCamera)
    if (!camera) {
      console.warn('No active camera to capture shot')
      return null
    }

    let imageDataUrl: string | undefined
    if (canvasRef.current) {
      try {
        imageDataUrl = canvasRef.current.toDataURL('image/png')
      } catch (err) {
        console.error('Failed to capture canvas:', err)
      }
    }

    const shot: Shot = {
      id: uuidv4(),
      number: shots.length + 1,
      timestamp: new Date().toISOString(),
      cameraPosition: camera.position,
      cameraRotation: camera.rotation,
      focalLength: camera.focalLength,
      aspectRatio: camera.aspectRatio,
      actors: [...actors],
      imageDataUrl
    }

    addShot(shot)
    console.log(`Captured shot #${shot.number}`)
    
    return shot
  }, [cameras, activeCamera, actors, shots, addShot])

  const exportShots = useCallback((format: 'json' | 'csv' = 'json') => {
    if (shots.length === 0) {
      console.warn('No shots to export')
      return
    }

    let content: string
    let filename: string
    let mimeType: string

    if (format === 'json') {
      const exportData = {
        projectName: 'Director\'s Eye Project',
        exportDate: new Date().toISOString(),
        totalShots: shots.length,
        shots: shots.map(shot => ({
          ...shot,
          imageDataUrl: undefined
        }))
      }
      content = JSON.stringify(exportData, null, 2)
      filename = 'directors-eye-shots.json'
      mimeType = 'application/json'
    } else {
      const headers = ['Shot', 'Timestamp', 'Focal Length', 'Aspect Ratio', 'Camera X', 'Camera Y', 'Camera Z', 'Actors']
      const rows = shots.map(shot => [
        shot.number,
        shot.timestamp,
        `${shot.focalLength}mm`,
        shot.aspectRatio,
        shot.cameraPosition[0].toFixed(2),
        shot.cameraPosition[1].toFixed(2),
        shot.cameraPosition[2].toFixed(2),
        shot.actors.map(a => a.label).join('; ')
      ])
      content = [headers.join(','), ...rows.map(r => r.join(','))].join('\n')
      filename = 'directors-eye-shots.csv'
      mimeType = 'text/csv'
    }

    const blob = new Blob([content], { type: mimeType })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }, [shots])

  return {
    captureShot,
    exportShots,
    setCanvasRef,
    shotCount: shots.length
  }
}
