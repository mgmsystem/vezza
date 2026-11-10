# Configuração do Vezza

## Pré-requisitos e banco

- SDK .NET 10
- MySQL 8.4
- Um banco vazio e exclusivo para a aplicação

Crie as tabelas executando `Vezza.Infra/Database/Scripts/CriacaoTabelas.sql`. `MigracaoSegurancaAutorizacao.sql` atualiza instalações antigas; uma instalação nova precisa apenas do script de criação.

## Desenvolvimento com User Secrets

O projeto não contém credenciais padrão. Configure valores locais fora do repositório:

```powershell
dotnet user-secrets set "ConnectionStrings:connectionString" "Server=127.0.0.1;Port=3306;Database=vezza;User=<usuario-local>;Password=<senha-local>;SslMode=Disabled" --project Vezza.Api
dotnet user-secrets set "JwtKey" "<chave-longa-e-exclusiva-para-desenvolvimento>" --project Vezza.Api
dotnet user-secrets set "Smtp:Host" "<smtp-host>" --project Vezza.Api
dotnet user-secrets set "Smtp:Port" "587" --project Vezza.Api
dotnet user-secrets set "Smtp:Username" "<smtp-username>" --project Vezza.Api
dotnet user-secrets set "Smtp:Password" "<smtp-password>" --project Vezza.Api
dotnet user-secrets set "Smtp:SenderEmail" "<sender-email>" --project Vezza.Api
dotnet user-secrets set "Smtp:SenderDisplayName" "Vezza" --project Vezza.Api
dotnet user-secrets set "Smtp:EnableSsl" "true" --project Vezza.Api
```

`SslMode=Disabled` é apropriado somente para MySQL local em uma interface confiável. Em ambientes remotos, configure TLS. A `JwtKey` deve ser longa, aleatória e exclusiva por ambiente. SMTP é necessário para o envio real do código de recuperação.

## Variáveis de ambiente

Em outros ambientes, use um cofre de segredos ou variáveis. Chaves hierárquicas usam dois sublinhados:

- `ConnectionStrings__connectionString` e `JwtKey` (obrigatórias)
- `JAWSDB_URL` pode substituir `ConnectionStrings__connectionString` no Heroku
- `Database__SslMode` e `Database__MaximumPoolSize` controlam TLS e pool do MySQL remoto
- `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__SenderEmail`, `Smtp__SenderDisplayName` e `Smtp__EnableSsl`
- `Security__PublicEndpointRateLimit__PermitLimit` e `Security__PublicEndpointRateLimit__WindowSeconds`
- `Security__PublicRegistrationEnabled` habilita o bootstrap do primeiro usuário; mantenha `false` em produção após o cadastro
- `DemoMode__Enabled` ativa as proteções do ambiente demonstrativo
- `DemoMode__Email` identifica a conta demo pelo claim de e-mail do JWT
- `ApiKeyName` e `ApiKey` são legadas e atualmente não protegem endpoints

`Vezza.Api/appsettings.Example.json` é apenas uma referência estrutural. Não grave segredos em arquivos versionados.

## Compilar e executar

```powershell
dotnet restore Vezza.sln
dotnet build Vezza.sln --configuration Release
dotnet run --project Vezza.Api --launch-profile http
```

Swagger fica disponível em `/swagger` somente com `ASPNETCORE_ENVIRONMENT=Development`; não é exposto automaticamente em `Production`.

Para o container integrado e o JawsDB, consulte [HEROKU.md](HEROKU.md).
