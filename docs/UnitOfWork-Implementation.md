# Implementação do Padrão Unit of Work - InnovaSfera Template

## Visão Geral

O padrão Unit of Work foi implementado no projeto InnovaSfera Template para melhorar o gerenciamento de transações e manter a consistência dos dados. Esta implementação segue as melhores práticas de Domain Driven Design (DDD) e Clean Architecture.

## Estratégias Implementadas

### 1. **Unit of Work Centralizado** ⭐ (Estratégia Recomendada)

#### **Vantagens:**
- ✅ **Controle transacional robusto** - Gerenciamento explícito de transações
- ✅ **Consistência de dados** - Todas as operações são atômicas
- ✅ **Performance otimizada** - Uma única conexão por operação
- ✅ **Facilidade de manutenção** - Centralização das operações de persistência
- ✅ **Testabilidade** - Fácil criação de mocks e testes unitários

#### **Estrutura Implementada:**

```
src/
├── Domain/
│   └── Interfaces/
│       └── IUnitOfWork.cs                 # Interface principal
├── Infrastructure/
│   └── Data/
│       └── UnitOfWork/
│           └── UnitOfWork.cs              # Implementação concreta
└── Services/
    ├── SampleDataService.cs               # Serviço atualizado
    └── TransactionalSampleDataService.cs  # Exemplo com transações
```

#### **Componentes Principais:**

**1. Interface IUnitOfWork**
```csharp
public interface IUnitOfWork : IDisposable
{
    ISampleDataRepository SampleDataRepository { get; }
    Task<int> CommitAsync();
    Task<int> CommitAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

**2. Implementação UnitOfWork**
- Lazy loading de repositórios
- Controle de transações explícitas
- Gerenciamento automático de recursos

**3. Repositórios Atualizados**
- Remoção do `SaveChangesAsync` dos repositórios
- Operações CRUD sem persistência automática
- Responsabilidade de persistência transferida para UnitOfWork

## Exemplos de Uso

### **Operação Simples:**
```csharp
public async Task AddSampleDataAsync(SampleData data)
{
    _unitOfWork.SampleDataRepository.Add(data);
    await _unitOfWork.CommitAsync();
}
```

### **Operação com Transação:**
```csharp
public async Task AddMultipleSampleDataAsync(IEnumerable<SampleData> dataList)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        foreach (var data in dataList)
        {
            _unitOfWork.SampleDataRepository.Add(data);
        }

        await _unitOfWork.CommitAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### **Operações Múltiplas em uma Transação:**
```csharp
public async Task UpdateAndAddAsync(SampleData existing, SampleData newData)
{
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        _unitOfWork.SampleDataRepository.Update(existing);
        _unitOfWork.SampleDataRepository.Add(newData);

        await _unitOfWork.CommitAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## Estratégias Alternativas Consideradas

### 2. **Repository com Unit of Work Interno**
- **Vantagens:** Simplicidade, menos mudanças no código
- **Desvantagens:** Menor controle transacional, dificulta operações complexas

### 3. **Unit of Work com Context Sharing**
- **Vantagens:** Otimização de recursos
- **Desvantagens:** Complexidade de gerenciamento, possíveis conflitos

### 4. **Unit of Work por Aggregate Root**
- **Vantagens:** Alinhamento com DDD
- **Desvantagens:** Maior complexidade para cenários simples

## Benefícios da Implementação Escolhida

### **1. Controle Transacional Avançado**
- Transações explícitas quando necessário
- Rollback automático em caso de erro
- Suporte a operações atômicas complexas

### **2. Performance Otimizada**
- Uma única conexão por Unit of Work
- Lazy loading de repositórios
- Batch de operações em uma única transação

### **3. Manutenibilidade**
- Separação clara de responsabilidades
- Código mais limpo e organizdo
- Fácil extensão para novos repositórios

### **4. Testabilidade**
- Interface bem definida para mocking
- Controle granular sobre operações
- Facilita testes de integração

## Configuração e Registro

A implementação foi registrada no container de DI em `DomainDrivenDesignModule.cs`:

```csharp
// Unit of Work
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Próximos Passos Recomendados

1. **📝 Criar testes unitários** para UnitOfWork e repositórios
2. **🔧 Implementar Command/Query Responsibility Segregation (CQRS)** se necessário
3. **📊 Adicionar logging e métricas** para monitoramento
4. **🔒 Implementar retry policies** para operações transacionais
5. **📈 Considerar Event Sourcing** para auditoria avançada

## Considerações de Performance

- **Memory Usage:** Repositórios são criados sob demanda (lazy loading)
- **Connection Pooling:** Utiliza o pool de conexões do Entity Framework
- **Transaction Scope:** Transações são mantidas pelo menor tempo possível
- **Disposal:** Recursos são automaticamente liberados via `using` statements

## Troubleshooting

### **Problema:** `SaveChangesAsync` não encontrado nos repositórios
**Solução:** Use `_unitOfWork.CommitAsync()` ao invés de chamar save no repositório

### **Problema:** Transação já em andamento
**Solução:** Verifique se `BeginTransactionAsync()` foi chamado antes de `CommitTransactionAsync()`

### **Problema:** Dados não são persistidos
**Solução:** Certifique-se de chamar `await _unitOfWork.CommitAsync()`

## Conclusão

A implementação do Unit of Work centralizado oferece um controle robusto sobre transações e persistência de dados, mantendo a arquitetura limpa e escalável. Esta estratégia é ideal para aplicações que precisam de controle transacional avançado e operações atômicas complexas.
