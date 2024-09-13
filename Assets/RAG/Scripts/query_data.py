import argparse
import os
from langchain_community.vectorstores import Chroma
from langchain.prompts import ChatPromptTemplate
from langchain_community.llms.ollama import Ollama

from get_embedding_function import get_embedding_function
import warnings
warnings.filterwarnings("ignore", category=DeprecationWarning) 


CHROMA_PATH = "chroma"

#PROMPT_TEMPLATE = """
#
#Responda a pergunta com base apenas no seguinte contexto, se comportando como um paciente com Anorexia Nervosa:
#
#{context}
#
#---
#
#Responda a pergunta com base contexto acima, se comportando comom um paciente com Anorexia Nervosa: {question}
#"""

PROMPT_TEMPLATE = """

Responda a pergunta com base apenas no seguinte contexto, se comportando como um paciente {illness}:

{context}

---

Responda a pergunta com base no contexto acima, se comportando como um paciente com {illness}, evite falar de religião: {question}
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
    # write, while clearing, response to file model_output.txt
    with open("model_output.txt", "w") as f:
        f.write(response)




def query_rag(query_text: str, illness: str):
    # Prepare the DB.
    embedding_function = get_embedding_function()
    db = Chroma(persist_directory=CHROMA_PATH, embedding_function=embedding_function)

    # Search the DB.
    results = db.similarity_search_with_score(query_text, k=5)

    context_text = "\n\n---\n\n".join([doc.page_content for doc, _score in results])
    prompt_template = ChatPromptTemplate.from_template(PROMPT_TEMPLATE)
    prompt = prompt_template.format(context=context_text, question=query_text, illness=illness)
    # print(prompt)

    model = Ollama(model="llama3")
    response_text = model.invoke(prompt)

    sources = [doc.metadata.get("id", None) for doc, _score in results]
    formatted_response = f"Response: {response_text}\nSources: {sources}"
    print(formatted_response)
    return formatted_response


if __name__ == "__main__":
    main()
