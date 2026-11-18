import { config } from "./config.js";

const endpoint = `${config.apiBaseUrl.replace(/\/+$/, "")}/api/categorias`;

class ErroApi extends Error {
  constructor(mensagem, campo) {
    super(mensagem);
    this.campo = campo;
  }
}

async function requisitar(url, opcoes) {
  let resposta;

  try {
    resposta = await fetch(url, opcoes);
  } catch {
    throw new ErroApi("Não foi possível conectar à API. Verifique a conexão e tente novamente.");
  }

  if (opcoes?.method === "DELETE" && resposta.ok) {
    return;
  }

  const dados = await resposta.json().catch(() => null);

  if (!resposta.ok) {
    throw new ErroApi(
      dados?.mensagem || dados?.erro || dados?.detail || `A API retornou um erro (${resposta.status}).`,
      dados?.campo,
    );
  }

  if (dados === null) {
    throw new ErroApi("A API retornou uma resposta inválida.");
  }

  return dados;
}

function enviarNome(url, method, nome) {
  return requisitar(url, {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ nome }),
  });
}

export const apiCategorias = {
  listar: () => requisitar(endpoint),
  criar: (nome) => enviarNome(endpoint, "POST", nome),
  atualizar: (id, nome) => enviarNome(`${endpoint}/${id}`, "PUT", nome),
  excluir: (id) => requisitar(`${endpoint}/${id}`, { method: "DELETE" }),
};
