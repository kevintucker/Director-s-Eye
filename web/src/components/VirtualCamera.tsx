import { useRef, useMemo } from 'react'
import { useFrame, useThree } from '@react-three/fiber'
import { Text, PerspectiveCamera, useFBO } from '@react-three/drei'
import * as THREE from 'three'
import type { VirtualCamera as VirtualCameraType } from '../types'
import { useAppStore } from '../stores/appStore'

interface VirtualCameraProps extends VirtualCameraType {
  isActive: boolean
  onSelect?: () => void
}

export function VirtualCamera({ 
  id, 
  position, 
  rotation, 
  focalLength, 
  aspectRatio,
  isActive,
  onSelect 
}: VirtualCameraProps) {
  const groupRef = useRef<THREE.Group>(null)
  const cameraRef = useRef<THREE.PerspectiveCamera>(null)
  const { setActiveCamera } = useAppStore()

  const fov = useMemo(() => {
    const sensorHeight = 24
    return 2 * Math.atan(sensorHeight / (2 * focalLength)) * (180 / Math.PI)
  }, [focalLength])

  const aspect = useMemo(() => {
    switch (aspectRatio) {
      case '2.39:1': return 2.39
      case '1.85:1': return 1.85
      case '1:1': return 1
      default: return 16 / 9
    }
  }, [aspectRatio])

  const renderTarget = useFBO(512, Math.floor(512 / aspect))

  const handleClick = (e: any) => {
    e.stopPropagation()
    setActiveCamera(id)
    onSelect?.()
  }

  const { gl, scene } = useThree()
  
  useFrame(() => {
    if (cameraRef.current && isActive) {
      const cam = cameraRef.current
      cam.position.set(...position)
      cam.rotation.set(
        rotation[0] * Math.PI / 180,
        rotation[1] * Math.PI / 180,
        rotation[2] * Math.PI / 180
      )
      cam.fov = fov
      cam.aspect = aspect
      cam.updateProjectionMatrix()

      gl.setRenderTarget(renderTarget)
      gl.render(scene, cam)
      gl.setRenderTarget(null)
    }
  })

  return (
    <group
      ref={groupRef}
      position={position}
      rotation={rotation.map(r => r * Math.PI / 180) as [number, number, number]}
    >
      <mesh onClick={handleClick}>
        <boxGeometry args={[0.3, 0.2, 0.4]} />
        <meshStandardMaterial 
          color={isActive ? '#22c55e' : '#374151'} 
          metalness={0.8}
          roughness={0.2}
        />
      </mesh>

      <group position={[0, 0, 0.25]} rotation={[Math.PI / 2, 0, 0]}>
        <mesh>
          <cylinderGeometry args={[0.08, 0.1, 0.15, 16]} />
          <meshStandardMaterial color="#1f2937" metalness={0.9} roughness={0.1} />
        </mesh>
      </group>

      <mesh position={[0, 0, 0.32]}>
        <circleGeometry args={[0.07, 16]} />
        <meshStandardMaterial color="#3b82f6" transparent opacity={0.5} />
      </mesh>

      {isActive && (
        <mesh>
          <coneGeometry args={[Math.tan(fov * Math.PI / 360) * 3, 3, 4]} />
          <meshBasicMaterial 
            color="#22c55e" 
            transparent 
            opacity={0.1} 
            wireframe 
            side={THREE.DoubleSide}
          />
        </mesh>
      )}

      {isActive && (
        <group position={[0, 0.5, 0]}>
          <mesh>
            <planeGeometry args={[0.6 * aspect / 2, 0.6 / 2]} />
            <meshBasicMaterial map={renderTarget.texture} />
          </mesh>
        </group>
      )}

      <Text
        position={[0, 0.35, 0]}
        fontSize={0.08}
        color="white"
        anchorX="center"
        anchorY="bottom"
        outlineWidth={0.01}
        outlineColor="black"
      >
        {`${focalLength}mm | ${aspectRatio}`}
      </Text>

      <PerspectiveCamera
        ref={cameraRef}
        fov={fov}
        aspect={aspect}
        near={0.1}
        far={100}
        makeDefault={false}
      />
    </group>
  )
}
