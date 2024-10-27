import argparse
import os
from langchain_community.vectorstores import Chroma
from langchain.prompts import ChatPromptTemplate
from langchain_community.llms.ollama import Ollama

from get_embedding_function import get_embedding_function
import warnings
warnings.filterwarnings("ignore", category=DeprecationWarning) 


CHROMA_PATH = "chroma"


PROMPT_TEMPLATE = """
Responda à pergunta com base apenas no seguinte contexto, se comportando como um paciente {illness} masculino (use palavras masculinas quando necessário) (cujo comportamento é indicado pelo contexto abaixo). Responda seguindo o formato:


[Emoção]

Resposta à pergunta


As emoções possíveis são: [Neutro, Felicidade, Tristeza, Raiva, Surpresa], escolha a que melhor se encaixa na sua resposta.

---

{context}

---
Novamente as emoções possíveis são: [Neutro, Felicidade, Tristeza, Raiva, Surpresa], escolha a que melhor se encaixa na sua resposta.

Responda à pergunta com base no contexto acima, se comportando como um paciente com {illness} (cujo comportamento é indicado pelo contexto acima), evite falar de religião e do número de calorias, seja realista e ambíguo, não dê respostas muito detalhadas: {question}
"""



def main():
    # Create CLI.
    parser = argparse.ArgumentParser()
    parser.add_argument("query_text", type=str, help="The query text.")
    # The illness to be used for the query.
    parser.add_argument("--illness", type=str, default="anorexia", help="The illness to be used for the query.")
    args = parser.parse_args()
    query_text = args.query_text
    illness = args.illness
    response = query_rag(query_text, illness)
    # remove empty lines
    response = os.linesep.join([s for s in response.splitlines() if s])
    mood, text = separate_text(response)
    
    # write, while clearing, response text to file model_output.txt
    with open("./outputs/model_output.txt", "w") as f:
        f.write(text)
    # write, while clearing, emotion to file mood_output.txt
    with open("./outputs/mood_output.txt", "w") as f:
        f.write(mood)


# Function to separate the response text from the emotion
def separate_text(response):
    mood = response.split("\n")[0]
    # Remove brackets from emotion if they exist
    if mood[0] == "[":
        mood = mood[1:-1]
    text = "\n".join(response.split("\n")[1:])
    # remove empty lines
    text = os.linesep.join([s for s in text.splitlines() if s])
    return mood, text
    
    

def query_rag(query_text: str, illness: str):
    # Prepare the DB.
    embedding_function = get_embedding_function()
    db = Chroma(persist_directory=CHROMA_PATH, embedding_function=embedding_function)

    # Search the DB.
    results = db.similarity_search_with_score(query_text, k=10)

    context_text = "\n\n---\n\n".join([doc.page_content for doc, _score in results])
    prompt_template = ChatPromptTemplate.from_template(PROMPT_TEMPLATE)
    prompt = prompt_template.format(context=context_text, question=query_text, illness=illness)
    # print(prompt)

   # Initialize the Ollama model with customized parameters for randomness
    model = Ollama(
        model="llama3", # The model to use.
        temperature=1.0,  # Lower temperature means less randomness.
    )

    response_text = model.invoke(prompt)

    sources = [doc.metadata.get("id", None) for doc, _score in results]
    #formatted_response = f"{response_text}\nSources: {sources}"
    formatted_response = f"{response_text}"
    print(formatted_response)
    return formatted_response


if __name__ == "__main__":
    main()
