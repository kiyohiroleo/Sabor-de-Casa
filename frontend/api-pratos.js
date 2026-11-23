import { config } from "./config.js";

// CONTRATO ESPERADO DA API DA PESSOA 4 (baseado em database/schema.sql e no padrão de /api/categorias):
//
//   GET    /api/pratos                  -> [ { id, nome, preco, disponivel, categoriaId } ]
//   GET    /api/pratos?categoriaId=1    -> (opcional) mesma lista filtrada; o frontend filtra sozinho se a API ignorar
//   POST   /api/pratos                  -> body { nome, preco, disponivel, categoriaId } -> 201 + prato criado
//   PUT    /api/pratos/{id}             -> mesmo body -> 200 + prato atualizado
//   DELETE /api/pratos/{id}             -> 200 (ou 204)
//
// Erros: JSON { statusCode, codigo, erro, mensagem, campo? }, como em categorias.
// O campo `campo` deve ser "nome", "preco" ou "categoriaId" para o erro aparecer no input certo.
//
// Para testar a tela sem a API, abra pratos.html?mock=1 (dados só em memória).

const endpoint = `${config.apiBaseUrl.replace(/\/+$/, "")}/api/pratos`;
const usarMock = new URLSearchParams(window.location.search).has("mock");

export class ErroApi extends Error {
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

function enviar(url, method, prato) {
  return requisitar(url, {
    method,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      nome: prato.nome,
      preco: prato.preco,
      disponivel: prato.disponivel,
      categoriaId: prato.categoriaId,
    }),
  });
}

const apiReal = {
  listar: () => requisitar(endpoint),
  criar: (prato) => enviar(endpoint, "POST", prato),
  atualizar: (id, prato) => enviar(`${endpoint}/${id}`, "PUT", prato),
  excluir: (id) => requisitar(`${endpoint}/${id}`, { method: "DELETE" }),
};

// --- Mock em memória (somente com ?mock=1) -------------------------------------------------
function criarMock() {
  let proximoId = 4;
  let pratos = [
    { id: 1, nome: "Feijoada completa", preco: 32.9, disponivel: true, categoriaId: 1 },
    { id: 2, nome: "Frango grelhado com legumes", preco: 27.5, disponivel: true, categoriaId: 1 },
    { id: 3, nome: "Pudim de leite", preco: 9, disponivel: false, categoriaId: 2 },
  ];
  const espera = () => new Promise((resolver) => setTimeout(resolver, 250));
  const duplicado = (prato, ignorarId) =>
    pratos.some(
      (p) =>
        p.id !== ignorarId &&
        p.categoriaId === prato.categoriaId &&
        p.nome.toLowerCase() === prato.nome.toLowerCase(),
    );

  return {
    async listar() {
      await espera();
      return structuredClone(pratos);
    },
    async criar(prato) {
      await espera();
      if (duplicado(prato)) throw new ErroApi("Já existe um prato com esse nome nesta categoria.", "nome");
      const novo = { ...prato, id: proximoId++ };
      pratos.push(novo);
      return structuredClone(novo);
    },
    async atualizar(id, prato) {
      await espera();
      if (duplicado(prato, id)) throw new ErroApi("Já existe um prato com esse nome nesta categoria.", "nome");
      const indice = pratos.findIndex((p) => p.id === id);
      if (indice < 0) throw new ErroApi("Prato não encontrado.");
      pratos[indice] = { ...prato, id };
      return structuredClone(pratos[indice]);
    },
    async excluir(id) {
      await espera();
      pratos = pratos.filter((p) => p.id !== id);
    },
  };
}

export const apiPratos = usarMock ? criarMock() : apiReal;
export const modoMock = usarMock;
