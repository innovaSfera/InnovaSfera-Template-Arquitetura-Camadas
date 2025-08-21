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
- **.NET**: Projeto baseado em .NET, utilizando boas práticas de desenvolvimento.
- **ASP.NET Core**: Framework web para criação de APIs REST.
- **Entity Framework**: ORM para acesso a dados (configurado na camada de Infrastructure).
- **IoC**: Injeção de dependência via Infrastructure.IoC.
- **Testes**: Estrutura separada para testes unitários e funcionais.

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

### Rotas da API

A API está configurada para rodar em `http://localhost:5101` e possui as seguintes rotas implementadas:

#### SampleController (`/Sample`)

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `GET` | `/Sample` | Retorna todos os dados de exemplo | Nenhum |
| `POST` | `/Sample` | Adiciona um novo dado de exemplo | `SampleDataDto` no body |

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

### Serviços Implementados

- **ISampleDataAppService**: Interface do serviço de aplicação
- **SampleDataAppService**: Implementação do serviço de aplicação
- **SampleDataMapper**: Mapeamento entre entidades de domínio e DTOs

## Como Utilizar
Este template serve como base para novos projetos ERP na InnovaSfera, facilitando a padronização e reuso de componentes. Para iniciar um novo projeto:

1. Clone este repositório
2. Renomeie os namespaces conforme o novo projeto
3. Substitua as implementações de exemplo pelas suas entidades de negócio
4. Configure a string de conexão no `appsettings.json`
5. Execute as migrações do Entity Framework
6. Siga as boas práticas de desenvolvimento recomendadas pela equipe técnica

## Executando o Projeto

### Pré-requisitos
- .NET 8.0 ou superior
- Visual Studio 2022 ou VS Code
- SQL Server (ou outro banco compatível com Entity Framework)

### Passos para Execução
1. Clone o repositório
2. Restaure os pacotes NuGet:
   ```bash
   dotnet restore
   ```
3. Configure a string de conexão no `appsettings.json`
4. Execute as migrações do banco (se aplicável)
5. Execute o projeto:
   ```bash
   dotnet run --project src/Presentation/InnovaSfera.Template.Presentation.Api
   ```
6. Acesse a API em `http://localhost:5101`

### Testando a API
Use o arquivo `DomainDriveDesign.Presentation.Api.http` incluído no projeto para testar os endpoints, ou utilize ferramentas como Postman ou Swagger UI.

## Contato
Dúvidas ou solicitações devem ser encaminhadas para o time de arquitetura da InnovaSfera.

---
**InnovaSfera © Todos os direitos reservados. Uso restrito a funcionários.**
