const ambienteLocal = ["localhost", "127.0.0.1", "[::1]"].includes(window.location.hostname);

export const config = {
  // Em publicação separada, use a URL pública da API sem o sufixo /api.
  apiBaseUrl: ambienteLocal ? "http://localhost:5269" : "https://sabor-de-casa-m2iu.onrender.com",
  paginaPratos: "./pratos.html",
};