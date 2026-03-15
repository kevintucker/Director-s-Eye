import { useCallback } from 'react'
import { XRScene } from './components/XRScene'
import { ControlPanel } from './components/UI/ControlPanel'
import { VRButton } from './components/UI/VRButton'
import { useShotCapture } from './hooks/useShotCapture'
import './App.css'

function App() {
  const { setCanvasRef } = useShotCapture()

  const handleCanvasReady = useCallback((canvas: HTMLCanvasElement) => {
    setCanvasRef(canvas)
  }, [setCanvasRef])

  return (
    <div className="app">
      <header className="header">
        <h1>🎬 Director's Eye</h1>
        <p>Describe your vision. Step inside it. Direct from within.</p>
      </header>
      
      <main className="main">
        <XRScene onCanvasReady={handleCanvasReady} />
        <ControlPanel />
        <VRButton />
      </main>
    </div>
  )
}

export default App
