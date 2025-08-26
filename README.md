# InnovaSfera Template - DDD Architecture

Este projeto é um template de arquitetura DDD (Domain-Driven Design) desenvolvido e licenciado pela InnovaSfera. Seu uso é restrito exclusivamente a funcionários da InnovaSfera.

## Licença
Este projeto é propriedade da InnovaSfera e está licenciado apenas para uso interno por colaboradores da empresa. O uso externo, distribuição ou modificação sem autorização é proibido.

## Visão Geral da Arquitetura

O projeto segue os princípios de **Domain-Driven Design (DDD)** com arquitetura em camadas, promovendo separação de responsabilidades, escalabilidade e facilidade de manutenção. As principais camadas são:

- **Domain**: Contém as entidades de negócio, interfaces e regras de domínio
- **Application**: Implementa os Application Services que orquestram operações entre as camadas
- **Infrastructure**: Responsável pela persistência de dados, mensageria, armazenamento e integrações externas
- **Presentation**: Camada de apresentação (API REST) que usa apenas Application Services
- **Tests**: Testes unitários e funcionais com cobertura de 80%+

### Princípios DDD Implementados

#### **Separation of Concerns**
- Controllers dependem apenas de Application Services
- Domain não tem dependências externas
- Infrastructure implementa interfaces definidas no Domain

#### **Application Services Pattern**
- `ISampleDataAppService`: Operações de dados de exemplo
- `IMessagingAppService`: Operações de mensageria
- `IStorageAppService`: Operações de armazenamento

#### **Ports and Adapters (Hexagonal Architecture)**
- Interfaces (Ports) definem contratos no Domain/Application
- Implementações (Adapters) ficam na Infrastructure

### Detalhamento das Camadas

#### Domain (InnovaSfera.Template.Domain)
- **Entities/**: Entidades de domínio que representam os conceitos principais do negócio
- **Interfaces/**: Contratos para repositórios e serviços de domínio
- **Services/**: Implementações de lógica de negócio complexa

#### Application (InnovaSfera.Template.Application)
- **Entities/**: DTOs (Data Transfer Objects) para transferência de dados
- **Interfaces/**: Contratos dos Application Services
- **Mapper/**: Mapeamento entre entidades de domínio e DTOs
- **Services/**: Implementação dos Application Services

#### Infrastructure
- **Core**: Componentes base da infraestrutura e entidades de mensageria
- **Data**: Contexto do Entity Framework, configurações e repositórios
- **IoC**: Configuração de injeção de dependência

#### Presentation (InnovaSfera.Template.Presentation.Api)
- **Controllers/**: Controladores da API REST (seguindo DDD)
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

## Testes Unitários e Validação

### Cobertura de Testes
O projeto implementa **testes unitários abrangentes** com cobertura de **80%+** para o SampleController:

#### SampleControllerTest
- **Total de Testes**: 27 testes unitários
- **Framework**: xUnit + Moq para mocking
- **Padrão**: AAA (Arrange-Act-Assert)
- **Status**: ✅ Todos os testes passando

#### Categorias de Testes Implementadas

##### Data Operations Tests (6 testes)
- Operações GET com dados existentes/inexistentes
- Operações POST com validação FluentValidation
- Cenários de sucesso e falha

##### File Operations Tests (10 testes)
- Operações de CRUD de arquivos
- Testes de Storage Strategy
- Cenários de arquivos existentes/inexistentes

##### Storage Operations Tests (4 testes)
- Listagem de arquivos de storage
- Operações com wizards (API externa)
- Validação de ModelState

##### Messaging Operations Tests (7 testes)
- Envio de mensagens customizadas
- Processamento em lote
- Health checks do sistema de mensageria
- Simulação de DLQ (Dead Letter Queue)
- Retry automático

#### Configuração de Mocks
```csharp
private readonly Mock<ISampleDataAppService> _sampleDataAppServiceMock;
private readonly Mock<IMessagingAppService> _messagingAppServiceMock;
private readonly Mock<IStorageAppService> _storageAppServiceMock;
private readonly Mock<IValidator<SampleDataDto>> _validatorMock;
```

#### Exemplo de Teste
```csharp
[Fact]
public async Task PostAsync_WhenValidData_ShouldReturnOkWithEventResult()
{
    // Arrange
    var testId = Guid.NewGuid();
    var request = new SampleDataDto { Id = testId, Message = "Test Message", TimeStamp = DateTime.UtcNow };
    var addedData = new SampleDataDto { Id = testId, Message = "Test Message", TimeStamp = DateTime.UtcNow };
    var eventResult = MessageResult.Successful("123", "TestProvider");

    _validatorMock
        .Setup(x => x.ValidateAsync(request, default))
        .ReturnsAsync(new ValidationResult());

    _sampleDataAppServiceMock
        .Setup(x => x.AddAsync(request))
        .ReturnsAsync((addedData, eventResult));

    // Act
    var result = await _controller.PostAsync(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<SendMessageDtoResponse>(okResult.Value);
    Assert.Equal("Sample data created successfully", response.Message);
    Assert.Equal(addedData.Id, response.SampleId);
    Assert.True(response.EventSent);
}
```

### FluentValidation
O projeto implementa **validação robusta** usando FluentValidation:

#### SampleDataDto Validation
```csharp
public class SampleDataDtoValidator : AbstractValidator<SampleDataDto>
{
    public SampleDataDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id é obrigatório");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message é obrigatória")
            .MinimumLength(3)
            .WithMessage("Message deve ter pelo menos 3 caracteres");

        RuleFor(x => x.TimeStamp)
            .NotEmpty()
            .WithMessage("TimeStamp é obrigatório");
    }
}
```

#### Integração no Controller
```csharp
[HttpPost("PostSampleData")]
public async Task<ActionResult<SendMessageDtoResponse>> PostAsync([FromBody] SampleDataDto request)
{
    var validationResult = await _validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        List<ErrorHandlerResponse> errors = new();
        foreach (var item in validationResult.Errors)
            errors.Add(new ErrorHandlerResponse() { Error = item.ErrorMessage });

        return StatusCode(StatusCodes.Status400BadRequest, errors);
    }

    if (!ModelState.IsValid)
        return StatusCode(StatusCodes.Status400BadRequest, ModelState);

    var (addedSampleData, eventResult) = await _sampleDataAppService.AddAsync(request);

    return Ok(new SendMessageDtoResponse
    {
        Message = "Sample data created successfully",
        SampleId = addedSampleData.Id,
        EventSent = eventResult.IsSuccess,
        MessagingProvider = eventResult.Provider,
        EventError = eventResult.IsSuccess ? null : eventResult.ErrorMessage
    });
}
```

### Executando os Testes
```bash
# Executar todos os testes
dotnet test

# Executar testes específicos do SampleController
dotnet test --filter "FullyQualifiedName~SampleControllerTest"

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory:TestResults
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

A API está configurada para rodar em `http://localhost:5101` e possui as seguintes rotas implementadas seguindo os princípios DDD:

#### SampleController (`/api/Sample`) - Application Services Only

##### Sample Data Operations
| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/api/Sample/GetSampleData` | Retorna todos os dados de exemplo | Nenhum |
| `POST` | `/api/Sample/PostSampleData` | Adiciona novo dado com validação FluentValidation | `SampleDataDto` no body |
| `GET` | `/api/Sample/GetFiles` | Lista arquivos usando Storage Strategy | Nenhum |
| `GET` | `/api/Sample/GetWizards` | Retorna personagens do Harry Potter | Nenhum |

##### Storage Management (`/api/Sample/storage`)
| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `POST` | `/api/Sample/storage/test-file` | Cria um arquivo de teste | `string` content no body |
| `GET` | `/api/Sample/storage/files` | Lista todos os arquivos | `path` (query string, opcional) |
| `GET` | `/api/Sample/storage/file/{fileName}` | Lê o conteúdo de um arquivo | `fileName` no path |
| `DELETE` | `/api/Sample/storage/file/{fileName}` | Remove um arquivo | `fileName` no path |

##### Messaging Operations (`/api/Sample/messaging`) - **Nova Funcionalidade**
| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `POST` | `/api/Sample/messaging/send` | Envia mensagem customizada | `SendMessageDtoRequest` no body |
| `POST` | `/api/Sample/messaging/batch` | Envia múltiplas mensagens em lote | `IEnumerable<SampleDataDto>` no body |
| `GET` | `/api/Sample/messaging/health` | Verifica saúde do sistema de mensageria | Nenhum |
| `POST` | `/api/Sample/messaging/simulate-dlq` | Simula falha e envia para DLQ | `SampleDataDto` no body |
| `POST` | `/api/Sample/messaging/send-with-retry` | Envia evento com retry automático | `SampleDataDto` no body |

#### Exemplos de Uso

**Buscar todos os dados:**
```http
GET http://localhost:5101/api/Sample/GetSampleData
Accept: application/json
```

**Adicionar novo dado com validação:**
```http
POST http://localhost:5101/api/Sample/PostSampleData
Content-Type: application/json

{
    "id": "00000000-0000-0000-0000-000000000000",
    "message": "Exemplo de mensagem",
    "timeStamp": "2025-08-26T10:00:00Z"
}
```

**Enviar mensagem customizada:**
```http
POST http://localhost:5101/api/Sample/messaging/send
Content-Type: application/json

{
    "topic": "sample.events",
    "payload": {
        "type": "SampleCreated",
        "data": { "id": "123", "message": "Test" }
    },
    "correlationId": "corr-123",
    "headers": {
        "source": "SampleController"
    }
}
```

**Enviar múltiplas mensagens em lote:**
```http
POST http://localhost:5101/api/Sample/messaging/batch
Content-Type: application/json

[
    {
        "id": "11111111-1111-1111-1111-111111111111",
        "message": "Mensagem 1",
        "timeStamp": "2025-08-26T10:00:00Z"
    },
    {
        "id": "22222222-2222-2222-2222-222222222222", 
        "message": "Mensagem 2",
        "timeStamp": "2025-08-26T10:01:00Z"
    }
]
```

**Verificar saúde da mensageria:**
```http
GET http://localhost:5101/api/Sample/messaging/health
Accept: application/json
```

**Simular DLQ com dados de exemplo:**
```http
POST http://localhost:5101/api/Sample/messaging/simulate-dlq
Content-Type: application/json

{
    "id": "33333333-3333-3333-3333-333333333333",
    "message": "Teste DLQ",
    "timeStamp": "2025-08-26T10:05:00Z"
}
```

### Application Services Implementados (DDD Pattern)

O projeto implementa o padrão **Application Services** seguindo os princípios DDD:

#### ISampleDataAppService - Operações de Dados
```csharp
public interface ISampleDataAppService
{
    Task<IEnumerable<SampleDataDto>> GetAllAsync();
    Task<(SampleDataDto SampleData, MessageResult EventResult)> AddAsync(SampleDataDto sampleDataDto);
    Task<(IEnumerable<string> Files, MessageResult? EventResult)> GetFilesAsync();
    Task<ICollection<CharacterDtoResponse>> GetAllWizardsAsync();
}
```

#### IMessagingAppService - Operações de Mensageria 
```csharp
public interface IMessagingAppService
{
    Task<MessageResult> SendCustomMessageAsync(string topic, object payload, string? correlationId = null, Dictionary<string, object>? headers = null);
    Task<IEnumerable<MessageResult>> SendBatchSampleMessagesAsync(IEnumerable<SampleDataDto> sampleDataList);
    Task<(bool IsHealthy, string Provider, DateTime Timestamp)> CheckHealthAsync();
    Task<MessageResult> SimulateDlqWithSampleAsync(SampleDataDto sampleData);
    Task<MessageResult> SendSampleEventWithRetryAsync(SampleDataDto sampleData);
}
```

#### IStorageAppService - Operações de Armazenamento
```csharp
public interface IStorageAppService
{
    Task<(string FileName, string StorageType)> CreateTestFileAsync(string? content);
    Task<(IEnumerable<string> Files, string StorageType)> GetStorageFilesAsync(string path = "");
    Task<(string Content, string StorageType)> GetFileContentAsync(string fileName);
    Task<string> DeleteFileAsync(string fileName);
    Task<bool> FileExistsAsync(string fileName);
}
```

### Arquitetura de Mensageria

#### Unified Messaging Interface
O projeto implementa uma **interface unificada de mensageria** que suporta diferentes providers:

```csharp
public interface IMessagingService
{
    Task<MessageResult> SendMessageAsync(Message message, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageResult>> SendBatchMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default);
    Task<(bool IsHealthy, string Provider, DateTime Timestamp)> CheckHealthAsync();
    Task<MessageResult> SimulateDlqAsync(Message message, CancellationToken cancellationToken = default);
    Task<MessageResult> SendWithRetryAsync(Message message, int maxRetries = 3, CancellationToken cancellationToken = default);
}
```

#### MessageResult Entity
```csharp
public class MessageResult
{
    public bool Success { get; set; }
    public bool IsSuccess => Success;
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? Provider { get; set; }

    public static MessageResult Successful(string messageId, string provider) => new()
    {
        Success = true,
        MessageId = messageId,
        Provider = provider
    };

    public static MessageResult Failed(string errorMessage, Exception? exception = null, string? provider = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        Exception = exception,
        Provider = provider
    };
}
```

#### Message Entity
```csharp
public class Message
{
    public string Topic { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
    public string? CorrelationId { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Adds a header to the message
    /// </summary>
    public Message AddHeader(string key, object value)
    {
        Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Gets the current retry delay based on exponential backoff
    /// </summary>
    public TimeSpan GetRetryDelay()
    {
        var baseDelay = TimeSpan.FromSeconds(2);
        return TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, RetryCount));
    }

    /// <summary>
    /// Determines if the message can be retried
    /// </summary>
    public bool CanRetry() => RetryCount < MaxRetries;
}
```

### Serviços Implementados

- **ISampleDataAppService**: Application Service para operações de dados seguindo DDD
- **IMessagingAppService**: Application Service para operações de mensageria
- **IStorageAppService**: Application Service para operações de armazenamento
- **SampleDataMapper**: Mapeamento entre entidades de domínio e DTOs usando AutoMapper
- **IHarryPotterApiManager**: Interface para integração com API externa
- **HarryPotterApiManager**: Cliente HTTP com políticas Polly para resiliência
- **IStorageContext**: Contexto que utiliza Strategy Pattern para storage
- **ICacheContext**: Contexto que utiliza Strategy Pattern para cache

### Dependency Injection - DDD Architecture

O template utiliza registro de dependências seguindo os princípios DDD com diferentes ciclos de vida:

```csharp
// Application Services (DDD Pattern)
services.AddScoped<ISampleDataAppService, SampleDataAppService>();
services.AddScoped<IMessagingAppService, MessagingAppService>();
services.AddScoped<IStorageAppService, StorageAppService>();

// Messaging Services
services.AddScoped<IMessagingService, MessagingService>();
services.AddScoped<IMessageAdapter, MessageAdapter>();

// Storage and Cache Services
services.AddScoped<IStorageContext, StorageContext>();
services.AddScoped<ICacheContext, CacheContext>();

// Factories
services.AddScoped<IStorageStrategyFactory, StorageStrategyFactory>();
services.AddScoped<ICacheStrategyFactory, CacheStrategyFactory>();

// FluentValidation
services.AddValidatorsFromAssemblyContaining<SampleDataDtoValidator>();

// AutoMapper
services.AddAutoMapper(typeof(SampleDataMapper));

// Polly Policies for External APIs
services.AddHttpClient<IHarryPotterApiManager, HarryPotterApiManager>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

#### DDD Compliance
- **Controllers** dependem apenas de **Application Services**
- **Application Services** coordenam operações entre Domain e Infrastructure
- **Domain** permanece livre de dependências externas
- **Infrastructure** implementa interfaces definidas no Domain

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

### 1. Domain-Driven Design (DDD)
- **Separation of Concerns**: Camadas bem definidas com responsabilidades específicas
- **Application Services**: Orquestração de operações entre Domain e Infrastructure
- **Dependency Inversion**: Controllers dependem apenas de Application Services
- **Domain Isolation**: Domain livre de dependências externas

### 2. Application Services Pattern
- **ISampleDataAppService**: Operações de dados com lógica de aplicação
- **IMessagingAppService**: Operações de mensageria com diferentes providers
- **IStorageAppService**: Operações de armazenamento com strategy pattern
- **Benefícios**: Separação clara entre apresentação e lógica de negócio

### 3. Unified Messaging Pattern
- **IMessagingService**: Interface única para todas as operações de mensageria
- **IMessageAdapter**: Abstração para diferentes providers de mensageria
- **MessageResult Factory**: Métodos estáticos para criação de resultados
- **Benefícios**: Consistência e flexibilidade para trocar providers

### 4. Strategy Pattern
- **Storage Strategy**: Permite alternar entre Local Storage e Azure Blob Storage
- **Cache Strategy**: Permite alternar entre Memory Cache e Redis Cache
- **Benefícios**: Flexibilidade para trocar implementações sem alterar código cliente

### 5. Ports and Adapters (Hexagonal Architecture)
- **Ports**: Interfaces definidas no Domain/Application (ISampleDataAppService, IMessagingService)
- **Adapters**: Implementações na Infrastructure (MessagingService, StorageService)
- **Benefícios**: Isolamento do core da aplicação de detalhes de infraestrutura

### 6. Dependency Injection
- Registro automático de dependências com ciclos de vida apropriados
- Factory Pattern para criação de estratégias baseadas em configuração
- Application Services registration seguindo DDD
- Facilita testes unitários e manutenção

### 7. Repository Pattern (via Entity Framework)
- Abstração do acesso a dados
- Separação entre lógica de negócio e persistência
- Facilita troca de providers de banco de dados

### 8. DTO Pattern
- Separação entre entidades de domínio e objetos de transferência
- Controle sobre dados expostos na API
- Versionamento independente de APIs
- Mapeamento automático com AutoMapper

### 9. Validation Pattern (FluentValidation)
- **SampleDataDtoValidator**: Validação de regras de negócio
- **Pipeline Integration**: Integração com pipeline de request
- **Custom Error Messages**: Mensagens de erro personalizadas
- **Separation of Concerns**: Validação separada da lógica de controle

### 10. Resilience Patterns (Polly)
- **Retry**: Tentativas automáticas com backoff exponencial
- **Circuit Breaker**: Proteção contra cascata de falhas
- **Timeout**: Controle de tempo limite para operações
- **Bulkhead**: Isolamento de recursos críticos

### 11. Clean Architecture
- Separação clara de responsabilidades em camadas
- Dependências apontam para o centro (Domain)
- Facilita testes e manutenção
- Application Services como orquestradores

### 12. CQRS Foundations
- Separação entre comandos e consultas
- Estrutura preparada para evolução para CQRS completo
- Services específicos para diferentes responsabilidades

### 13. Unit Testing Pattern
- **AAA Pattern**: Arrange-Act-Assert em todos os testes
- **Comprehensive Mocking**: Mock de todos os Application Services
- **80%+ Coverage**: Cobertura abrangente de cenários
- **Integration Testing Ready**: Estrutura preparada para testes de integração

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

### Versão Atual - DDD Architecture Implementation
- ✅ **DDD Architecture**: Implementação completa dos princípios Domain-Driven Design
- ✅ **Application Services Pattern**: Separação correta entre Presentation, Application e Domain
- ✅ **Unified Messaging Interface**: Interface consolidada IMessagingService com IMessageAdapter
- ✅ **Messaging Operations**: Endpoints completos para envio, lote, health check, DLQ e retry
- ✅ **FluentValidation Integration**: Validação robusta com mensagens de erro customizadas
- ✅ **Unit Testing Suite**: 27 testes unitários com 80%+ cobertura usando xUnit + Moq
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

### Funcionalidades Implementadas na Arquitetura DDD

#### 1. **Domain-Driven Design Implementation**
   - Separation of Concerns entre camadas
   - Application Services como única interface para Controllers
   - Domain livre de dependências externas
   - Infrastructure com implementações de adapters

#### 2. **Unified Messaging Architecture**
   - IMessagingService como interface principal
   - IMessageAdapter para diferentes providers de mensageria
   - MessageResult com factory methods (Successful/Failed)
   - Message entity com headers, retry logic e correlation

#### 3. **Advanced Messaging Operations**
   - Send Custom Message: Envio de mensagens personalizadas
   - Batch Processing: Processamento em lote de múltiplas mensagens
   - Health Monitoring: Verificação de saúde do sistema de mensageria
   - DLQ Simulation: Simulação de Dead Letter Queue
   - Automatic Retry: Retry automático com backoff exponencial

#### 4. **Comprehensive Unit Testing**
   - SampleControllerTest com 27 testes
   - Cobertura de 80%+ de todas as operações
   - Mocking completo de Application Services
   - Testes de validação FluentValidation
   - Padrão AAA (Arrange-Act-Assert)

#### 5. **FluentValidation Integration**
   - Validação robusta para DTOs
   - Mensagens de erro customizadas
   - Integração com pipeline de validação
   - Separação entre validação e ModelState

#### 6. **Storage Strategy Pattern**
   - LocalStorageStrategy com sistema de arquivos
   - AzureBlobStorageStrategy com Azure Storage
   - StorageContext para alternância dinâmica

#### 7. **Cache Strategy Pattern**
   - CacheMemoryStrategy para desenvolvimento
   - CacheRedisStrategy para produção distribuída
   - CacheContext para alternância dinâmica

#### 8. **API Resilience com Polly**
   - Retry policy com backoff exponencial
   - Circuit breaker para proteção contra falhas
   - Timeout policy para controle de tempo

#### 9. **External API Integration**
   - HarryPotterApiManager com políticas de resiliência
   - Mapeamento automático de entidades externas
   - Tratamento de erros e logging

#### 10. **Dependency Injection Avançada**
   - Factory patterns para estratégias
   - Configuração baseada em appsettings
   - Scoped services para contextos
   - Application Services registration

---
**InnovaSfera © Todos os direitos reservados. Uso restrito a funcionários.**
