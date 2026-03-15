"""
Director's Eye - Backend API
FastAPI server for World Labs Marble API integration and voice transcription
"""

import os
import io
import base64
import logging
from typing import Optional
from datetime import datetime

from fastapi import FastAPI, HTTPException, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import httpx

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("directors-eye")

app = FastAPI(
    title="Director's Eye API",
    description="Backend for VR filmmaking previs tool",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Configuration
MARBLE_API_URL = os.getenv("MARBLE_API_URL", "https://api.worldlabs.ai/v1")
MARBLE_API_KEY = os.getenv("MARBLE_API_KEY", "")
WHISPER_API_URL = os.getenv("WHISPER_API_URL", "https://api.openai.com/v1/audio/transcriptions")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "")

# Session storage for scene context
sessions: dict = {}


class SceneStyle(BaseModel):
    name: str = "cinematic"
    mood: str = "dramatic"
    lighting: str = "natural"
    detail_level: float = 0.8


class GenerateRequest(BaseModel):
    prompt: str
    style_name: str = "cinematic"
    style_mood: str = "dramatic"
    style_lighting: str = "natural"
    detail_level: float = 0.8
    output_format: str = "glb"
    session_id: str = ""


class RefineRequest(BaseModel):
    refinement_prompt: str
    preserve_layout: bool = True
    session_id: str = ""


class GenerateResponse(BaseModel):
    success: bool
    error: Optional[str] = None
    scene_data_base64: Optional[str] = None
    texture_data_base64: Optional[str] = None
    metadata: Optional[str] = None


class TranscriptionResponse(BaseModel):
    text: str
    confidence: float = 1.0


@app.get("/")
async def root():
    return {"service": "Director's Eye API", "status": "running"}


@app.get("/health")
async def health_check():
    return {"status": "healthy", "timestamp": datetime.utcnow().isoformat()}


@app.post("/api/generate", response_model=GenerateResponse)
async def generate_scene(request: GenerateRequest):
    """Generate a 3D environment from text prompt using World Labs Marble API"""
    logger.info(f"Generate request: {request.prompt[:100]}...")
    
    if not MARBLE_API_KEY:
        # Demo mode - return placeholder data
        logger.warning("No Marble API key configured, returning demo response")
        return GenerateResponse(
            success=True,
            scene_data_base64=base64.b64encode(b"DEMO_MESH_DATA").decode(),
            texture_data_base64=base64.b64encode(b"DEMO_TEXTURE_DATA").decode(),
            metadata='{"demo": true, "prompt": "' + request.prompt[:50] + '"}'
        )
    
    try:
        enhanced_prompt = build_enhanced_prompt(request)
        
        async with httpx.AsyncClient(timeout=120.0) as client:
            response = await client.post(
                f"{MARBLE_API_URL}/generate",
                headers={
                    "Authorization": f"Bearer {MARBLE_API_KEY}",
                    "Content-Type": "application/json"
                },
                json={
                    "prompt": enhanced_prompt,
                    "style": {
                        "name": request.style_name,
                        "mood": request.style_mood,
                        "lighting": request.style_lighting,
                        "detail_level": request.detail_level
                    },
                    "output_format": request.output_format,
                    "options": {
                        "generate_colliders": True,
                        "optimize_for_vr": True,
                        "lod_levels": 3
                    }
                }
            )
            
            if response.status_code != 200:
                raise HTTPException(
                    status_code=response.status_code,
                    detail=f"Marble API error: {response.text}"
                )
            
            data = response.json()
            
            # Store session context for refinements
            if request.session_id:
                sessions[request.session_id] = {
                    "original_prompt": request.prompt,
                    "scene_id": data.get("scene_id"),
                    "style": {
                        "name": request.style_name,
                        "mood": request.style_mood,
                        "lighting": request.style_lighting
                    }
                }
            
            return GenerateResponse(
                success=True,
                scene_data_base64=data.get("mesh_data"),
                texture_data_base64=data.get("texture_data"),
                metadata=str(data.get("metadata", {}))
            )
            
    except httpx.TimeoutException:
        raise HTTPException(status_code=504, detail="Marble API request timed out")
    except Exception as e:
        logger.error(f"Generation error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/refine", response_model=GenerateResponse)
async def refine_scene(request: RefineRequest):
    """Refine an existing scene with additional instructions"""
    logger.info(f"Refine request: {request.refinement_prompt[:100]}...")
    
    session = sessions.get(request.session_id, {})
    
    if not MARBLE_API_KEY:
        return GenerateResponse(
            success=True,
            scene_data_base64=base64.b64encode(b"REFINED_MESH_DATA").decode(),
            texture_data_base64=base64.b64encode(b"REFINED_TEXTURE_DATA").decode(),
            metadata='{"demo": true, "refined": true}'
        )
    
    try:
        async with httpx.AsyncClient(timeout=120.0) as client:
            response = await client.post(
                f"{MARBLE_API_URL}/refine",
                headers={
                    "Authorization": f"Bearer {MARBLE_API_KEY}",
                    "Content-Type": "application/json"
                },
                json={
                    "scene_id": session.get("scene_id"),
                    "refinement_prompt": request.refinement_prompt,
                    "preserve_layout": request.preserve_layout,
                    "original_context": session.get("original_prompt", "")
                }
            )
            
            if response.status_code != 200:
                raise HTTPException(
                    status_code=response.status_code,
                    detail=f"Marble API error: {response.text}"
                )
            
            data = response.json()
            
            return GenerateResponse(
                success=True,
                scene_data_base64=data.get("mesh_data"),
                texture_data_base64=data.get("texture_data"),
                metadata=str(data.get("metadata", {}))
            )
            
    except Exception as e:
        logger.error(f"Refinement error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/transcribe", response_model=TranscriptionResponse)
async def transcribe_audio(audio: UploadFile = File(...)):
    """Transcribe audio using OpenAI Whisper API"""
    logger.info(f"Transcribe request: {audio.filename}")
    
    if not OPENAI_API_KEY:
        # Demo mode
        return TranscriptionResponse(
            text="Demo transcription: A dark alley at night with rain falling",
            confidence=0.95
        )
    
    try:
        audio_data = await audio.read()
        
        async with httpx.AsyncClient(timeout=60.0) as client:
            response = await client.post(
                WHISPER_API_URL,
                headers={
                    "Authorization": f"Bearer {OPENAI_API_KEY}"
                },
                files={
                    "file": (audio.filename, io.BytesIO(audio_data), audio.content_type),
                },
                data={
                    "model": "whisper-1",
                    "language": "en"
                }
            )
            
            if response.status_code != 200:
                raise HTTPException(
                    status_code=response.status_code,
                    detail=f"Whisper API error: {response.text}"
                )
            
            data = response.json()
            
            return TranscriptionResponse(
                text=data.get("text", ""),
                confidence=1.0
            )
            
    except Exception as e:
        logger.error(f"Transcription error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


def build_enhanced_prompt(request: GenerateRequest) -> str:
    """Enhance user prompt with cinematic context for better results"""
    base_prompt = request.prompt.strip()
    
    enhancements = []
    
    # Add style context
    if request.style_name == "cinematic":
        enhancements.append("cinematic composition, film set quality")
    elif request.style_name == "noir":
        enhancements.append("film noir aesthetic, high contrast shadows")
    
    # Add lighting context
    lighting_map = {
        "natural": "natural ambient lighting",
        "dramatic": "dramatic directional lighting with shadows",
        "volumetric": "volumetric god rays and atmospheric haze",
        "high_contrast": "high contrast lighting, deep blacks",
        "soft": "soft diffused lighting"
    }
    if request.style_lighting in lighting_map:
        enhancements.append(lighting_map[request.style_lighting])
    
    # Add mood context
    mood_map = {
        "dramatic": "dramatic atmosphere",
        "moody": "moody and atmospheric",
        "tense": "tense and suspenseful atmosphere",
        "romantic": "warm romantic atmosphere",
        "horror": "eerie unsettling atmosphere"
    }
    if request.style_mood in mood_map:
        enhancements.append(mood_map[request.style_mood])
    
    # Add VR optimization hints
    enhancements.append("optimized for VR walkthrough, human-scale environment")
    
    enhanced = f"{base_prompt}. {', '.join(enhancements)}"
    logger.info(f"Enhanced prompt: {enhanced[:200]}...")
    
    return enhanced


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
