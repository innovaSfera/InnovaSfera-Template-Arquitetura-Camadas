# InnovaSfera Clean Architecture Template

Este é o template oficial da InnovaSfera para criação de projetos seguindo arquitetura limpa em camadas.

## 🚀 Como usar este template

### Instalação

#### Opção 1: Instalação local (desenvolvimento)
```bash
# Clonar o repositório
git clone https://github.com/innovaSfera/InnovaSfera-Template-Arquitetura-Camadas.git
cd InnovaSfera-Template-Arquitetura-Camadas

# Instalar o template localmente
dotnet new install .
```

#### Opção 2: Instalação via NuGet (produção)
```bash
# Instalar via NuGet package (quando publicado)
dotnet new install InnovaSfera.CleanArchitecture.Template
```

### Criação de novo projeto

```bash
# Criar projeto básico
dotnet new innovasfera-clean -n MeuProjeto

# Criar projeto com empresa personalizada
dotnet new innovasfera-clean -n MeuProjeto --CompanyName "MinhaEmpresa"

# Criar projeto com .NET 9.0
dotnet new innovasfera-clean -n MeuProjeto --TargetFramework net9.0

# Criar projeto sem HTTPS
dotnet new innovasfera-clean -n MeuProjeto --UseHttps false

# Criar projeto sem Dockerfile
dotnet new innovasfera-clean -n MeuProjeto --IncludeDockerfile false
```

### Parâmetros disponíveis

| Parâmetro | Tipo | Descrição | Valor Padrão |
|-----------|------|-----------|--------------|
| `CompanyName` | string | Nome da empresa/organização | MyCompany |
| `UseHttps` | bool | Configurar HTTPS | true |
| `IncludeDockerfile` | bool | Incluir Dockerfile | true |
| `TargetFramework` | choice | Framework (.NET 8.0 ou 9.0) | net8.0 |

## Estrutura gerada

```
MeuProjeto/
├── MeuProjeto.sln
├── src/
│   ├── Application/
│   │   └── MeuProjeto.Application/
│   ├── Domain/
│   │   └── MeuProjeto.Domain/
│   ├── Infrastructure/
│   │   ├── MeuProjeto.Infrastructure.Core/
│   │   ├── MeuProjeto.Infrastructure.Data/
│   │   └── MeuProjeto.Infrastructure.IoC/
│   └── Presentation/
│       └── MeuProjeto.Presentation.Api/
└── tests/
    ├── UnitTests/
    │   └── MeuProjeto.Tests.Unit/
    └── FunctionalTests/
        └── MeuProjeto.Tests.FunctionalTests/
```

## Próximos passos

Após gerar o projeto:

1. **Configure a string de conexão** em `appsettings.json`
2. **Execute as migrações** do Entity Framework
3. **Implemente suas entidades** de domínio
4. **Crie seus serviços** de aplicação
5. **Configure seus controladores** da API

## Suporte

Para suporte e dúvidas, entre em contato com a equipe de arquitetura da InnovaSfera.
