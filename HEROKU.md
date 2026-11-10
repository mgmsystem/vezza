# Publicação no Heroku com JawsDB

Este guia prepara um único aplicativo Heroku: o container compila o React, publica o ASP.NET Core e copia os arquivos estáticos para o `wwwroot` da API. O navegador e a API usam a mesma origem, portanto `VITE_API_BASE_URL` permanece vazio no build de produção.

Nenhum dos comandos abaixo deve ser executado sem uma conta, um aplicativo e um plano previamente aprovados. Substitua todos os valores entre `<...>`.

## 1. Pré-requisitos

- Heroku CLI autenticado.
- Docker em execução.
- Cliente MySQL 8 para inicialização manual do esquema.
- Um nome de aplicativo Heroku disponível.
- Um plano JawsDB escolhido após conferir preço, armazenamento, conexões, limite de consultas e retenção de backup vigentes.

## 2. Criar os recursos

```powershell
heroku create <nome-do-app>
heroku stack:set container --app <nome-do-app>
heroku addons:create jawsdb:<plano-escolhido> --app <nome-do-app>
```

Espere o add-on ficar disponível. `JAWSDB_URL` inicialmente pode conter um placeholder:

```powershell
heroku config:get JAWSDB_URL --app <nome-do-app>
```

Não copie essa URL para arquivos versionados.

## 3. Configurar o runtime

```powershell
heroku config:set ASPNETCORE_ENVIRONMENT=Production --app <nome-do-app>
heroku config:set JwtKey="<chave-aleatoria-longa>" --app <nome-do-app>
heroku config:set Database__SslMode=Required --app <nome-do-app>
heroku config:set Database__MaximumPoolSize=5 --app <nome-do-app>
heroku config:set DemoMode__Enabled=true --app <nome-do-app>
heroku config:set DemoMode__Email=demo@vezza.app --app <nome-do-app>
heroku config:set Security__PublicRegistrationEnabled=false --app <nome-do-app>
```

O Heroku fornece `PORT` automaticamente. A aplicação lê `JAWSDB_URL`, converte a URL para o formato aceito pelo provider MySQL e usa pool pequeno. `SslMode=Required` exige criptografia em trânsito, mas não substitui validação de CA. Se o painel do JawsDB fornecer uma CA compatível com o provider e o plano, prefira `VerifyCA` ou `VerifyFull` após validar a configuração.

Configure `Smtp__*` apenas se a recuperação de acesso por e-mail for demonstrada. Valores sensíveis devem permanecer em Config Vars.

### Criar o proprietário e a conta demo

Em uma instalação sem usuários, habilite temporariamente o bootstrap:

```powershell
heroku config:set Security__PublicRegistrationEnabled=true --app vezza
```

Envie uma única requisição HTTPS para `POST /v1/usuarios/registrar` com o nome, e-mail e senha forte do proprietário. O backend recusa o autorregistro se já existir qualquer usuário. Confirme o login e desabilite novamente:

```powershell
heroku config:set Security__PublicRegistrationEnabled=false --app vezza
```

Autenticado como proprietário, crie a conta demonstrativa por `POST /v1/usuarios`, usando exatamente o e-mail configurado em `DemoMode__Email` e as credenciais públicas documentadas no README. Esse fluxo aplica o mesmo `PasswordHasher` usado em qualquer cadastro; não insira hash ou senha manualmente no Adminer.

Não altere o e-mail da conta diretamente no banco. O modo demo só reconhece a identidade quando o claim do JWT coincide com `DemoMode__Email`.

## 4. Criar o esquema uma única vez

`Vezza.Infra/Database/Scripts/CriacaoTabelas.sql` contém apenas `CREATE TABLE`, sem `DROP`, `TRUNCATE` ou carga de dados. O script não é idempotente: execute somente em um banco novo e vazio.

No PowerShell, com o cliente `mysql` instalado:

```powershell
$jawsUrl = heroku config:get JAWSDB_URL --app <nome-do-app>
$uri = [Uri]$jawsUrl
$credentials = $uri.UserInfo.Split(':', 2)
$mysqlUser = [Uri]::UnescapeDataString($credentials[0])
$env:MYSQL_PWD = [Uri]::UnescapeDataString($credentials[1])
$database = [Uri]::UnescapeDataString($uri.AbsolutePath.TrimStart('/'))

Get-Content -Raw Vezza.Infra/Database/Scripts/CriacaoTabelas.sql |
  mysql --ssl-mode=REQUIRED --host=$($uri.Host) --port=$($uri.Port) --user=$mysqlUser --database=$database

Remove-Item Env:MYSQL_PWD
```

Antes de executar, confirme que o banco não contém tabelas. Depois, verifique as sete tabelas esperadas. Não coloque esse script em uma release phase enquanto ele não for transformado em migração idempotente e controlada.

## 5. Construir e validar o container

```powershell
docker build --tag vezza:local .
docker run --rm --publish 8080:8080 `
  --env PORT=8080 `
  --env ASPNETCORE_ENVIRONMENT=Production `
  --env ConnectionStrings__connectionString="<conexao-mysql-local>" `
  --env JwtKey="<chave-local-de-teste>" `
  vezza:local
```

Valide:

- `http://localhost:8080/health-check`
- `http://localhost:8080/login`
- atualização direta em `http://localhost:8080/app/dashboard`
- login e consultas com dados exclusivamente fictícios

## 6. Publicação manual de contingência

O fluxo normal de produção é o GitHub Actions descrito na seção seguinte. A publicação manual deve ser usada apenas para diagnóstico ou contingência, depois que o mesmo commit tiver passado pelo CI.

```powershell
heroku container:login
heroku container:push web --app <nome-do-app>
heroku container:release web --app <nome-do-app>
heroku ps:scale web=1 --app <nome-do-app>
heroku open --app <nome-do-app>
heroku logs --tail --app <nome-do-app>
```

O `Dockerfile` é o contrato de build. Alterações em variáveis `VITE_*` exigiriam novo build do frontend; neste modelo de mesma origem elas não são necessárias.

## 7. CI/CD pelo GitHub Actions

O workflow `.github/workflows/dotnetCI.yml` continua responsável pelos jobs independentes `backend` e `frontend`. Em um `push` para `main`, o job `deploy` só chama `.github/workflows/dotnetCD.yml` depois que ambos terminam com sucesso. Pull requests executam apenas o CI e nunca recebem acesso ao segredo de produção.

Essa chamada reutilizável foi escolhida em vez de `workflow_run` para manter testes e publicação no mesmo evento e no mesmo SHA. Também evita que um workflow privilegiado faça checkout de artefatos ou código originado de uma execução diferente.

### Segredo obrigatório

No repositório do GitHub, abra **Settings → Secrets and variables → Actions → New repository secret** e crie:

```text
HEROKU_API_KEY
```

O valor deve ser um token de uma conta autorizada a publicar somente os aplicativos necessários. Não use `JAWSDB_URL`, `JwtKey` ou qualquer credencial da aplicação como GitHub Secret: esses valores permanecem nas Config Vars do Heroku.

O `GITHUB_TOKEN` recebe apenas `contents: read`. O checkout não persiste suas credenciais.

Nas regras da branch `main`, mantenha os checks dos jobs `backend` e `frontend` como obrigatórios antes de merge. Também confirme em **Settings → Actions → General** que workflows locais reutilizáveis são permitidos. O workflow de CD não possui `workflow_dispatch` e não pode ser iniciado isoladamente pela interface.

### Compatibilidade da imagem

O CD não usa o formato padrão implícito do Docker Desktop. Ele cria um builder isolado e publica diretamente no registry com:

- plataforma única `linux/amd64`, única arquitetura aceita pelo Heroku Container Runtime;
- `oci-mediatypes=false`, produzindo Docker Image Manifest V2 Schema 2;
- provenance e SBOM desabilitados para não criar índices ou artefatos auxiliares;
- camadas gzip;
- metadados gravados apenas no diretório temporário do runner.

Antes do release, o workflow exige o media type abaixo tanto nos metadados do Buildx quanto no manifesto lido de volta do registry:

```text
application/vnd.docker.distribution.manifest.v2+json
```

Se o push voltar a produzir `error from registry: unsupported`, se o manifesto estiver ausente ou se o media type for OCI, o workflow falha antes de alterar a formação do aplicativo. Nesse caso, preserve a release atual e use como alternativa uma publicação por `heroku.yml`, na qual a imagem é construída pelo sistema do Heroku, depois de validar esse método em uma alteração separada. Não remova a verificação para forçar o release.

Como esta preparação não realiza deploy, a primeira execução em `main` ainda é a homologação real do registry. Considere o pipeline aprovado somente quando o manifesto remoto for confirmado e uma release saudável aparecer no Heroku.

### Concorrência e banco

O grupo de concorrência `heroku-vezza-production` permite somente um deploy por vez. Execuções anteriores não são canceladas durante uma publicação. O workflow não executa migrations, scripts SQL, comandos JawsDB nem alterações em Config Vars.

### Acompanhamento

```powershell
heroku releases --app vezza
heroku releases:info --app vezza
heroku ps --app vezza
heroku logs --tail --app vezza
```

Após o primeiro deploy, valide `/health-check`, `/login`, uma rota profunda de `/app` e o login com a conta fictícia.

### Rollback

Identifique primeiro a release estável:

```powershell
heroku releases --app vezza
heroku rollback v<numero-estavel> --app vezza
```

O rollback cria uma nova release com a imagem e as Config Vars da release escolhida. Ele não desfaz alterações no JawsDB. Como o CD não modifica o banco, não há rollback SQL automático.

Depois do rollback emergencial, reverta ou corrija o commit no Git e deixe o pipeline publicar uma nova versão rastreável.

## 8. Checklist público

- Usar somente dados e conta de demonstração fictícios.
- Não reutilizar a chave JWT ou senhas de outros ambientes.
- Confirmar HTTPS no domínio Heroku.
- Testar cadastro, edição e exclusão apenas sobre dados descartáveis.
- Monitorar logs sem registrar tokens ou a URL do banco.
- Acompanhar armazenamento e `max_questions` do plano JawsDB.
- Manter backup antes de reset ou mudança de plano.
- Planejar restauração periódica dos dados de demonstração fora do processo web.
- Considerar que todos os usuários autenticados compartilham os mesmos registros; não há papéis nem isolamento por usuário.
