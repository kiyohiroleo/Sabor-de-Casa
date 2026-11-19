# Integração com a API .NET de categorias

## Configurar o endereço da API

Fora de localhost, o frontend usa por padrão a mesma origem do site. Se a API estiver em outra origem, informe a URL base em `apiBaseUrl` em [config.js](./config.js), sem o sufixo `/api`, e configure o CORS no backend para aceitar a origem do frontend.

## Rotas consumidas

| Operação | Método e rota |
| --- | --- |
| Listar | `GET /api/categorias` |
| Cadastrar | `POST /api/categorias` |
| Editar | `PUT /api/categorias/{id}` |
| Excluir | `DELETE /api/categorias/{id}` |

O cadastro e a edição enviam um JSON como `{ "nome": "Marmitas executivas" }`. O frontend usa `id`, `nome` e `quantidadePratos` nas respostas de categoria e exibe as mensagens de erro retornadas pela API.
