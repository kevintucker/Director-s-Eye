import { useState } from 'react'
import { useAppStore } from '../../stores/appStore'
import { useVoiceRecording } from '../../hooks/useVoiceRecording'
import { useSceneGeneration } from '../../hooks/useSceneGeneration'
import { useShotCapture } from '../../hooks/useShotCapture'
import type { InteractionMode } from '../../types'

export function ControlPanel() {
  const [textInput, setTextInput] = useState('')
  const [showShotList, setShowShotList] = useState(false)
  
  const { 
    mode, 
    setMode, 
    isGenerating,
    transcription,
    actors,
    cameras,
    shots,
    environment
  } = useAppStore()
  
  const { isRecording, isProcessing, toggleRecording } = useVoiceRecording()
  const { parseAndExecuteCommand } = useSceneGeneration()
  const { captureShot, exportShots, shotCount } = useShotCapture()

  const handleVoiceClick = async () => {
    if (isRecording) {
      const text = await toggleRecording()
      if (text) {
        await parseAndExecuteCommand(text)
      }
    } else {
      await toggleRecording()
    }
  }

  const handleTextSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (textInput.trim()) {
      await parseAndExecuteCommand(textInput.trim())
      setTextInput('')
    }
  }

  const modeButtons: { mode: InteractionMode; label: string; icon: string }[] = [
    { mode: 'pointer', label: 'Pointer', icon: '👆' },
    { mode: 'placeActor', label: 'Actor', icon: '🎭' },
    { mode: 'placeCamera', label: 'Camera', icon: '📷' },
    { mode: 'grab', label: 'Grab', icon: '✊' }
  ]

  return (
    <div className="control-panel">
      <div className="status-bar">
        <div className="status-item">
          <span className="status-label">Scene:</span>
          <span className={`status-value ${environment ? 'active' : ''}`}>
            {environment ? 'Generated' : 'Empty'}
          </span>
        </div>
        <div className="status-item">
          <span className="status-label">Actors:</span>
          <span className="status-value">{actors.length}</span>
        </div>
        <div className="status-item">
          <span className="status-label">Cameras:</span>
          <span className="status-value">{cameras.length}</span>
        </div>
        <div className="status-item">
          <span className="status-label">Shots:</span>
          <span className="status-value">{shotCount}</span>
        </div>
      </div>

      <div className="voice-section">
        <button 
          className={`voice-btn ${isRecording ? 'recording' : ''} ${isProcessing ? 'processing' : ''}`}
          onClick={handleVoiceClick}
          disabled={isProcessing || isGenerating}
        >
          {isRecording ? '🔴 Stop Recording' : isProcessing ? '⏳ Processing...' : '🎤 Voice Command'}
        </button>
        
        {transcription && (
          <div className="transcription">
            "{transcription}"
          </div>
        )}
        
        {isGenerating && (
          <div className="generating">
            ✨ Generating scene...
          </div>
        )}
      </div>

      <form className="text-input-form" onSubmit={handleTextSubmit}>
        <input
          type="text"
          value={textInput}
          onChange={(e) => setTextInput(e.target.value)}
          placeholder="Describe your scene..."
          disabled={isGenerating}
        />
        <button type="submit" disabled={isGenerating || !textInput.trim()}>
          Go
        </button>
      </form>

      <div className="mode-section">
        <div className="mode-label">Mode:</div>
        <div className="mode-buttons">
          {modeButtons.map(({ mode: m, label, icon }) => (
            <button
              key={m}
              className={`mode-btn ${mode === m ? 'active' : ''}`}
              onClick={() => setMode(m)}
            >
              <span className="mode-icon">{icon}</span>
              <span className="mode-text">{label}</span>
            </button>
          ))}
        </div>
      </div>

      <div className="shot-section">
        <button 
          className="capture-btn"
          onClick={() => captureShot()}
          disabled={cameras.length === 0}
        >
          📸 Capture
        </button>
        <button 
          className="shots-btn"
          onClick={() => setShowShotList(!showShotList)}
        >
          📋 ({shotCount})
        </button>
        <button 
          className="export-btn"
          onClick={() => exportShots('json')}
          disabled={shotCount === 0}
        >
          💾 Export
        </button>
      </div>

      {showShotList && (
        <div className="shot-list">
          <div className="shot-list-header">
            <h3>Shot List</h3>
            <button onClick={() => setShowShotList(false)}>✕</button>
          </div>
          <div className="shot-list-content">
            {shots.length === 0 ? (
              <p className="empty-message">No shots yet</p>
            ) : (
              shots.map((shot) => (
                <div key={shot.id} className="shot-item">
                  <div className="shot-thumbnail">
                    {shot.imageDataUrl ? (
                      <img src={shot.imageDataUrl} alt={`Shot ${shot.number}`} />
                    ) : (
                      <div className="no-thumbnail">📷</div>
                    )}
                  </div>
                  <div className="shot-info">
                    <div className="shot-number">Shot {shot.number}</div>
                    <div className="shot-details">{shot.focalLength}mm</div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}

      <div className="instructions">
        <p><strong>Quick Start:</strong></p>
        <ul>
          <li>🎤 Voice or type scene description</li>
          <li>🎭 Actor mode → click floor</li>
          <li>📷 Camera mode → click to place</li>
          <li>📸 Capture shots</li>
        </ul>
      </div>

      <style>{`
        .control-panel {
          position: fixed;
          left: 20px;
          top: 20px;
          width: 300px;
          background: rgba(15, 23, 42, 0.95);
          border: 1px solid rgba(99, 102, 241, 0.3);
          border-radius: 16px;
          padding: 16px;
          color: white;
          font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
          max-height: calc(100vh - 40px);
          overflow-y: auto;
          backdrop-filter: blur(10px);
          font-size: 13px;
        }
        .status-bar {
          display: flex;
          flex-wrap: wrap;
          gap: 10px;
          margin-bottom: 12px;
          padding-bottom: 12px;
          border-bottom: 1px solid rgba(255,255,255,0.1);
        }
        .status-item { display: flex; gap: 4px; }
        .status-label { color: #94a3b8; }
        .status-value { color: #e2e8f0; font-weight: 600; }
        .status-value.active { color: #22c55e; }
        .voice-section { margin-bottom: 12px; }
        .voice-btn {
          width: 100%;
          padding: 12px;
          font-size: 14px;
          font-weight: 600;
          border: none;
          border-radius: 10px;
          cursor: pointer;
          background: linear-gradient(135deg, #6366f1, #8b5cf6);
          color: white;
        }
        .voice-btn:hover { transform: translateY(-1px); }
        .voice-btn.recording {
          background: linear-gradient(135deg, #ef4444, #dc2626);
          animation: pulse 1s infinite;
        }
        .voice-btn.processing { background: #475569; cursor: wait; }
        .voice-btn:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }
        @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.7; } }
        .transcription {
          margin-top: 8px;
          padding: 10px;
          background: rgba(99, 102, 241, 0.1);
          border-radius: 6px;
          font-style: italic;
          color: #c4b5fd;
        }
        .generating {
          margin-top: 8px;
          padding: 10px;
          background: rgba(34, 197, 94, 0.1);
          border-radius: 6px;
          color: #86efac;
          text-align: center;
        }
        .text-input-form {
          display: flex;
          gap: 6px;
          margin-bottom: 12px;
        }
        .text-input-form input {
          flex: 1;
          padding: 8px 10px;
          border: 1px solid rgba(255,255,255,0.2);
          border-radius: 6px;
          background: rgba(255,255,255,0.05);
          color: white;
          font-size: 13px;
        }
        .text-input-form input::placeholder { color: #64748b; }
        .text-input-form button {
          padding: 8px 14px;
          background: #6366f1;
          border: none;
          border-radius: 6px;
          color: white;
          font-weight: 600;
          cursor: pointer;
        }
        .text-input-form button:disabled { opacity: 0.5; }
        .mode-section { margin-bottom: 12px; }
        .mode-label { font-size: 12px; color: #94a3b8; margin-bottom: 6px; }
        .mode-buttons { display: grid; grid-template-columns: repeat(4, 1fr); gap: 6px; }
        .mode-btn {
          display: flex;
          flex-direction: column;
          align-items: center;
          padding: 8px 4px;
          background: rgba(255,255,255,0.05);
          border: 1px solid rgba(255,255,255,0.1);
          border-radius: 8px;
          color: #94a3b8;
          cursor: pointer;
        }
        .mode-btn:hover { background: rgba(255,255,255,0.1); color: white; }
        .mode-btn.active { background: rgba(99, 102, 241, 0.2); border-color: #6366f1; color: white; }
        .mode-icon { font-size: 18px; margin-bottom: 2px; }
        .mode-text { font-size: 10px; }
        .shot-section { display: flex; gap: 6px; margin-bottom: 12px; }
        .capture-btn, .shots-btn, .export-btn {
          flex: 1;
          padding: 8px;
          border: none;
          border-radius: 6px;
          font-size: 11px;
          font-weight: 600;
          cursor: pointer;
        }
        .capture-btn { background: #22c55e; color: white; }
        .shots-btn { background: rgba(255,255,255,0.1); color: white; }
        .export-btn { background: #3b82f6; color: white; }
        .capture-btn:disabled, .export-btn:disabled { opacity: 0.5; cursor: not-allowed; }
        .shot-list {
          background: rgba(0,0,0,0.3);
          border-radius: 10px;
          padding: 12px;
          margin-bottom: 12px;
        }
        .shot-list-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
        }
        .shot-list-header h3 { margin: 0; font-size: 13px; }
        .shot-list-header button { background: none; border: none; color: #94a3b8; cursor: pointer; }
        .shot-list-content { max-height: 150px; overflow-y: auto; }
        .empty-message { color: #64748b; text-align: center; }
        .shot-item {
          display: flex;
          gap: 10px;
          padding: 6px;
          background: rgba(255,255,255,0.05);
          border-radius: 6px;
          margin-bottom: 6px;
        }
        .shot-thumbnail {
          width: 50px;
          height: 35px;
          background: #1e293b;
          border-radius: 4px;
          overflow: hidden;
          display: flex;
          align-items: center;
          justify-content: center;
        }
        .shot-thumbnail img { width: 100%; height: 100%; object-fit: cover; }
        .no-thumbnail { font-size: 16px; opacity: 0.5; }
        .shot-info { flex: 1; }
        .shot-number { font-weight: 600; font-size: 12px; }
        .shot-details { font-size: 10px; color: #94a3b8; }
        .instructions {
          padding: 10px;
          background: rgba(255,255,255,0.03);
          border-radius: 6px;
        }
        .instructions p { margin: 0 0 6px 0; color: #94a3b8; font-size: 11px; }
        .instructions ul { margin: 0; padding-left: 14px; }
        .instructions li { color: #64748b; margin-bottom: 2px; font-size: 11px; }
      `}</style>
    </div>
  )
}
