# Vezza

O Vezza é uma aplicação full stack para gestão de clientes, obras, orçamentos e itens de orçamento. O projeto foi construído como portfólio técnico, com backend ASP.NET Core em camadas, frontend React independente, autenticação JWT, persistência MySQL e testes automatizados.

```text
Cliente → Obra → Orçamento → Itens do orçamento
```

## Funcionalidades

### Aplicação web

- Login integrado à API e sessão armazenada em `sessionStorage`.
- Rotas protegidas, logout e tratamento de sessão expirada, HTTP 401 e HTTP 429.
- Dashboard com indicadores reais, distribuição por situação e orçamentos recentes.
- CRUD de clientes.
- CRUD de obras vinculadas a clientes.
- CRUD de orçamentos vinculados a obras.
- Gerenciamento de itens dentro do orçamento.
- Pesquisa local, estados de carregamento, vazio e erro e confirmações de exclusão.
- Layout administrativo responsivo para desktop e dispositivos móveis.

### API

- Endpoints autenticados para usuários, clientes, obras, orçamentos, itens e fluxos financeiros.
- Registro, autenticação e recuperação de acesso.
- Respostas padronizadas por `GenericResult`.
- Rate limiting nos endpoints públicos sensíveis.
- Transações em operações compostas, incluindo a exclusão de orçamento e registros relacionados.
- Swagger/OpenAPI disponível somente em desenvolvimento.
- Health check em `/health-check`.

## Tecnologias

**Backend:** C#, .NET 10, ASP.NET Core Web API, Entity Framework Core, Dapper, MySQL, JWT Bearer, MSTest e `WebApplicationFactory`.

**Frontend:** React 19, TypeScript, Vite, React Router, TanStack Query, Vitest e React Testing Library.

**Infraestrutura:** Docker Compose para MySQL de integração, container multi-stage para produção e GitHub Actions para CI de backend/frontend e CD controlado no Heroku.

## Organização

| Projeto | Responsabilidade |
| --- | --- |
| `Vezza.Api` | Controllers, composição, autenticação, rate limiting e OpenAPI |
| `Vezza.Domain` | Entidades, Commands, Handlers, serviços e contratos de repositório |
| `Vezza.Infra` | EF Core, Dapper, MySQL, repositórios, mapeamentos e Unit of Work |
| `Vezza.Shared` | Validações e utilitários compartilhados |
| `Vezza.Tests` | Testes automatizados do backend |
| `Vezza.Web` | Aplicação React independente e testes do frontend |

O fluxo principal do backend é `Controller → Command → Handler → Service/Repository`. Trata-se de CQRS pragmático com separação de responsabilidades, não de uma implementação completa de DDD ou de microsserviços. Consulte [ARCHITECTURE.md](ARCHITECTURE.md) para detalhes.

## Pré-requisitos

- SDK .NET 10.
- Node.js 22 ou versão LTS compatível com o Vite utilizado.
- MySQL 8.4.
- Docker com Compose, opcional para os testes de integração.

## Configuração do backend

Crie um banco vazio e execute:

```text
Vezza.Infra/Database/Scripts/CriacaoTabelas.sql
```

Configure segredos locais com .NET User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:connectionString" "Server=127.0.0.1;Port=3306;Database=vezza;User=<usuario>;Password=<senha>;SslMode=Disabled" --project Vezza.Api
dotnet user-secrets set "JwtKey" "<chave-longa-aleatoria-e-exclusiva>" --project Vezza.Api
```

O SMTP é necessário somente para envio real de códigos de recuperação. A lista completa de configurações está em [CONFIGURATION.md](CONFIGURATION.md). `Vezza.Api/appsettings.Example.json` contém apenas a estrutura segura, sem credenciais.

Variáveis obrigatórias fora de desenvolvimento:

| Variável | Finalidade |
| --- | --- |
| `ConnectionStrings__connectionString` | Conexão MySQL |
| `JwtKey` | Assinatura dos tokens JWT |
| `Smtp__*` | Envio de recuperação de acesso, quando utilizado |

## Executar localmente

### Backend

```powershell
dotnet restore Vezza.sln
dotnet build Vezza.sln --configuration Release
dotnet run --project Vezza.Api --launch-profile Vezza.Api
```

A API usa `https://localhost:7094` e `http://localhost:5288` no perfil local. Em `Development`, o Swagger fica disponível em `/swagger`.

### Frontend

Em outro terminal:

```powershell
cd Vezza.Web
npm ci
npm run dev
```

O proxy do Vite encaminha `/v1` e `/health-check` para a API. Para alterar o destino local:

```powershell
Copy-Item .env.example .env.local
```

```dotenv
VITE_API_PROXY_TARGET=https://localhost:7094
```

No container integrado do Heroku, mantenha `VITE_API_BASE_URL` vazio para que React e API usem a mesma origem. Em uma eventual publicação separada do frontend, configure-o com a URL HTTPS pública da API. Nunca coloque segredos em variáveis `VITE_*`, pois elas são incorporadas ao bundle do navegador.

## Testes e validação

Backend sem dependência externa:

```powershell
dotnet test Vezza.sln --configuration Release --filter "TestCategory!=Integration"
```

Frontend:

```powershell
cd Vezza.Web
npm run typecheck
npm run test
npm run build
```

Os testes do frontend simulam as respostas HTTP e não acessam o banco. Os testes MySQL usam uma instância isolada; consulte [INTEGRATION_TESTS.md](INTEGRATION_TESTS.md).

## Decisões técnicas

- EF Core para persistência orientada a entidades e Dapper para SQL específico e parametrizado.
- Unit of Work apenas onde operações compostas precisam compartilhar transação.
- Cliente HTTP centralizado no frontend, sem Redux ou biblioteca adicional de formulários.
- TanStack Query para cache, mutações e invalidações.
- Proxy Vite no desenvolvimento para evitar CORS irrestrito.
- Valor comercial do orçamento independente da soma informativa dos itens.

## Limitações conhecidas

- Não há papéis, permissões granulares ou isolamento de dados por usuário.
- Listagens da API não possuem paginação ou filtros; a pesquisa atual é local.
- A API não fornece indicadores agregados; o Dashboard calcula métricas a partir das listagens existentes.
- O envio de recuperação de acesso depende de SMTP externo.
- A implantação separada do frontend exigirá CORS restrito à origem publicada.
- `Vezza.Api/wwwroot` preserva um frontend AngularJS legado; a interface React está em `Vezza.Web`.
- Os testes de integração MySQL não são executados no workflow padrão de CI.

## Documentação

- [Arquitetura](ARCHITECTURE.md)
- [Configuração](CONFIGURATION.md)
- [Testes de integração](INTEGRATION_TESTS.md)
- [Frontend](Vezza.Web/README.md)
- [Publicação no Heroku com JawsDB](HEROKU.md)
