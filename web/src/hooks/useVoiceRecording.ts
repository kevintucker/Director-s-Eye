import { useState, useRef, useCallback } from 'react'
import { transcribeAudio } from '../api/backend'
import { useAppStore } from '../stores/appStore'

export function useVoiceRecording() {
  const [isRecording, setIsRecording] = useState(false)
  const [isProcessing, setIsProcessing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  
  const mediaRecorderRef = useRef<MediaRecorder | null>(null)
  const chunksRef = useRef<Blob[]>([])
  
  const { setTranscription, setIsRecording: setStoreRecording } = useAppStore()

  const startRecording = useCallback(async () => {
    try {
      setError(null)
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      
      const mediaRecorder = new MediaRecorder(stream, {
        mimeType: 'audio/webm'
      })
      
      chunksRef.current = []
      
      mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          chunksRef.current.push(event.data)
        }
      }
      
      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(chunksRef.current, { type: 'audio/webm' })
        
        // Stop all tracks
        stream.getTracks().forEach(track => track.stop())
        
        // Process transcription
        setIsProcessing(true)
        try {
          const result = await transcribeAudio(audioBlob)
          setTranscription(result.text)
          return result.text
        } catch (err) {
          setError('Failed to transcribe audio')
          console.error('Transcription error:', err)
        } finally {
          setIsProcessing(false)
        }
      }
      
      mediaRecorderRef.current = mediaRecorder
      mediaRecorder.start(100) // Collect data every 100ms
      setIsRecording(true)
      setStoreRecording(true)
      
    } catch (err) {
      setError('Failed to access microphone')
      console.error('Microphone error:', err)
    }
  }, [setTranscription, setStoreRecording])

  const stopRecording = useCallback((): Promise<string> => {
    return new Promise((resolve) => {
      if (mediaRecorderRef.current && isRecording) {
        const recorder = mediaRecorderRef.current
        
        recorder.onstop = async () => {
          const audioBlob = new Blob(chunksRef.current, { type: 'audio/webm' })
          
          setIsProcessing(true)
          try {
            const result = await transcribeAudio(audioBlob)
            setTranscription(result.text)
            resolve(result.text)
          } catch (err) {
            setError('Failed to transcribe audio')
            console.error('Transcription error:', err)
            resolve('')
          } finally {
            setIsProcessing(false)
          }
        }
        
        recorder.stop()
        setIsRecording(false)
        setStoreRecording(false)
      } else {
        resolve('')
      }
    })
  }, [isRecording, setTranscription, setStoreRecording])

  const toggleRecording = useCallback(async () => {
    if (isRecording) {
      return stopRecording()
    } else {
      await startRecording()
      return ''
    }
  }, [isRecording, startRecording, stopRecording])

  return {
    isRecording,
    isProcessing,
    error,
    startRecording,
    stopRecording,
    toggleRecording
  }
}
