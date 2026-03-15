"""
World Labs Marble API Client
Handles communication with the Marble 3D generation API
"""

import os
import asyncio
import logging
from typing import Optional, Dict, Any
from dataclasses import dataclass
from enum import Enum

import httpx

logger = logging.getLogger("marble-client")


class OutputFormat(Enum):
    GLB = "glb"
    GLTF = "gltf"
    USD = "usd"
    FBX = "fbx"


@dataclass
class SceneStyle:
    name: str = "cinematic"
    mood: str = "dramatic"
    lighting: str = "natural"
    detail_level: float = 0.8
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "name": self.name,
            "mood": self.mood,
            "lighting": self.lighting,
            "detail_level": self.detail_level
        }


@dataclass
class GenerationOptions:
    generate_colliders: bool = True
    optimize_for_vr: bool = True
    lod_levels: int = 3
    texture_resolution: int = 2048
    pbr_materials: bool = True
    
    def to_dict(self) -> Dict[str, Any]:
        return {
            "generate_colliders": self.generate_colliders,
            "optimize_for_vr": self.optimize_for_vr,
            "lod_levels": self.lod_levels,
            "texture_resolution": self.texture_resolution,
            "pbr_materials": self.pbr_materials
        }


@dataclass
class GenerationResult:
    success: bool
    scene_id: Optional[str] = None
    mesh_data: Optional[bytes] = None
    texture_data: Optional[bytes] = None
    metadata: Optional[Dict[str, Any]] = None
    error: Optional[str] = None


class MarbleAPIClient:
    """Client for World Labs Marble API"""
    
    def __init__(
        self,
        api_key: Optional[str] = None,
        base_url: str = "https://api.worldlabs.ai/v1",
        timeout: float = 120.0
    ):
        self.api_key = api_key or os.getenv("MARBLE_API_KEY", "")
        self.base_url = base_url.rstrip("/")
        self.timeout = timeout
        self._session_cache: Dict[str, Dict] = {}
    
    @property
    def headers(self) -> Dict[str, str]:
        return {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
            "X-Client": "directors-eye-v1"
        }
    
    async def generate_environment(
        self,
        prompt: str,
        style: Optional[SceneStyle] = None,
        options: Optional[GenerationOptions] = None,
        output_format: OutputFormat = OutputFormat.GLB,
        session_id: Optional[str] = None
    ) -> GenerationResult:
        """Generate a 3D environment from text prompt"""
        
        if not self.api_key:
            logger.warning("No API key configured, using demo mode")
            return self._demo_result(prompt)
        
        style = style or SceneStyle()
        options = options or GenerationOptions()
        
        payload = {
            "prompt": prompt,
            "style": style.to_dict(),
            "options": options.to_dict(),
            "output_format": output_format.value
        }
        
        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                # Initial generation request
                response = await client.post(
                    f"{self.base_url}/generate",
                    headers=self.headers,
                    json=payload
                )
                
                if response.status_code == 202:
                    # Async generation - poll for result
                    task_id = response.json().get("task_id")
                    return await self._poll_generation(client, task_id)
                
                elif response.status_code == 200:
                    # Sync generation - result ready
                    data = response.json()
                    result = GenerationResult(
                        success=True,
                        scene_id=data.get("scene_id"),
                        mesh_data=self._decode_base64(data.get("mesh_data")),
                        texture_data=self._decode_base64(data.get("texture_data")),
                        metadata=data.get("metadata")
                    )
                    
                    if session_id:
                        self._session_cache[session_id] = {
                            "scene_id": result.scene_id,
                            "prompt": prompt,
                            "style": style.to_dict()
                        }
                    
                    return result
                
                else:
                    return GenerationResult(
                        success=False,
                        error=f"API error {response.status_code}: {response.text}"
                    )
                    
        except httpx.TimeoutException:
            return GenerationResult(success=False, error="Request timed out")
        except Exception as e:
            logger.error(f"Generation error: {e}")
            return GenerationResult(success=False, error=str(e))
    
    async def refine_environment(
        self,
        refinement_prompt: str,
        session_id: str,
        preserve_layout: bool = True
    ) -> GenerationResult:
        """Refine an existing environment with additional instructions"""
        
        session = self._session_cache.get(session_id)
        if not session:
            return GenerationResult(
                success=False,
                error="No active session found. Generate a scene first."
            )
        
        if not self.api_key:
            return self._demo_result(refinement_prompt, refined=True)
        
        payload = {
            "scene_id": session.get("scene_id"),
            "refinement_prompt": refinement_prompt,
            "preserve_layout": preserve_layout,
            "original_context": session.get("prompt", "")
        }
        
        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.post(
                    f"{self.base_url}/refine",
                    headers=self.headers,
                    json=payload
                )
                
                if response.status_code == 200:
                    data = response.json()
                    return GenerationResult(
                        success=True,
                        scene_id=data.get("scene_id"),
                        mesh_data=self._decode_base64(data.get("mesh_data")),
                        texture_data=self._decode_base64(data.get("texture_data")),
                        metadata=data.get("metadata")
                    )
                else:
                    return GenerationResult(
                        success=False,
                        error=f"API error {response.status_code}: {response.text}"
                    )
                    
        except Exception as e:
            logger.error(f"Refinement error: {e}")
            return GenerationResult(success=False, error=str(e))
    
    async def _poll_generation(
        self,
        client: httpx.AsyncClient,
        task_id: str,
        max_attempts: int = 60,
        poll_interval: float = 2.0
    ) -> GenerationResult:
        """Poll for async generation result"""
        
        for attempt in range(max_attempts):
            response = await client.get(
                f"{self.base_url}/tasks/{task_id}",
                headers=self.headers
            )
            
            if response.status_code != 200:
                return GenerationResult(
                    success=False,
                    error=f"Poll error: {response.status_code}"
                )
            
            data = response.json()
            status = data.get("status")
            
            if status == "completed":
                return GenerationResult(
                    success=True,
                    scene_id=data.get("scene_id"),
                    mesh_data=self._decode_base64(data.get("mesh_data")),
                    texture_data=self._decode_base64(data.get("texture_data")),
                    metadata=data.get("metadata")
                )
            
            elif status == "failed":
                return GenerationResult(
                    success=False,
                    error=data.get("error", "Generation failed")
                )
            
            await asyncio.sleep(poll_interval)
        
        return GenerationResult(success=False, error="Generation timed out")
    
    def _decode_base64(self, data: Optional[str]) -> Optional[bytes]:
        """Decode base64 string to bytes"""
        if not data:
            return None
        import base64
        return base64.b64decode(data)
    
    def _demo_result(self, prompt: str, refined: bool = False) -> GenerationResult:
        """Return demo result when no API key is configured"""
        import base64
        
        demo_mesh = b"DEMO_GLB_MESH_DATA_" + prompt[:20].encode()
        demo_texture = b"DEMO_TEXTURE_DATA"
        
        return GenerationResult(
            success=True,
            scene_id=f"demo_{hash(prompt) % 10000}",
            mesh_data=demo_mesh,
            texture_data=demo_texture,
            metadata={
                "demo": True,
                "refined": refined,
                "prompt_preview": prompt[:50]
            }
        )
    
    def get_session(self, session_id: str) -> Optional[Dict]:
        """Get cached session data"""
        return self._session_cache.get(session_id)
    
    def clear_session(self, session_id: str):
        """Clear session cache"""
        self._session_cache.pop(session_id, None)
