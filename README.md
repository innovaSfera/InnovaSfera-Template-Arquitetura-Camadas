# InnovaSfera Template

Este projeto é um template de arquitetura de camadas desenvolvido e licenciado pela InnovaSfera. Seu uso é restrito exclusivamente a funcionários da InnovaSfera.

## Licença
Este projeto é propriedade da InnovaSfera e está licenciado apenas para uso interno por colaboradores da empresa. O uso externo, distribuição ou modificação sem autorização é proibido.

## Visão Geral da Arquitetura
O projeto segue o padrão de arquitetura em camadas, promovendo separação de responsabilidades, escalabilidade e facilidade de manutenção. As principais camadas são:

- **Domain**: Contém as entidades de negócio, interfaces e serviços de domínio.
- **Application**: Implementa a lógica de aplicação, orquestrando operações entre as camadas e expondo serviços para o restante do sistema.
- **Infrastructure**: Responsável pela persistência de dados, configurações, IoC e integrações externas.
- **Presentation**: Camada de apresentação, normalmente uma API REST, responsável por expor os endpoints para consumo externo.
- **Tests**: Testes unitários e funcionais para garantir a qualidade e integridade do sistema.

### Detalhamento das Camadas

#### Domain (InnovaSfera.Template.Domain)
- **Entities/**: Entidades de domínio que representam os conceitos principais do negócio
- **Interfaces/**: Contratos para repositórios e serviços de domínio
- **Services/**: Implementações de lógica de negócio complexa

#### Application (InnovaSfera.Template.Application)
- **Entities/**: DTOs (Data Transfer Objects) para transferência de dados
- **Interfaces/**: Contratos dos serviços de aplicação
- **Mapper/**: Mapeamento entre entidades de domínio e DTOs
- **Services/**: Implementação dos serviços de aplicação

#### Infrastructure
- **Core**: Componentes base da infraestrutura
- **Data**: Contexto do Entity Framework, configurações e repositórios
- **IoC**: Configuração de injeção de dependência

#### Presentation (InnovaSfera.Template.Presentation.Api)
- **Controllers/**: Controladores da API REST
- **Properties/**: Configurações de propriedades da aplicação
- Arquivos de configuração (`appsettings.json`, `Program.cs`)

## Estrutura de Pastas
```
InnovaSfera.Template.sln
src/
  Application/
    InnovaSfera.Template.Application/
      Entities/
      Interfaces/
      Mapper/
      Services/
  Domain/
    InnovaSfera.Template.Domain/
      Entities/
      Interfaces/
      Services/
  Infrastructure/
    InnovaSfera.Template.Infrastructure.Core/
    InnovaSfera.Template.Infrastructure.Data/
    InnovaSfera.Template.Infrastructure.IoC/
  Presentation/
    InnovaSfera.Template.Presentation.Api/
      Controllers/
      Properties/
tests/
  FunctionalTests/
  UnitTests/
```

## Pacotes e Dependências
- **.NET 8.0**: Framework base do projeto
- **ASP.NET Core**: Framework web para criação de APIs REST
- **Entity Framework Core**: ORM para acesso a dados (In-Memory para desenvolvimento)
- **AutoMapper**: Mapeamento automático entre objetos
- **Polly**: Biblioteca para implementação de padrões de resiliência
- **StackExchange.Redis**: Cliente Redis para cache distribuído
- **Azure.Storage.Blobs**: SDK do Azure para Blob Storage
- **Microsoft.Extensions.Http.Polly**: Integração entre HttpClient e Polly
- **Newtonsoft.Json**: Serialização JSON
- **Swashbuckle.AspNetCore**: Documentação Swagger/OpenAPI

### Packages Principais

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.33" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
```

## Implementações Atuais

### Entidades de Exemplo
O template inclui implementações de exemplo que demonstram o padrão arquitetural:

#### SampleData (Domain Entity)
```csharp
public class SampleData : Entity
{
    public DateTime TimeStamp { get; set; }
    public required string Message { get; set; }
}
```

#### SampleDataDto (Application DTO)
```csharp
public class SampleDataDto
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
    public DateTime TimeStamp { get; set; }
}
```

### Pattern Strategy Implementation

O template implementa o padrão Strategy para duas funcionalidades principais:

#### Storage Strategy
Permite alternar entre diferentes tipos de armazenamento sem modificar o código cliente:

- **LocalStorageStrategy**: Armazenamento em sistema de arquivos local
- **AzureBlobStorageStrategy**: Armazenamento no Azure Blob Storage

**Configuração no appsettings.json:**
```json
{
  "Storage": {
    "Type": "local", // ou "azureblob"
    "ContainerName": "files"
  },
  "ConnectionStrings": {
    "AzureBlobStorage": "sua-connection-string-aqui"
  }
}
```

**Interface IStorageStrategy:**
```csharp
public interface IStorageStrategy
{
    Task<IEnumerable<string>> GetFilesAsync(string path);
    Task<byte[]> ReadFileAsync(string filePath);
    Task WriteFileAsync(string filePath, byte[] content);
    Task DeleteFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string StorageType { get; }
}
```

#### Cache Strategy
Permite alternar entre diferentes mecanismos de cache:

- **CacheMemoryStrategy**: Cache em memória (padrão)
- **CacheRedisStrategy**: Cache distribuído com Redis

**Configuração no appsettings.json:**
```json
{
  "Cache": {
    "Type": "memory" // ou "redis"
  },
  "ConnectionStrings": {
    "REDIS_CONNECTION_STRING": "localhost:6379"
  }
}
```

### Polly Resilience Patterns

O template implementa padrões de resiliência usando Polly:

- **Retry Policy**: Tentativas automáticas com backoff exponencial
- **Circuit Breaker**: Proteção contra cascata de falhas
- **Timeout Policy**: Limite de tempo para operações

**Configuração:**
```json
{
  "HarryPotterApi": {
    "BaseUrl": "https://hp-api.onrender.com",
    "Timeout": "00:00:30",
    "RetryCount": 3,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerDuration": "00:00:30"
  }
}
```

### External API Integration

#### Harry Potter API Integration
Demonstra integração com APIs externas usando HttpClient com políticas de resiliência:

**Entidades:**
```csharp
public class Character : Entity
{
    public required string Name { get; set; }
    public string? Species { get; set; }
    public string? Gender { get; set; }
    public string? House { get; set; }
    public string? DateOfBirth { get; set; }
    public bool Wizard { get; set; }
    public Wand? Wand { get; set; }
}
```

### AutoMapper Configuration
Mapeamento automático entre entidades e DTOs usando AutoMapper:

```csharp
public class SampleDataMapper : Profile
{
    public SampleDataMapper()
    {
        CreateMap<Character, CharacterDtoResponse>()
            .ForMember(dest => dest.Wand, opt => opt.MapFrom(src => src.Wand));
        CreateMap<Wand, WandDtoResponse>();
    }
}
```

### Rotas da API

A API está configurada para rodar em `http://localhost:5101` e possui as seguintes rotas implementadas:

#### SampleController (`/Sample`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/Sample` | Retorna todos os dados de exemplo | Nenhum |
| `POST` | `/Sample` | Adiciona um novo dado de exemplo | `SampleDataDto` no body |
| `GET` | `/Sample/files` | Lista arquivos usando Storage Strategy | Nenhum |
| `GET` | `/Sample/wizards` | Retorna personagens do Harry Potter | Nenhum |

#### Storage Management (`/Sample/storage`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `POST` | `/Sample/storage/test-file` | Cria um arquivo de teste | `string` content no body |
| `GET` | `/Sample/storage/files` | Lista todos os arquivos | `path` (query string, opcional) |
| `GET` | `/Sample/storage/file/{fileName}` | Lê o conteúdo de um arquivo | `fileName` no path |
| `DELETE` | `/Sample/storage/file/{fileName}` | Remove um arquivo | `fileName` no path |

#### Exemplos de Uso

**Buscar todos os dados:**
```http
GET http://localhost:5101/Sample
Accept: application/json
```

**Adicionar novo dado:**
```http
POST http://localhost:5101/Sample
Content-Type: application/json

{
    "id": "00000000-0000-0000-0000-000000000000",
    "message": "Exemplo de mensagem",
    "timeStamp": "2025-08-21T10:00:00Z"
}
```

**Buscar personagens do Harry Potter:**
```http
GET http://localhost:5101/Sample/wizards
Accept: application/json
```

**Criar arquivo de teste:**
```http
POST http://localhost:5101/Sample/storage/test-file
Content-Type: application/json

"Conteúdo do arquivo de teste"
```

**Listar arquivos:**
```http
GET http://localhost:5101/Sample/storage/files
Accept: application/json
```

**Ler arquivo:**
```http
GET http://localhost:5101/Sample/storage/file/test-20250822120000.txt
Accept: application/json
```

### Serviços Implementados

- **ISampleDataAppService**: Interface do serviço de aplicação
- **SampleDataAppService**: Implementação do serviço de aplicação com Pattern Strategy
- **SampleDataMapper**: Mapeamento entre entidades de domínio e DTOs usando AutoMapper
- **IHarryPotterApiManager**: Interface para integração com API externa
- **HarryPotterApiManager**: Cliente HTTP com políticas Polly para resiliência
- **IStorageContext**: Contexto que utiliza Strategy Pattern para storage
- **ICacheContext**: Contexto que utiliza Strategy Pattern para cache

### Dependency Injection

O template utiliza registro de dependências com diferentes ciclos de vida:

```csharp
// Scoped services
services.AddScoped<ISampleDataAppService, SampleDataAppService>();
services.AddScoped<IStorageContext, StorageContext>();
services.AddScoped<ICacheContext, CacheContext>();

// Factories
services.AddScoped<IStorageStrategyFactory, StorageStrategyFactory>();
services.AddScoped<ICacheStrategyFactory, CacheStrategyFactory>();

// Polly Policies
services.AddHttpClient<IHarryPotterApiManager, HarryPotterApiManager>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

### Configurações Avançadas

#### Storage Strategy Configuration
Para usar Azure Blob Storage, configure a connection string:
```json
{
  "Storage": {
    "Type": "azureblob",
    "ContainerName": "mycontainer"
  },
  "ConnectionStrings": {
    "AzureBlobStorage": "DefaultEndpointsProtocol=https;AccountName=..."
  }
}
```

#### Redis Cache Configuration
Para usar cache distribuído:
```json
{
  "Cache": {
    "Type": "redis"
  },
  "ConnectionStrings": {
    "REDIS_CONNECTION_STRING": "localhost:6379"
  }
}
```

#### Polly Policies Customization
Ajuste os valores conforme necessário:
```json
{
  "HarryPotterApi": {
    "RetryCount": 3,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerDuration": "00:00:30"
  }
}
```

## Como Utilizar
Este template serve como base para novos projetos ERP na InnovaSfera, facilitando a padronização e reuso de componentes. Para iniciar um novo projeto:

1. Clone este repositório
2. Renomeie os namespaces conforme o novo projeto
3. Substitua as implementações de exemplo pelas suas entidades de negócio
4. Configure as strings de conexão no `appsettings.json`
5. Escolha as estratégias de Storage e Cache apropriadas
6. Configure as políticas Polly conforme necessário
7. Execute as migrações do Entity Framework (se usar banco real)
8. Siga as boas práticas de desenvolvimento recomendadas pela equipe técnica

## Executando o Projeto

### Pré-requisitos
- .NET 8.0 ou superior
- Visual Studio 2022 ou VS Code
- Redis (opcional, para cache distribuído)
- Azure Storage Account (opcional, para storage em nuvem)

### Passos para Execução
1. Clone o repositório
2. Restaure os pacotes NuGet:
   ```bash
   dotnet restore
   ```
3. Configure as settings no `appsettings.json`:
   - Storage strategy (local ou azureblob)
   - Cache strategy (memory ou redis)
   - Strings de conexão conforme necessário
4. Execute o projeto:
   ```bash
   dotnet run --project src/Presentation/InnovaSfera.Template.Presentation.Api
   ```
5. Acesse a API em `http://localhost:5101`
6. Acesse a documentação Swagger em `http://localhost:5101/swagger`

### Testando a API
Use o arquivo `DomainDriveDesign.Presentation.Api.http` incluído no projeto para testar os endpoints, ou utilize ferramentas como Postman, curl ou Swagger UI.

### Exemplos de Teste

**Teste das Storage Strategies:**
```bash
# Criar arquivo
curl -X POST "http://localhost:5101/Sample/storage/test-file" \
     -H "Content-Type: application/json" \
     -d '"Conteúdo de teste"'

# Listar arquivos
curl -X GET "http://localhost:5101/Sample/storage/files"

# Ler arquivo
curl -X GET "http://localhost:5101/Sample/storage/file/test-20250822120000.txt"
```

**Teste da integração com API externa:**
```bash
# Buscar personagens do Harry Potter
curl -X GET "http://localhost:5101/Sample/wizards"
```

### Alternando entre Estratégias

Para testar diferentes estratégias, modifique o `appsettings.json`:

**Para usar armazenamento local:**
```json
{
  "Storage": {
    "Type": "local"
  }
}
```

**Para usar Azure Blob Storage:**
```json
{
  "Storage": {
    "Type": "azureblob",
    "ContainerName": "files"
  },
  "ConnectionStrings": {
    "AzureBlobStorage": "sua-connection-string"
  }
}
```

**Para usar cache Redis:**
```json
{
  "Cache": {
    "Type": "redis"
  },
  "ConnectionStrings": {
    "REDIS_CONNECTION_STRING": "localhost:6379"
  }
}
```

## Padrões Arquiteturais Implementados

### 1. Strategy Pattern
- **Storage Strategy**: Permite alternar entre Local Storage e Azure Blob Storage
- **Cache Strategy**: Permite alternar entre Memory Cache e Redis Cache
- **Benefícios**: Flexibilidade para trocar implementações sem alterar código cliente

### 2. Dependency Injection
- Registro automático de dependências com ciclos de vida apropriados
- Factory Pattern para criação de estratégias baseadas em configuração
- Facilita testes unitários e manutenção

### 3. Repository Pattern (via Entity Framework)
- Abstração do acesso a dados
- Separação entre lógica de negócio e persistência
- Facilita troca de providers de banco de dados

### 4. DTO Pattern
- Separação entre entidades de domínio e objetos de transferência
- Controle sobre dados expostos na API
- Versionamento independente de APIs

### 5. Resilience Patterns (Polly)
- **Retry**: Tentativas automáticas com backoff exponencial
- **Circuit Breaker**: Proteção contra cascata de falhas
- **Timeout**: Controle de tempo limite para operações
- **Bulkhead**: Isolamento de recursos críticos

### 6. Clean Architecture
- Separação clara de responsabilidades em camadas
- Dependências apontam para o centro (Domain)
- Facilita testes e manutenção

### 7. CQRS Foundations
- Separação entre comandos e consultas
- Estrutura preparada para evolução para CQRS completo
- Services específicos para diferentes responsabilidades

## Monitoramento e Logging

O template inclui configuração básica de logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "InnovaSfera.Template": "Debug",
      "System.Net.Http.HttpClient": "Information"
    }
  }
}
```

### Logs Implementados
- Operações de Storage Strategy
- Chamadas para APIs externas
- Falhas de Circuit Breaker
- Retries automáticos
- Operações de cache

## Contato
Dúvidas ou solicitações devem ser encaminhadas para o time de arquitetura da InnovaSfera.

## Changelog

### Versão Atual
- ✅ **Pattern Strategy**: Implementação completa para Storage e Cache
- ✅ **Polly Resilience**: Retry, Circuit Breaker e Timeout policies
- ✅ **External API Integration**: Cliente Harry Potter API com resiliência
- ✅ **AutoMapper**: Mapeamento automático entre entidades e DTOs
- ✅ **Azure Blob Storage**: Estratégia de armazenamento em nuvem
- ✅ **Redis Cache**: Cache distribuído como estratégia alternativa
- ✅ **Local Storage**: Estratégia de armazenamento local para desenvolvimento
- ✅ **Memory Cache**: Cache em memória para desenvolvimento
- ✅ **Storage Management API**: Endpoints completos para gerenciamento de arquivos
- ✅ **Comprehensive Documentation**: README atualizado com todas as funcionalidades

### Funcionalidades Implementadas
1. **Storage Strategy Pattern**
   - LocalStorageStrategy com sistema de arquivos
   - AzureBlobStorageStrategy com Azure Storage
   - StorageContext para alternância dinâmica

2. **Cache Strategy Pattern**
   - CacheMemoryStrategy para desenvolvimento
   - CacheRedisStrategy para produção distribuída
   - CacheContext para alternância dinâmica

3. **API Resilience com Polly**
   - Retry policy com backoff exponencial
   - Circuit breaker para proteção contra falhas
   - Timeout policy para controle de tempo

4. **External API Integration**
   - HarryPotterApiManager com políticas de resiliência
   - Mapeamento automático de entidades externas
   - Tratamento de erros e logging

5. **Dependency Injection Avançada**
   - Factory patterns para estratégias
   - Configuração baseada em appsettings
   - Scoped services para contextos

---
**InnovaSfera © Todos os direitos reservados. Uso restrito a funcionários.**
