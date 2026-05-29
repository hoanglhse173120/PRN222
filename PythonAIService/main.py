from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
from sentence_transformers import SentenceTransformer
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Embedding Service", description="FastAPI microservice for generating embeddings using multilingual-e5-base")

# Load model globally when app starts
MODEL_NAME = "intfloat/multilingual-e5-base"
logger.info(f"Loading model {MODEL_NAME}...")
model = SentenceTransformer(MODEL_NAME)
logger.info("Model loaded successfully.")

class EmbedRequest(BaseModel):
    texts: List[str]
    # In e5 models, you often prefix documents with "passage: " and queries with "query: "
    prefix: str = "passage: "

class EmbedResponse(BaseModel):
    embeddings: List[List[float]]

@app.post("/embed", response_model=EmbedResponse)
async def embed_texts(request: EmbedRequest):
    # Prefix the texts for the e5 model requirement
    prefixed_texts = [request.prefix + text for text in request.texts]
    
    # Generate embeddings
    # normalize_embeddings=True is highly recommended for e5 models so we can use dot product (which equals cosine similarity then)
    embeddings = model.encode(prefixed_texts, normalize_embeddings=True).tolist()
    
    return EmbedResponse(embeddings=embeddings)

@app.get("/health")
async def health_check():
    return {"status": "ok", "model": MODEL_NAME}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
