# Bachelor-Thesis-Project

# Author
- Carlo Smaniotto Mantovani

# Dependencies
- Docker
- Docker Compose
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
cd Build_Dir/Avatar-Chatbot_Data/rag-docker
```
- Start the RAG Infrastructure container
```
docker compose up --build
```
- NOTE: you may need to use docker-compose command instead

## NOTES
- When creating the database, ollama, llama3 and the preliminary database will be setup. This process can take a significant amount of time of up to 20min+ on lower end machines.
- Inference may also take a significant amount of time if ran on a CPU (1min+) rather than a GPU (10s+)

