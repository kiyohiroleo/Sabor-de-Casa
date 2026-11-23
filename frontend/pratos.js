import { apiCategorias } from "./api-categorias.js";
import { apiPratos, modoMock } from "./api-pratos.js";
import { validarPrato } from "./validacao-pratos.js";

const ids = (...nomes) => nomes.map((n) => document.getElementById(n));
const [
  grupos, modeloGrupo, modeloPrato, tituloLista, total, carregando, erroLista, mensagemErroLista,
  tentarNovamente, semCategorias, novoPrato, mensagem, filtro, avisoMock,
] = ids(
  "grupos", "modelo-grupo", "modelo-prato", "titulo-lista", "total-pratos", "carregando", "erro-lista",
  "mensagem-erro-lista", "tentar-novamente", "sem-categorias", "novo-prato", "mensagem", "filtro-categoria",
  "aviso-mock",
);

const dialogoPrato = document.getElementById("dialogo-prato");
const formularioPrato = document.getElementById("formulario-prato");
const tituloFormulario = document.getElementById("titulo-formulario");
const campos = {
  nome: document.getElementById("nome"),
  preco: document.getElementById("preco"),
  categoriaId: document.getElementById("categoria"),
};
const errosCampos = {
  nome: document.getElementById("erro-nome"),
  preco: document.getElementById("erro-preco"),
  categoriaId: document.getElementById("erro-categoria"),
};
const campoDisponivel = document.getElementById("disponivel");
const erroFormulario = document.getElementById("erro-formulario");

const dialogoExclusao = document.getElementById("dialogo-exclusao");
const formularioExclusao = document.getElementById("formulario-exclusao");
const nomeExclusao = document.getElementById("nome-exclusao");
const erroExclusao = document.getElementById("erro-exclusao");

const ordenar = new Intl.Collator("pt-BR", { sensitivity: "base" });
const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

let categorias = [];
let pratos = [];
let pratoEmEdicao = null;
let pratoEmExclusao = null;

avisoMock.hidden = !modoMock;

// ---------- filtro por categoria (?categoriaId= vindo da tela de categorias) ----------
function categoriaDaUrl() {
  const valor = new URLSearchParams(window.location.search).get("categoriaId");
  return valor && /^\d+$/.test(valor) ? valor : "";
}

function atualizarUrl(categoriaId) {
  const url = new URL(window.location.href);
  if (categoriaId) url.searchParams.set("categoriaId", categoriaId);
  else url.searchParams.delete("categoriaId");
  window.history.replaceState(null, "", url);
}

// ---------- renderização ----------
function criarPrato(prato, nomeCategoria) {
  const item = modeloPrato.content.firstElementChild.cloneNode(true);
  item.dataset.pratoId = prato.id;
  item.querySelector(".prato-nome").textContent = prato.nome;
  item.querySelector(".prato-categoria").textContent = nomeCategoria;
  item.querySelector(".selo-indisponivel").hidden = prato.disponivel !== false;
  item.querySelector(".prato-preco").textContent = moeda.format(Number(prato.preco));
  if (prato.disponivel === false) item.classList.add("prato-indisponivel");

  const editar = item.querySelector(".editar");
  editar.setAttribute("aria-label", `Editar ${prato.nome}`);
  editar.addEventListener("click", () => abrirFormulario(prato));

  const excluir = item.querySelector(".excluir");
  excluir.setAttribute("aria-label", `Excluir ${prato.nome}`);
  excluir.addEventListener("click", () => abrirExclusao(prato));
  return item;
}

function criarGrupo(titulo, lista) {
  const grupo = modeloGrupo.content.firstElementChild.cloneNode(true);
  grupo.querySelector(".grupo-titulo").textContent = titulo;
  grupo.querySelector(".grupo-contagem").textContent = lista.length === 1 ? "1 prato" : `${lista.length} pratos`;
  grupo.querySelector(".grupo-vazio").hidden = lista.length > 0;
  const ul = grupo.querySelector(".pratos");
  ul.hidden = lista.length === 0;
  ul.replaceChildren(...lista.map((prato) => criarPrato(prato, titulo)));
  return grupo;
}

function renderizarLista() {
  const filtroId = filtro.value;
  const ordenados = [...pratos].sort((a, b) => ordenar.compare(a.nome, b.nome));
  const categoriasVisiveis = [...categorias]
    .filter((c) => !filtroId || String(c.id) === filtroId)
    .sort((a, b) => ordenar.compare(a.nome, b.nome));

  const elementos = categoriasVisiveis.map((categoria) =>
    criarGrupo(categoria.nome, ordenados.filter((p) => p.categoriaId === categoria.id)),
  );

  // pratos cuja categoria não existe mais na lista (evita sumirem da tela)
  if (!filtroId) {
    const conhecidos = new Set(categorias.map((c) => c.id));
    const orfaos = ordenados.filter((p) => !conhecidos.has(p.categoriaId));
    if (orfaos.length) elementos.push(criarGrupo("Sem categoria", orfaos));
  }

  grupos.replaceChildren(...elementos);
  grupos.hidden = false;

  const visiveis = filtroId ? pratos.filter((p) => String(p.categoriaId) === filtroId).length : pratos.length;
  total.textContent = visiveis === 1 ? "1 prato" : `${visiveis} pratos`;
  total.hidden = false;
}

function preencherSelects() {
  const ordenadas = [...categorias].sort((a, b) => ordenar.compare(a.nome, b.nome));
  const opcao = (c) => new Option(c.nome, c.id);
  const filtroAtual = filtro.value || categoriaDaUrl();

  filtro.replaceChildren(new Option("Todas", ""), ...ordenadas.map(opcao));
  filtro.value = ordenadas.some((c) => String(c.id) === filtroAtual) ? filtroAtual : "";
  atualizarUrl(filtro.value);

  campos.categoriaId.replaceChildren(new Option("Selecione…", ""), ...ordenadas.map(opcao));
}

async function carregar() {
  erroLista.hidden = true;
  semCategorias.hidden = true;
  grupos.hidden = true;
  total.hidden = true;
  carregando.hidden = false;
  novoPrato.disabled = true;
  filtro.disabled = true;

  try {
    const [dadosCategorias, dadosPratos] = await Promise.all([apiCategorias.listar(), apiPratos.listar()]);
    if (!Array.isArray(dadosCategorias) || !Array.isArray(dadosPratos)) {
      throw new Error("A API retornou uma lista inválida.");
    }
    categorias = dadosCategorias;
    pratos = dadosPratos;
    preencherSelects();

    if (categorias.length === 0) {
      semCategorias.hidden = false;
      return;
    }
    renderizarLista();
    novoPrato.disabled = false;
    filtro.disabled = false;
  } catch (erro) {
    erroLista.hidden = false;
    mensagemErroLista.textContent = erro.message;
  } finally {
    carregando.hidden = true;
  }
}

// ---------- formulário ----------
function limparErros() {
  erroFormulario.textContent = "";
  for (const campo of Object.keys(campos)) {
    errosCampos[campo].textContent = "";
    campos[campo].removeAttribute("aria-invalid");
  }
}

// Mostra erros por campo e foca o primeiro com problema. Erros sem campo conhecido vão para o aviso geral.
function mostrarErros(erros, mensagemGeral) {
  let primeiro = null;
  for (const [campo, texto] of Object.entries(erros)) {
    if (!campos[campo]) continue;
    errosCampos[campo].textContent = texto;
    campos[campo].setAttribute("aria-invalid", "true");
    primeiro ??= campos[campo];
  }
  erroFormulario.textContent = mensagemGeral ?? "";
  primeiro?.focus();
}

function abrirFormulario(prato = null) {
  pratoEmEdicao = prato;
  mensagem.textContent = "";
  limparErros();
  tituloFormulario.textContent = prato ? "Editar prato" : "Novo prato";
  campos.nome.value = prato?.nome ?? "";
  campos.preco.value = prato ? String(prato.preco).replace(".", ",") : "";
  campos.categoriaId.value = String(prato?.categoriaId ?? (filtro.value || ""));
  campoDisponivel.checked = prato ? prato.disponivel !== false : true;
  dialogoPrato.showModal();
}

function abrirExclusao(prato) {
  pratoEmExclusao = prato;
  mensagem.textContent = "";
  nomeExclusao.textContent = prato.nome;
  erroExclusao.textContent = "";
  dialogoExclusao.showModal();
}

function definirEnvio(formulario, enviando, texto) {
  formulario.setAttribute("aria-busy", String(enviando));
  formulario.querySelectorAll("button, input, select").forEach((el) => {
    el.disabled = enviando;
  });
  formulario.querySelector('button[type="submit"]').textContent = texto;
}

formularioPrato.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  limparErros();

  const { valores, erros } = validarPrato({
    nome: campos.nome.value,
    preco: campos.preco.value,
    categoriaId: campos.categoriaId.value,
  });
  if (Object.keys(erros).length > 0) {
    mostrarErros(erros);
    return;
  }

  const dados = { ...valores, disponivel: campoDisponivel.checked };
  const id = pratoEmEdicao?.id;
  definirEnvio(formularioPrato, true, "Salvando…");
  try {
    const salvo = id === undefined ? await apiPratos.criar(dados) : await apiPratos.atualizar(id, dados);
    pratos = [...pratos.filter((p) => p.id !== salvo.id), salvo];
    renderizarLista();
    dialogoPrato.close();
    mensagem.textContent = id === undefined ? "Prato cadastrado." : "Prato atualizado.";
    if (id === undefined) novoPrato.focus();
    else document.querySelector(`[data-prato-id="${salvo.id}"] .editar`)?.focus();
  } catch (erro) {
    // reabilita os campos antes de focar o primeiro com erro
    definirEnvio(formularioPrato, false, "Salvar prato");
    // `campo` vem da API; se for desconhecido a mensagem aparece no aviso geral
    if (campos[erro.campo]) mostrarErros({ [erro.campo]: erro.message });
    else mostrarErros({}, erro.message);
  } finally {
    definirEnvio(formularioPrato, false, "Salvar prato");
  }
});

formularioExclusao.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  erroExclusao.textContent = "";
  definirEnvio(formularioExclusao, true, "Excluindo…");
  try {
    await apiPratos.excluir(pratoEmExclusao.id);
    pratos = pratos.filter((p) => p.id !== pratoEmExclusao.id);
    renderizarLista();
    dialogoExclusao.close();
    mensagem.textContent = "Prato excluído.";
    tituloLista.focus();
  } catch (erro) {
    erroExclusao.textContent = erro.message;
  } finally {
    definirEnvio(formularioExclusao, false, "Excluir prato");
  }
});

for (const dialogo of [dialogoPrato, dialogoExclusao]) {
  dialogo.querySelector("[data-cancelar]").addEventListener("click", () => dialogo.close());
  dialogo.addEventListener("cancel", (evento) => {
    if (dialogo.querySelector('form[aria-busy="true"]')) evento.preventDefault();
  });
}

for (const campo of Object.keys(campos)) {
  campos[campo].addEventListener("input", () => {
    errosCampos[campo].textContent = "";
    campos[campo].removeAttribute("aria-invalid");
  });
}

filtro.addEventListener("change", () => {
  atualizarUrl(filtro.value);
  renderizarLista();
});
novoPrato.addEventListener("click", () => abrirFormulario());
tentarNovamente.addEventListener("click", async () => {
  await carregar();
  (erroLista.hidden ? tituloLista : tentarNovamente).focus();
});

carregar();
