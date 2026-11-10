# Arquitetura do Vezza

O Vezza é uma aplicação full stack organizada em um backend ASP.NET Core em camadas e um frontend React independente. A solução usa uma abordagem CQRS pragmática para operações de escrita; não pretende implementar DDD completo, microsserviços ou arquitetura hexagonal.

## Backend

- **Vezza.Api** compõe a aplicação, registra dependências e expõe controllers, autenticação JWT, rate limiting, Swagger em desenvolvimento e o health check.
- **Vezza.Domain** contém entidades, Commands, Handlers, serviços de domínio, resultados e contratos de repositório.
- **Vezza.Infra** implementa acesso ao MySQL, mapeamentos EF Core, repositórios, consultas Dapper e o Unit of Work.
- **Vezza.Shared** reúne validações e utilitários compartilhados.
- **Vezza.Tests** contém testes unitários, de comportamento, contrato, inicialização e integração.

O fluxo típico de escrita é:

```text
Requisição HTTP → Controller → Command → Handler → Service/Repository → MySQL
                                      ↓
                                GenericResult
```

Controllers traduzem HTTP para Commands e respostas. Handlers validam a operação, coordenam serviços e repositórios e retornam `GenericResult`. Os repositórios encapsulam a persistência.

EF Core é usado no mapeamento e nas operações orientadas a entidades. Dapper atende SQL específico e parametrizado. Em operações compostas, como a exclusão de um orçamento e seus registros relacionados, o `UnitOfWork` controla commit e rollback; o Dapper reutiliza a conexão e a transação do contexto EF Core.

Os endpoints de negócio exigem Bearer JWT. Autenticação, registro e recuperação de acesso possuem endpoints públicos sujeitos a limitação de requisições. O modelo atual não implementa papéis, permissões granulares ou isolamento de dados por usuário.

## Frontend

`Vezza.Web` é uma aplicação React com TypeScript e Vite. React Router protege e organiza as rotas. TanStack Query gerencia consultas, mutações, cache e invalidações. Um cliente baseado em `fetch` centraliza o envelope `GenericResult`, o token Bearer e o tratamento de HTTP 401 e 429.

O token fica em memória e em `sessionStorage` durante a sessão. Logout, expiração e HTTP 401 removem a sessão; o cache de consultas é limpo para não reutilizar dados entre usuários.

Em desenvolvimento, o proxy do Vite encaminha `/v1` e `/health-check` à API local. Em produção, `VITE_API_BASE_URL` deve apontar para a API HTTPS, que precisa autorizar explicitamente a origem do frontend.

## Testes

O backend usa MSTest e `WebApplicationFactory`. Testes que dependem de MySQL real são categorizados como integração e possuem execução documentada separadamente.

O frontend usa Vitest, React Testing Library e jsdom. Requisições HTTP são simuladas nos testes; nenhum teste automatizado do frontend altera o banco real.
