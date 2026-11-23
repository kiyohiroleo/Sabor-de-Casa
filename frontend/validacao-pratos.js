// Regras de validação do formulário de pratos (espelham o schema.sql:
// preco NUMERIC(10,2) > 0, nome NOT NULL, categoria_id NOT NULL).
// Módulo sem acesso ao DOM, para poder ser testado isoladamente.

export const NOME_MIN = 2;
export const NOME_MAX = 80;
export const PRECO_MAX = 99999999.99; // limite de NUMERIC(10,2)

export function normalizarNome(valor) {
  return String(valor ?? "").trim().replace(/\s+/g, " ");
}

// Aceita "12", "12,5", "12,50", "12.50", "1.234,56" e "R$ 12,50".
// Retorna número ou NaN quando o texto não é um valor monetário válido.
export function interpretarPreco(valor) {
  let texto = String(valor ?? "").replace(/R\$/gi, "").replace(/\s/g, "");
  if (!texto) return NaN;

  if (texto.includes(",")) {
    // vírgula decimal: pontos são separadores de milhar
    texto = texto.replace(/\./g, "").replace(",", ".");
  } else if (/^\d{1,3}(\.\d{3})+$/.test(texto)) {
    // "1.234" sem vírgula: milhar
    texto = texto.replace(/\./g, "");
  }

  if (!/^\d+(\.\d+)?$/.test(texto)) return NaN;
  return Number(texto);
}

// Retorna { valores, erros }. `erros` é um objeto { campo: mensagem } (vazio = válido).
export function validarPrato({ nome, preco, categoriaId }) {
  const erros = {};
  const nomeNormalizado = normalizarNome(nome);

  if (!nomeNormalizado) {
    erros.nome = "Informe o nome do prato.";
  } else if (nomeNormalizado.length < NOME_MIN || nomeNormalizado.length > NOME_MAX) {
    erros.nome = `O nome do prato deve ter entre ${NOME_MIN} e ${NOME_MAX} caracteres.`;
  }

  const precoNumero = interpretarPreco(preco);
  if (String(preco ?? "").trim() === "") {
    erros.preco = "Informe o preço do prato.";
  } else if (Number.isNaN(precoNumero)) {
    erros.preco = "Preço inválido. Use apenas números, por exemplo 25,90.";
  } else if (precoNumero <= 0) {
    erros.preco = "O preço deve ser maior que zero.";
  } else if (precoNumero > PRECO_MAX) {
    erros.preco = "O preço informado é alto demais.";
  } else if (Math.round(precoNumero * 100) / 100 !== precoNumero) {
    erros.preco = "Use no máximo 2 casas decimais no preço.";
  }

  const idNumero = Number(categoriaId);
  if (!categoriaId || !Number.isInteger(idNumero) || idNumero <= 0) {
    erros.categoriaId = "Selecione uma categoria.";
  }

  return {
    valores: { nome: nomeNormalizado, preco: precoNumero, categoriaId: idNumero },
    erros,
  };
}
