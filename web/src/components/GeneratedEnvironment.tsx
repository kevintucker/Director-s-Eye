import type { SceneData } from '../types'

interface GeneratedEnvironmentProps {
  data: SceneData
}

export function GeneratedEnvironment({ data }: GeneratedEnvironmentProps) {
  const isDemoMode = data.metadata && (data.metadata as Record<string, unknown>).demo === true

  if (isDemoMode) {
    return <DemoEnvironment />
  }

  return <PlaceholderEnvironment />
}

function DemoEnvironment() {
  return (
    <group>
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, 0, 0]} receiveShadow>
        <planeGeometry args={[50, 50]} />
        <meshStandardMaterial color="#1a1a2e" roughness={0.8} />
      </mesh>

      <fog attach="fog" args={['#0a0a1a', 5, 30]} />

      {Array.from({ length: 20 }).map((_, i) => {
        const x = (Math.random() - 0.5) * 40
        const z = -10 - Math.random() * 20
        const height = 3 + Math.random() * 8
        const width = 1 + Math.random() * 2
        return (
          <mesh key={i} position={[x, height / 2, z]} castShadow>
            <boxGeometry args={[width, height, width]} />
            <meshStandardMaterial 
              color="#16213e" 
              emissive="#4361ee"
              emissiveIntensity={0.05}
            />
          </mesh>
        )
      })}

      <pointLight position={[-5, 2, -3]} color="#ff006e" intensity={2} distance={10} />
      <pointLight position={[5, 2, -5]} color="#00b4d8" intensity={2} distance={10} />
      <pointLight position={[0, 3, -8]} color="#7209b7" intensity={3} distance={15} />

      <mesh position={[-3, 0.4, 2]} castShadow>
        <boxGeometry args={[0.8, 0.8, 0.8]} />
        <meshStandardMaterial color="#4a4e69" />
      </mesh>
      <mesh position={[-3.5, 0.3, 2.5]} castShadow>
        <boxGeometry args={[0.6, 0.6, 0.6]} />
        <meshStandardMaterial color="#4a4e69" />
      </mesh>

      <mesh position={[4, 0.5, 1]} castShadow>
        <cylinderGeometry args={[0.3, 0.35, 1, 16]} />
        <meshStandardMaterial color="#3d405b" metalness={0.3} />
      </mesh>

      <group position={[2, 0, -2]}>
        <mesh position={[0, 1.5, 0]}>
          <cylinderGeometry args={[0.05, 0.08, 3, 8]} />
          <meshStandardMaterial color="#2b2d42" metalness={0.8} />
        </mesh>
        <mesh position={[0.3, 3, 0]}>
          <boxGeometry args={[0.6, 0.1, 0.2]} />
          <meshStandardMaterial color="#2b2d42" metalness={0.8} />
        </mesh>
        <pointLight position={[0.5, 2.9, 0]} color="#ffd166" intensity={1} distance={8} />
      </group>

      <ambientLight intensity={0.1} />
      <directionalLight 
        position={[5, 10, 5]} 
        intensity={0.3} 
        castShadow
      />
    </group>
  )
}

function PlaceholderEnvironment() {
  return (
    <group>
      <gridHelper args={[20, 20, '#444444', '#222222']} />
      
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.01, 0]}>
        <planeGeometry args={[20, 20]} />
        <meshStandardMaterial color="#1a1a1a" />
      </mesh>

      <mesh position={[0, 1, -5]}>
        <boxGeometry args={[4, 2, 0.1]} />
        <meshStandardMaterial color="#333333" />
      </mesh>

      <ambientLight intensity={0.5} />
      <directionalLight position={[5, 5, 5]} intensity={0.5} />
    </group>
  )
}
