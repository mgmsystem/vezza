# Vezza.Web

Frontend independente do Vezza, construído com React, TypeScript e Vite.

## Execução local

Inicie a API em `http://localhost:5288` e, em outro terminal:

```powershell
cd Vezza.Web
npm install
npm run dev
```

O Vite encaminha `/v1` e `/health-check` para a API, evitando a necessidade de CORS no desenvolvimento. Para outro endereço, copie `.env.example` para `.env.local` e ajuste `VITE_API_PROXY_TARGET`.

Em produção, defina `VITE_API_BASE_URL` com a URL HTTPS da API. O backend deverá permitir explicitamente apenas a origem do frontend; não use CORS irrestrito.

## Validação

```powershell
npm run typecheck
npm run build
npm run test
```

Os testes usam Vitest, React Testing Library e requisições HTTP simuladas. Eles não acessam a API ou o banco de dados reais.
