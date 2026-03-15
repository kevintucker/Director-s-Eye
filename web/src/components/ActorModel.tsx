import { useRef, useState } from 'react'
import { Text } from '@react-three/drei'
import * as THREE from 'three'
import type { Actor } from '../types'
import { useAppStore } from '../stores/appStore'

interface ActorModelProps extends Actor {
  onSelect?: () => void
}

export function ActorModel({ position, rotation, pose, color, label, onSelect }: ActorModelProps) {
  const groupRef = useRef<THREE.Group>(null)
  const [hovered, setHovered] = useState(false)
  const { mode } = useAppStore()

  const isGrabbable = mode === 'grab'

  const getPoseTransform = () => {
    switch (pose) {
      case 'sitting':
        return { bodyY: -0.3, scale: [1, 0.6, 1] as [number, number, number] }
      case 'crouching':
        return { bodyY: -0.2, scale: [1, 0.7, 1] as [number, number, number] }
      default:
        return { bodyY: 0, scale: [1, 1, 1] as [number, number, number] }
    }
  }

  const poseTransform = getPoseTransform()

  const handlePointerDown = (e: any) => {
    e.stopPropagation()
    if (isGrabbable) {
      onSelect?.()
    }
  }

  return (
    <group
      ref={groupRef}
      position={position}
      rotation={rotation.map(r => r * Math.PI / 180) as [number, number, number]}
    >
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, 0.01, 0]}>
        <ringGeometry args={[0.3, 0.4, 32]} />
        <meshBasicMaterial color={color} transparent opacity={0.8} />
      </mesh>

      <group position={[0, poseTransform.bodyY, 0]} scale={poseTransform.scale}>
        <mesh 
          position={[0, 1, 0]}
          onPointerDown={handlePointerDown}
          onPointerOver={() => setHovered(true)}
          onPointerOut={() => setHovered(false)}
        >
          <capsuleGeometry args={[0.25, 0.6, 8, 16]} />
          <meshStandardMaterial 
            color={hovered && isGrabbable ? '#ffffff' : color} 
            emissive={hovered && isGrabbable ? color : '#000000'}
            emissiveIntensity={hovered ? 0.3 : 0}
          />
        </mesh>

        <mesh position={[0, 1.65, 0]}>
          <sphereGeometry args={[0.15, 16, 16]} />
          <meshStandardMaterial color={color} />
        </mesh>

        <mesh position={[-0.35, 1, 0]} rotation={[0, 0, -0.2]}>
          <capsuleGeometry args={[0.06, 0.4, 4, 8]} />
          <meshStandardMaterial color={color} />
        </mesh>
        <mesh position={[0.35, 1, 0]} rotation={[0, 0, 0.2]}>
          <capsuleGeometry args={[0.06, 0.4, 4, 8]} />
          <meshStandardMaterial color={color} />
        </mesh>

        <mesh position={[-0.12, 0.4, 0]}>
          <capsuleGeometry args={[0.08, 0.5, 4, 8]} />
          <meshStandardMaterial color={color} />
        </mesh>
        <mesh position={[0.12, 0.4, 0]}>
          <capsuleGeometry args={[0.08, 0.5, 4, 8]} />
          <meshStandardMaterial color={color} />
        </mesh>
      </group>

      <Text
        position={[0, 2.2, 0]}
        fontSize={0.15}
        color="white"
        anchorX="center"
        anchorY="bottom"
        outlineWidth={0.02}
        outlineColor="black"
      >
        {label}
      </Text>
    </group>
  )
}
