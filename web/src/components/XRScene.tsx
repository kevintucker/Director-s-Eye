import { Suspense, useCallback, useRef } from 'react'
import { Canvas } from '@react-three/fiber'
import { XR, Controllers, Hands } from '@react-three/xr'
import { OrbitControls, Sky } from '@react-three/drei'
import * as THREE from 'three'
import { useAppStore } from '../stores/appStore'
import { ActorModel } from './ActorModel'
import { VirtualCamera } from './VirtualCamera'
import { GeneratedEnvironment } from './GeneratedEnvironment'

interface ClickEvent {
  point: THREE.Vector3
  stopPropagation: () => void
}

function Scene() {
  const { 
    environment, 
    actors, 
    cameras, 
    activeCamera, 
    mode,
    addActor,
    addCamera
  } = useAppStore()
  
  const floorRef = useRef<THREE.Mesh>(null)

  const handleFloorClick = useCallback((event: ClickEvent) => {
    if (!event.point) return

    const point: [number, number, number] = [
      event.point.x,
      0,
      event.point.z
    ]

    if (mode === 'placeActor') {
      addActor(point)
    } else if (mode === 'placeCamera') {
      addCamera([point[0], 1.6, point[2]])
    }
  }, [mode, addActor, addCamera])

  return (
    <>
      {environment ? (
        <GeneratedEnvironment data={environment} />
      ) : (
        <DefaultEnvironment />
      )}

      <mesh 
        ref={floorRef}
        rotation={[-Math.PI / 2, 0, 0]} 
        position={[0, 0.001, 0]}
        onClick={handleFloorClick as any}
        visible={false}
      >
        <planeGeometry args={[100, 100]} />
        <meshBasicMaterial transparent opacity={0} />
      </mesh>

      {actors.map((actor) => (
        <ActorModel key={actor.id} {...actor} />
      ))}

      {cameras.map((camera) => (
        <VirtualCamera 
          key={camera.id} 
          {...camera} 
          isActive={camera.id === activeCamera}
        />
      ))}

      <Controllers />
      <Hands />
    </>
  )
}

function DefaultEnvironment() {
  return (
    <>
      <gridHelper args={[30, 30, '#333333', '#1a1a1a']} />
      
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.01, 0]} receiveShadow>
        <planeGeometry args={[30, 30]} />
        <meshStandardMaterial color="#111111" />
      </mesh>

      <ambientLight intensity={0.4} />
      <directionalLight 
        position={[10, 10, 5]} 
        intensity={0.6} 
        castShadow
      />

      <Sky sunPosition={[100, 10, 100]} />
    </>
  )
}

interface XRSceneProps {
  onCanvasReady?: (canvas: HTMLCanvasElement) => void
}

export function XRScene({ onCanvasReady }: XRSceneProps) {
  return (
    <Canvas
      shadows
      camera={{ position: [0, 5, 10], fov: 60 }}
      style={{ background: '#0a0a0a' }}
      gl={{ preserveDrawingBuffer: true }}
      onCreated={({ gl }) => {
        if (onCanvasReady) {
          onCanvasReady(gl.domElement)
        }
      }}
    >
      <XR>
        <Suspense fallback={null}>
          <Scene />
          <OrbitControls 
            makeDefault
            minDistance={2}
            maxDistance={50}
            minPolarAngle={0.1}
            maxPolarAngle={Math.PI / 2 - 0.1}
          />
        </Suspense>
      </XR>
    </Canvas>
  )
}
