# Testes de integração com MySQL

O ambiente usa o container `mysql:8.4.13`, com banco e credenciais deliberadamente fictícios e exclusivos de teste. A porta é publicada apenas em `127.0.0.1:33070`.

## Iniciar o banco

```powershell
docker compose -f docker-compose.integration.yml up -d --wait
$env:VEZZA_TEST_MYSQL_CONNECTION = "Server=127.0.0.1;Port=33070;Database=vezza_integration;User=vezza_test;Password=vezza_test_password;SslMode=Disabled;AllowPublicKeyRetrieval=True"
```

Não reutilize essas credenciais fora do container local. O script `CriacaoTabelas.sql` é aplicado automaticamente somente na criação inicial do volume.

## Executar os testes

```powershell
# Somente integração
dotnet test Vezza.sln --configuration Release --filter "TestCategory=Integration"

# Sem infraestrutura externa
dotnet test Vezza.sln --configuration Release --filter "TestCategory!=Integration"

# Suíte completa
dotnet test Vezza.sln --configuration Release
```

Sem `VEZZA_TEST_MYSQL_CONNECTION`, os testes de integração são inconclusivos e nenhum resultado de banco real é presumido. Os testes limpam tabelas e criam/removem estruturas auxiliares: nunca aponte a variável para um banco compartilhado, de desenvolvimento ou de produção.

## Encerrar e limpar

```powershell
Remove-Item Env:VEZZA_TEST_MYSQL_CONNECTION -ErrorAction SilentlyContinue
docker compose -f docker-compose.integration.yml down --volumes --remove-orphans
```

`--volumes` remove somente o volume do ambiente Compose e força a reaplicação do script na próxima inicialização.
