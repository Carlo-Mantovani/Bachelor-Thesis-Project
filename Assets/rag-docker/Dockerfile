# Use an official Python image
FROM python:3.9-slim

# Set environment variables
ENV CHROMA_PATH=/app/chroma
ENV DATA_PATH=/app/data

# Set working directory
WORKDIR /app

# Install system dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    build-essential \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install Ollama (assuming there is an Ollama CLI for Linux)
RUN curl -fsSL https://ollama.com/install.sh | sh

#RUN systemctl start ollama.service
# Pull Ollama models
#RUN ollama pull llama3 && ollama pull nomic-embed-text
RUN ollama serve & sleep 3 && ollama pull llama3 
RUN ollama serve & sleep 3 && ollama pull nomic-embed-text

# Copy the local requirements file to the container
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the application code
COPY . .

# Create base database
RUN ollama serve & sleep 3 && python populate_database.py

# Define the entrypoint for the container
CMD ["bash", "-c", "ollama serve & sleep infinity"]

