import { useState, useEffect } from 'react'

export function VRButton() {
  const [isVRSupported, setIsVRSupported] = useState(false)

  useEffect(() => {
    if ('xr' in navigator) {
      (navigator as any).xr?.isSessionSupported('immersive-vr').then((supported: boolean) => {
        setIsVRSupported(supported)
      })
    }
  }, [])

  if (!isVRSupported) {
    return (
      <div className="vr-button-container">
        <div className="vr-not-supported">
          VR not available - Desktop mode active
        </div>
        <style>{`
          .vr-button-container {
            position: fixed;
            bottom: 20px;
            right: 20px;
            z-index: 100;
          }
          .vr-not-supported {
            padding: 12px 20px;
            background: rgba(15, 23, 42, 0.9);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            color: #94a3b8;
            font-size: 13px;
          }
        `}</style>
      </div>
    )
  }

  return (
    <div className="vr-button-container">
      <button className="vr-button">
        🥽 Enter VR
      </button>
      <style>{`
        .vr-button-container {
          position: fixed;
          bottom: 20px;
          right: 20px;
          z-index: 100;
        }
        .vr-button {
          padding: 16px 32px;
          font-size: 18px;
          font-weight: 700;
          background: linear-gradient(135deg, #6366f1, #8b5cf6);
          border: none;
          border-radius: 12px;
          color: white;
          cursor: pointer;
          transition: all 0.2s;
          box-shadow: 0 4px 20px rgba(99, 102, 241, 0.4);
        }
        .vr-button:hover {
          transform: translateY(-2px);
          box-shadow: 0 6px 30px rgba(99, 102, 241, 0.5);
        }
      `}</style>
    </div>
  )
}
