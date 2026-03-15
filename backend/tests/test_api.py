"""
Tests for Director's Eye Backend API
"""

import pytest
from fastapi.testclient import TestClient
from app.main import app


client = TestClient(app)


def test_root():
    """Test root endpoint returns service info"""
    response = client.get("/")
    assert response.status_code == 200
    data = response.json()
    assert data["service"] == "Director's Eye API"
    assert data["status"] == "running"


def test_health_check():
    """Test health endpoint"""
    response = client.get("/health")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "healthy"
    assert "timestamp" in data


def test_generate_scene_demo_mode():
    """Test scene generation in demo mode (no API key)"""
    response = client.post("/api/generate", json={
        "prompt": "A dark alley at night",
        "style_name": "noir",
        "style_mood": "moody",
        "style_lighting": "high_contrast",
        "detail_level": 0.8,
        "output_format": "glb",
        "session_id": "test_session"
    })
    assert response.status_code == 200
    data = response.json()
    assert data["success"] is True
    assert data["scene_data_base64"] is not None


def test_generate_scene_missing_prompt():
    """Test scene generation with missing prompt"""
    response = client.post("/api/generate", json={
        "style_name": "cinematic"
    })
    assert response.status_code == 422  # Validation error


def test_refine_scene_demo_mode():
    """Test scene refinement in demo mode"""
    response = client.post("/api/refine", json={
        "refinement_prompt": "Add more fog",
        "preserve_layout": True,
        "session_id": "test_session"
    })
    assert response.status_code == 200
    data = response.json()
    assert data["success"] is True


def test_transcribe_endpoint_exists():
    """Test transcription endpoint exists (will fail without file)"""
    response = client.post("/api/transcribe")
    # Should return 422 (missing file) not 404
    assert response.status_code == 422
