# TDT4290 Customer Driven Project - VR Training Application

A Unity-based Virtual Reality training application developed for TDT4290 Customer Driven Project course. This project implements a multi-level VR experience with dynamic audio systems, wall mechanics, and elevator navigation.

## 🎯 Project Overview

This VR application provides an immersive training environment where users navigate through multiple levels, interact with moving walls, and experience spatial audio feedback. The project focuses on providing realistic audio cues and haptic feedback for training scenarios.

## 🛠️ Technical Stack

### Unity Version
- **Unity 2023.3.x** (or later)
- **Universal Render Pipeline (URP) 17.2.0**

### Core Dependencies

#### VR/XR Packages
- **Meta XR SDK All (78.0.0)** - Complete Meta/Oculus VR integration
- **Meta XR SDK Audio (77.0.0)** - Spatial audio support for Meta headsets
- **Unity XR Interaction Toolkit (3.2.1)** - Cross-platform VR interaction system
- **Unity XR Management (4.5.1)** - XR provider management
- **Unity XR OpenXR (1.15.1)** - OpenXR standard support

#### AI & Workflow
- **Unity AI Assistant (1.0.0-pre.8)** - AI-powered development assistance
- **Unity AI Generators (1.0.0-pre.15)** - AI content generation tools
- **Unity AI Inference (2.2.1)** - Machine learning inference

#### Core Systems
- **Unity Input System (1.14.2)** - Modern input handling
- **Cinemachine (3.1.2)** - Advanced camera systems
- **Unity Learn IET Framework (5.0.1)** - Interactive tutorial framework

#### Development Tools
- **Unity Collab Proxy (2.9.2)** - Version control integration
- **Device Simulator Devices (1.0.0)** - Device testing
- **Multiplayer Center (1.0.0)** - Networking support

## 📁 Project Structure

```
Assets/
├── Audio/                    # Audio files and audio prefabs
│   ├── Background music and ambient sounds
│   ├── Button interaction sounds
│   ├── Wall collision audio
│   └── Voice line clips
├── Materials/               # Materials and material-related scripts
│   ├── Wall.cs             # Main wall movement and collision logic
│   └── WallSpawner.cs      # Wall spawning system
├── Prefabs/                # Reusable game objects
├── Resources/              # Runtime loadable assets
│   └── Levels/             # Level prefabs (Level 1-6)
├── Scripts/                # Core application scripts
│   ├── Audio/              # Audio-related systems
│   ├── Core/               # Core game mechanics
│   ├── UI/                 # User interface
│   ├── VR/                 # VR-specific functionality
│   └── Levels/             # Level management
├── Skyboxes/               # Environment skybox materials
├── UI/                     # User interface assets
└── VFX/                    # Visual effects
```

## 🎮 Core Systems

### Level Management
- **LevelManager.cs** - Handles scene transitions and level progression
- **LevelVoiceController.cs** - Manages voice guidance for each level
- **GameEvents.cs** - Event system for inter-system communication
- **Dynamic Level Loading** - Levels are loaded as prefabs from Resources/Levels/ at runtime

### Wall System
- **Wall.cs** - Dynamic wall movement with collision detection
- **WallSpawner.cs** - Procedural wall generation
- **WallCollisionsNew.cs** - Advanced wall collision handling
- **WallAudioLogic.cs** - Audio feedback for wall interactions

### Audio Systems
- **VoiceLineManager.cs** - Centralized voice line management
- **VoiceAudioRouter.cs** - Audio routing and mixing
- **AudioChannels.cs** - Audio channel management
- **HandTriggeredAudio.cs** - Hand-proximity audio triggers
- **SurfaceAttachAudio.cs** - Surface-based audio positioning
- **AudioSourceFollowWall.cs** - Dynamic audio source positioning

### VR Interaction
- **SimpleXRButton.cs** - VR button interactions
- **ButtonManager.cs** - Button state management
- **FirstButton.cs** - Tutorial button logic
- **PlayerMovement.cs** - VR player movement handling

### Elevator System
- **Elevator.cs** - Multi-state elevator with audio integration
- **ElevatorExitZone.cs** - Exit detection and management

## 🎵 Audio Features

### Spatial Audio
- **3D Positional Audio** - Realistic spatial audio using Unity's audio system
- **Distance-Based Volume** - Audio volume adjusts based on proximity
- **Dynamic Pitch Modulation** - Audio pitch changes based on wall proximity
- **Multi-Channel Audio Routing** - Separate audio channels for different game elements

### Audio Components
- **Background Music System** - Ambient background tracks
- **Voice Guidance System** - Contextual voice instructions
- **Interactive Audio Feedback** - Real-time audio responses to user actions
- **Wall Proximity Audio** - Audio intensity increases near moving walls

## 🚀 Getting Started

### Prerequisites
1. **Unity 2023.3.x** or later
2. **Meta Quest 2/3** or compatible VR headset
3. **Meta XR SDK** support
4. **Windows 10/11** or **macOS** development environment

### Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/davidmegli/TDT4290-Customer-Driven-Project.git
   cd TDT4290-Customer-Driven-Project
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" and select the project folder
   - Ensure Unity version compatibility

3. **Configure VR Settings**
   - Go to `Edit > Project Settings > XR Plug-in Management`
   - Enable your VR provider (Oculus/Meta or OpenXR)
   - Configure input mappings in `Input Actions`

4. **Build Settings**
   - Go to `File > Build Settings`
   - Select your target platform (PC or Android for Quest)
   - Add scenes in order: MainScene → Level scenes
   - Note: Level content is loaded dynamically from Resources/Levels/ prefabs

### Running the Application

1. **In Editor (Development)**
   - Connect VR headset
   - Open `MainScene.unity`
   - Press Play in Unity Editor
   - Use VR headset or simulate with keyboard/mouse

2. **Standalone Build**
   - Configure build settings for your platform
   - Build and run the executable
   - Launch with VR headset connected

## 🎮 Gameplay Flow

### Level Progression
1. **MainScene** - Introduction and tutorial instructions
2. **Level 1** - Basic wall avoidance mechanics (Resources/Levels/Level 1.prefab)
3. **Level 2** - Intermediate challenges with faster walls (Resources/Levels/Level 2.prefab)
4. **Level 3** - Advanced patterns and multiple walls (Resources/Levels/Level 3.prefab)
5. **Level 4** - Expert level with complex wall movements (Resources/Levels/Level 4.prefab)
6. **Level 5** - Advanced training scenarios (Resources/Levels/Level 5.prefab)
7. **Level 6** - Master level with maximum difficulty (Resources/Levels/Level 6.prefab)

### Core Mechanics
- **Wall Avoidance** - Navigate around moving walls without collision
- **Audio Guidance** - Follow voice instructions for optimal navigation
- **Spatial Awareness** - Use audio cues to detect wall proximity
- **Elevator Navigation** - Travel between levels using the elevator system

## 🔧 Configuration

### Level System
- **Dynamic Loading** - Levels are stored as prefabs in `Resources/Levels/` and loaded at runtime
- **6 Progressive Levels** - From basic tutorial to master-level challenges
- **Modular Design** - Each level prefab contains its own wall configurations and audio settings

### Audio Settings
- **Audio Mixer** - `NewAudioMixer.mixer` contains all audio routing
- **Volume Controls** - Separate volume controls for music, SFX, and voice
- **Spatial Audio Settings** - Configure 3D audio parameters in Inspector

### VR Settings
- **Tracking** - 6DOF head and hand tracking
- **Input Mapping** - Customizable input actions for different VR controllers
- **Comfort Settings** - Configurable comfort options for VR users

### Performance Settings
- **Quality Settings** - Adjustable graphics quality for different hardware
- **Frame Rate** - Target 90fps for VR comfort
- **Optimization** - LOD systems and occlusion culling enabled

## 🧪 Development & Testing

### Debug Features
- **Gizmos Visualization** - Visual debugging for wall paths and audio zones
- **Console Logging** - Comprehensive logging for troubleshooting
- **Inspector Tools** - Real-time parameter adjustment during play

### Testing Scenarios
- **VR Headset Testing** - Full VR experience testing
- **Desktop Simulation** - Mouse/keyboard testing for development
- **Audio Testing** - Spatial audio verification tools

## 📋 Known Issues & Limitations

### Current Limitations
- **VR Headset Required** - Optimal experience requires VR hardware
- **Platform Specific** - Primarily optimized for Meta Quest devices
- **Audio Dependencies** - Requires proper audio driver configuration

### Future Improvements
- **Cross-Platform VR Support** - Expand to other VR platforms
- **Advanced Haptic Feedback** - Enhanced tactile responses
- **Multiplayer Support** - Multi-user training scenarios
- **Analytics Integration** - Performance tracking and metrics

## 🤝 Contributing

### Development Workflow
1. Create feature branch from `main`
2. Implement changes with appropriate testing
3. Ensure code follows project conventions
4. Submit pull request with detailed description

### Code Style
- **C# Conventions** - Follow Microsoft C# coding guidelines
- **Unity Best Practices** - Adhere to Unity development patterns
- **Comment Standards** - Document public methods and complex logic

### Asset Guidelines
- **Audio Format** - Use compressed audio formats (OGG/MP3)
- **Texture Optimization** - Compress textures appropriately for VR
- **Model Optimization** - Keep polygon counts reasonable for VR performance

## 📄 License

This project is developed for educational purposes as part of the TDT4290 Customer Driven Project course.

## 👥 Team

**TDT4290 Development Team**
- Customer-driven development approach
- Agile development methodology
- Continuous integration and testing

## 📞 Support

For technical issues or questions:
1. Check the Unity Console for error messages
2. Verify VR headset compatibility
3. Ensure all dependencies are properly installed
4. Review audio driver configuration

---

**Note**: This project is part of an educational course and is continuously evolving. Some features may be in development or subject to change.

