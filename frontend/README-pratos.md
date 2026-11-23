# Pratos (Pessoa 3)

Página `pratos.html`: lista de pratos agrupada por categoria (nome, preço, categoria, disponibilidade), com cadastro, edição e exclusão.

## Arquivos
| Arquivo | Função |
| --- | --- |
| `pratos.html` | Estrutura da página, modelos e diálogos |
| `pratos.js` | Renderização, filtro, formulário e exclusão |
| `api-pratos.js` | Chamadas à API da Pessoa 4 (+ modo de teste `?mock=1`) |
| `validacao-pratos.js` | Regras de validação (sem DOM) |
| `pratos.css` | Só o que é específico de pratos; usa as variáveis e classes do `styles.css` |

## Como testar sem a API de pratos
Abra `pratos.html?mock=1` (precisa de `/api/categorias` rodando, ou veja abaixo). Os pratos são fictícios e ficam só em memória.

## Contrato esperado da API (combinar com a Pessoa 4)
| Operação | Rota | Corpo / resposta |
| --- | --- | --- |
| Listar | `GET /api/pratos` | `[ { id, nome, preco, disponivel, categoriaId } ]` |
| Cadastrar | `POST /api/pratos` | `{ nome, preco, disponivel, categoriaId }` → 201 + prato |
| Editar | `PUT /api/pratos/{id}` | mesmo corpo → 200 + prato |
| Excluir | `DELETE /api/pratos/{id}` | 200 ou 204 |

Erros no mesmo formato de categorias: `{ statusCode, codigo, erro, mensagem, campo }`, com `campo` = `"nome"`, `"preco"` ou `"categoriaId"` para o erro aparecer no input certo.
`preco` deve ser número (não string). Regras do banco refletidas no formulário: preço > 0, até 2 casas, máx. 99.999.999,99.

## Alinhamento com a Pessoa 1
- Nenhuma alteração no `styles.css`: `pratos.css` só adiciona classes novas (`.prato`, `.grupo`, `.selo`, `.campo`…).
- Falta só um link no menu do `index.html`, dentro do `<nav>`:
  `<a class="nav-link nav-link-inativo" href="./pratos.html">Pratos</a>`
  (e trocar `class="nav-link"` de Categorias por `nav-link nav-link-inativo` se quiserem o mesmo visual do `pratos.html`; precisa do `pratos.css` ou mover `.nav-lista`/`.nav-link-inativo` para o `styles.css`).
- O botão "Ver pratos" das categorias já leva a `pratos.html?categoriaId=ID`; a página abre filtrada.
