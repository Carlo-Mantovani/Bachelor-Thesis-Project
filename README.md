# Bachelor-Thesis-Project

# Author
- Carlo Smaniotto Mantovani

# Dependencies
- Docker
- Docker compose
- Unity, if running from editor

# Run

## Editor
- Open the Editor
- Before clicking "Play" execute the following commands from the root directory
- Change to rag-docker directory
```
cd Assets/rag-docker
```
- Start the RAG infrastructure container
```
docker compose up --build
```
- NOTE: you may need to use docker-compose command instead

## Build
- Before executing the application
- From root directory, cd to data directory from build
```
cd Builds/Build_Dir/Avatar-Chatbot_Data/rag-docker
```
- Start the RAG Infrastructure container
```
docker compose up --build
```
- NOTE: you may need to use docker-compose command instead

