# Contributing to Director's Eye

Thank you for your interest in contributing to Director's Eye!

## Development Setup

### Prerequisites
- Unity 2022.3 LTS
- Python 3.10+
- Pico 4 / Pico 4 Ultra headset
- Android SDK (for Pico builds)

### Unity Setup
1. Clone the repository
2. Open `unity/` folder in Unity Hub
3. Install Pico SDK via Package Manager
4. Install GLTFUtility for mesh loading

### Backend Setup
```bash
cd backend
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
# Edit .env with your API keys
uvicorn app.main:app --reload
```

## Code Style

### C# (Unity)
- Use PascalCase for public members
- Use camelCase for private members with underscore prefix (`_privateField`)
- Use `#region` blocks sparingly
- Add XML documentation for public APIs

### Python (Backend)
- Follow PEP 8
- Use type hints
- Use docstrings for functions

## Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Hackathon Contributors

This project was created during the Worlds in Action Hack - San Francisco.

Core Team:
- [Add team member names]

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
