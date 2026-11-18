import { apiCategorias } from "./api-categorias.js";
import { config } from "./config.js";

const lista = document.getElementById("categorias");
const modelo = document.getElementById("modelo-categoria");
const tituloLista = document.getElementById("titulo-lista");
const total = document.getElementById("total-categorias");
const carregando = document.getElementById("carregando");
const listaVazia = document.getElementById("lista-vazia");
const erroLista = document.getElementById("erro-lista");
const mensagemErroLista = document.getElementById("mensagem-erro-lista");
const tentarNovamente = document.getElementById("tentar-novamente");
const novaCategoria = document.getElementById("nova-categoria");
const mensagem = document.getElementById("mensagem");

const dialogoCategoria = document.getElementById("dialogo-categoria");
const formularioCategoria = document.getElementById("formulario-categoria");
const tituloFormulario = document.getElementById("titulo-formulario");
const campoNome = document.getElementById("nome");
const erroFormulario = document.getElementById("erro-formulario");

const dialogoExclusao = document.getElementById("dialogo-exclusao");
const formularioExclusao = document.getElementById("formulario-exclusao");
const nomeExclusao = document.getElementById("nome-exclusao");
const erroExclusao = document.getElementById("erro-exclusao");

const ordenarNomes = new Intl.Collator("pt-BR", { sensitivity: "base" });
let categorias = [];
let categoriaEmEdicao = null;
let categoriaEmExclusao = null;

function criarLinha(categoria) {
  const linha = modelo.content.firstElementChild.cloneNode(true);
  linha.dataset.categoriaId = categoria.id;
  linha.querySelector(".categoria-nome").textContent = categoria.nome;
  linha.querySelector(".categoria-pratos").textContent =
    categoria.quantidadePratos === 1 ? "1 prato" : `${categoria.quantidadePratos} pratos`;

  const verPratos = linha.querySelector(".ver-pratos");
  const urlPratos = new URL(config.paginaPratos, window.location.href);
  urlPratos.searchParams.set("categoriaId", categoria.id);
  verPratos.href = urlPratos.href;
  verPratos.setAttribute("aria-label", `Ver pratos de ${categoria.nome}`);

  const editar = linha.querySelector(".editar");
  editar.setAttribute("aria-label", `Editar ${categoria.nome}`);
  editar.addEventListener("click", () => abrirFormulario(categoria));

  const excluir = linha.querySelector(".excluir");
  excluir.setAttribute("aria-label", `Excluir ${categoria.nome}`);
  excluir.addEventListener("click", () => abrirExclusao(categoria));

  return linha;
}

function renderizarLista() {
  categorias.sort((a, b) => ordenarNomes.compare(a.nome, b.nome));
  lista.replaceChildren(...categorias.map(criarLinha));

  const quantidade = categorias.length;
  total.textContent = quantidade === 1 ? "1 categoria" : `${quantidade} categorias`;
  total.hidden = false;
  lista.hidden = quantidade === 0;
  listaVazia.hidden = quantidade > 0;
}

async function carregarCategorias() {
  erroLista.hidden = true;
  total.hidden = true;
  lista.hidden = true;
  listaVazia.hidden = true;
  carregando.hidden = false;
  novaCategoria.disabled = true;

  try {
    const dados = await apiCategorias.listar();
    if (!Array.isArray(dados)) {
      throw new Error("A API retornou uma lista de categorias inválida.");
    }

    categorias = dados;
    renderizarLista();
    novaCategoria.disabled = false;
  } catch (erro) {
    erroLista.hidden = false;
    mensagemErroLista.textContent = erro.message;
  } finally {
    carregando.hidden = true;
  }
}

function limparErroFormulario() {
  erroFormulario.textContent = "";
  campoNome.removeAttribute("aria-invalid");
}

function mostrarErroFormulario(mensagemErro, campo) {
  erroFormulario.textContent = mensagemErro;
  if (campo === "nome") {
    campoNome.setAttribute("aria-invalid", "true");
    campoNome.focus();
  }
}

function abrirFormulario(categoria = null) {
  categoriaEmEdicao = categoria;
  mensagem.textContent = "";
  limparErroFormulario();
  tituloFormulario.textContent = categoria ? "Editar categoria" : "Nova categoria";
  campoNome.value = categoria?.nome ?? "";
  dialogoCategoria.showModal();
}

function abrirExclusao(categoria) {
  categoriaEmExclusao = categoria;
  mensagem.textContent = "";
  nomeExclusao.textContent = categoria.nome;
  erroExclusao.textContent = "";
  dialogoExclusao.showModal();
}

function definirEnvio(formulario, enviando, texto) {
  formulario.setAttribute("aria-busy", String(enviando));
  formulario.querySelectorAll("button").forEach((botao) => {
    botao.disabled = enviando;
  });
  formulario.querySelector('button[type="submit"]').textContent = texto;
}

formularioCategoria.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  limparErroFormulario();

  const nome = campoNome.value.trim().replace(/\s+/g, " ");
  if (!nome) {
    mostrarErroFormulario("Informe o nome da categoria.", "nome");
    return;
  }
  if (nome.length < 2 || nome.length > 80) {
    mostrarErroFormulario("O nome da categoria deve ter entre 2 e 80 caracteres.", "nome");
    return;
  }

  const id = categoriaEmEdicao?.id;
  campoNome.readOnly = true;
  definirEnvio(formularioCategoria, true, "Salvando…");
  try {
    const categoria = id === undefined
      ? await apiCategorias.criar(nome)
      : await apiCategorias.atualizar(id, nome);

    categorias = [...categorias.filter((item) => item.id !== categoria.id), categoria];
    renderizarLista();
    dialogoCategoria.close();
    mensagem.textContent = id === undefined ? "Categoria cadastrada." : "Categoria atualizada.";
    if (id === undefined) {
      novaCategoria.focus();
    } else {
      lista.querySelector(`[data-categoria-id="${id}"] .editar`)?.focus();
    }
  } catch (erro) {
    mostrarErroFormulario(erro.message, erro.campo);
  } finally {
    campoNome.readOnly = false;
    definirEnvio(formularioCategoria, false, "Salvar categoria");
  }
});

formularioExclusao.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  erroExclusao.textContent = "";
  definirEnvio(formularioExclusao, true, "Excluindo…");

  try {
    await apiCategorias.excluir(categoriaEmExclusao.id);
    categorias = categorias.filter((categoria) => categoria.id !== categoriaEmExclusao.id);
    renderizarLista();
    dialogoExclusao.close();
    mensagem.textContent = "Categoria excluída.";
    tituloLista.focus();
  } catch (erro) {
    erroExclusao.textContent = erro.message;
  } finally {
    definirEnvio(formularioExclusao, false, "Excluir categoria");
  }
});

for (const dialogo of [dialogoCategoria, dialogoExclusao]) {
  dialogo.querySelector("[data-cancelar]").addEventListener("click", () => dialogo.close());
  dialogo.addEventListener("cancel", (evento) => {
    if (dialogo.querySelector('form[aria-busy="true"]')) {
      evento.preventDefault();
    }
  });
}

campoNome.addEventListener("input", limparErroFormulario);
novaCategoria.addEventListener("click", () => abrirFormulario());
tentarNovamente.addEventListener("click", async () => {
  await carregarCategorias();
  if (erroLista.hidden) {
    tituloLista.focus();
  } else {
    tentarNovamente.focus();
  }
});

carregarCategorias();
