# MetaSigns: AI-Powered Learning with Sign Language for Key School Concepts

## Project Overview
**MetaSigns** is an educational game that challenges kids to identify key school concept words inside a short paragraph. It integrates a Unity game application with Meta’s LLaMA text generator using the Neocortex API. MetaSigns also uses accessibility and cutting-edge technology to enhance kids’ vocabulary on any school subject. It displays sign language videos for key concept words to aid learning.

## System Architecture
The MetaSigns game consists of three main layers:

- **Unity Frontend**: The UI application that collects input, displays the paragraph/videos, and validates the chosen words.  
- **Neocortex API**: The connection between Unity and LLaMA. It transmits input to LLaMA and returns the generated paragraph.  
- **LLaMA Backend**: Receives input (school subject, concept words) from Unity and generates a short paragraph including the concept words.

### [ Unity Game (UI + Video) ]
### ⇅
### [ Neocortex API Layer ]
### ⇅
### [ LLaMA Server (NLP) ]


## Gameplay Process
1. A school subject is pre-selected (e.g. Biology).
2. The user presses the **Play** button.
3. A paragraph is displayed containing 2–4 concept words related to the subject (e.g. *Membrane*, *Cell*).
4. When the user clicks a concept word:
   - It is highlighted **green** if correct (and plays the sign language video).
   - It is highlighted **red** if incorrect.

## Project Structure


MetaSigns/
├── Assets/
│ ├── Resources/
│ │ ├── aslVideos/ # Sign language videos
│ │ └── Concepts.xlsx # Excel file with subject & concept words
│ └── Scripts/
│ ├── LlamaParagraphGenerator.cs # Main game logic
│ ├── BackendManager.cs # Handles Excel input
│ └── VideoManager.cs # Handles video playback
├── LLaMA-Backend/
│ ├── app.py # FastAPI server using Ollama
│ └── requirements.txt # Python dependencies
├── README.md # Project documentation
├── .gitignore
└── LICENSE



## Setup & Dependencies

### Installing Unity
- [Download Unity Hub](https://unity.com/download)
- [Unity Documentation](https://docs.unity3d.com/)

### Installing LLaMA 2
1. [Download Python](https://www.python.org/downloads/)
2. Install required packages:
   ```bash
   pip install transformers torch accelerate fastapi uvicorn
3. Request access to Meta LLaMA 2
   pip install neocortex



### Installing NPOI for Unity

In Unity, go to Window > Package Manager. Click the + symbol.

Paste this URL: https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity

Reopen Unity. Then go to NuGet > Manage NuGet Packages, search for npoi, and install it.

### Sign Language Videos
Place video files in the following directory: Assets/Resources/aslVideos/

### Algorithm Overview

1. When the user presses Start:

2. A prompt is created using a randomly selected school subject and 2–4 concept words from a spreadsheet.

3. The LLaMA backend (via Ollama) generates a paragraph.

4. Words in the paragraph: Are highlighted yellow on hover.

5. Turn green and play a sign language video if clicked and correct.

6. Turn red if clicked and incorrect.

### Accessibility & Inclusion
MetaSigns promotes inclusivity by engaging all children, including those who are partially or fully deaf. By combining text with visual sign language demonstrations, it helps solidify vocabulary through multiple sensory pathways (reading and watching).

### Summary
MetaSigns is an educational game designed to help children—especially deaf learners—build vocabulary through an interactive, inclusive experience. Using Unity, LLaMA, and the Neocortex API, it combines AI-generated content, gameplay, and sign language videos to enhance learning outcomes.
