# Implementação de Autenticação JWT

## Visão Geral

Este projeto implementa autenticação JWT (JSON Web Token) seguindo as melhores práticas de segurança, utilizando uma arquitetura em camadas que permite fácil migração para o ASP.NET Core Identity no futuro.

## Características da Implementação

### 🔐 Segurança
- **Hash de senhas**: PBKDF2 com SHA-256, salt aleatório e 10.000 iterações
- **Tokens JWT**: Assinados com HMAC-SHA256
- **Refresh Tokens**: Tokens seguros de 64 bytes para renovação
- **Validação robusta**: Validação de issuer, audience, tempo de vida e assinatura
- **Cookies HttpOnly**: Refresh tokens podem ser armazenados em cookies seguros

### 🏗️ Arquitetura
- **Separação de responsabilidades**: Cada camada tem responsabilidades bem definidas
- **Inversão de dependências**: Interfaces bem definidas para fácil teste e manutenção
- **Repository Pattern**: Abstração do acesso a dados (atualmente InMemory)
- **Services Pattern**: Lógica de negócio encapsulada em serviços
- **DTO Pattern**: Transferência de dados controlada entre camadas

### 🚀 Funcionalidades

#### Endpoints Disponíveis

##### Autenticação
- `POST /api/auth/login` - Login do usuário
- `POST /api/auth/register` - Registro de novo usuário
- `POST /api/auth/refresh-token` - Renovação do token de acesso
- `POST /api/auth/revoke-token` - Revogação do refresh token (logout)

##### Perfil
- `GET /api/auth/profile` - Obter perfil do usuário autenticado

##### Testes de Autorização
- `GET /api/auth/protected` - Endpoint protegido (qualquer usuário autenticado)
- `GET /api/auth/admin-only` - Endpoint protegido (apenas administradores)

## Configuração

### 1. JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "MinhaChaveSuperSecretaDe256BitsParaJWT!@#$%^&*()_+",
    "Issuer": "InnovaSfera.Template.Api",
    "Audience": "InnovaSfera.Template.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "RequireHttpsMetadata": false,
    "SaveToken": true,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "ClockSkewSeconds": 5
  }
}
```

### 2. Usuário Padrão para Testes

**Email**: `admin@innovasfera.com`  
**Senha**: `Admin@123`  
**Roles**: `Admin`, `User`

## Exemplos de Uso

### 1. Login

```bash
curl -X POST "http://localhost:5101/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@innovasfera.com",
    "password": "Admin@123"
  }'
```

**Resposta de sucesso:**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-base64-string",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid",
    "email": "admin@innovasfera.com",
    "userName": "admin",
    "firstName": "Admin",
    "lastName": "User",
    "fullName": "Admin User",
    "isActive": true,
    "isEmailConfirmed": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "roles": ["Admin", "User"]
  }
}
```

### 2. Registro

```bash
curl -X POST "http://localhost:5101/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "usuario@exemplo.com",
    "userName": "usuario",
    "password": "MinhaSenh@123",
    "confirmPassword": "MinhaSenh@123",
    "firstName": "João",
    "lastName": "Silva"
  }'
```

### 3. Acessar Endpoint Protegido

```bash
curl -X GET "http://localhost:5101/api/auth/protected" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Renovar Token

```bash
curl -X POST "http://localhost:5101/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

### 5. Logout (Revogar Token)

```bash
curl -X POST "http://localhost:5101/api/auth/revoke-token" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Swagger/OpenAPI

O projeto inclui configuração completa do Swagger com suporte a JWT:

1. Acesse: `http://localhost:5101`
2. Clique em "Authorize" no canto superior direito
3. Digite: `Bearer YOUR_JWT_TOKEN`
4. Agora você pode testar os endpoints protegidos

## Estrutura de Arquivos

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   └── RefreshToken.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   └── IUserRepository.cs
│   │   └── Services/
│   │       ├── IAuthService.cs
│   │       ├── ITokenService.cs
│   │       └── IPasswordService.cs
│   └── Settings/
│       └── JwtSettings.cs
├── Application/
│   ├── Dto/
│   │   ├── Request/
│   │   │   ├── LoginRequestDto.cs
│   │   │   ├── RegisterRequestDto.cs
│   │   │   └── RefreshTokenRequestDto.cs
│   │   └── Response/
│   │       ├── AuthResponseDto.cs
│   │       └── UserResponseDto.cs
│   ├── Interfaces/
│   │   └── IAuthAppService.cs
│   ├── Mapper/
│   │   └── AuthMapper.cs
│   └── Services/
│       └── AuthAppService.cs
├── Infrastructure/
│   └── Data/
│       ├── Repositories/
│       │   └── InMemoryUserRepository.cs
│       └── Services/
│           ├── AuthService.cs
│           ├── TokenService.cs
│           └── PasswordService.cs
└── Presentation/
    └── Api/
        ├── Controllers/
        │   └── AuthController.cs
        └── Extensions/
            └── SwaggerExtensions.cs
```

## Migração para ASP.NET Core Identity

Esta implementação foi projetada para facilitar a migração futura para o ASP.NET Core Identity:

### 1. Entidade User
- Compatível com `IdentityUser`
- Propriedades adicionais podem ser mantidas

### 2. Interfaces
- As interfaces podem ser adaptadas para usar `UserManager<User>`
- Lógica de negócio permanece inalterada

### 3. Repositório
- Substitua `InMemoryUserRepository` por implementação Entity Framework
- Use `UserManager<User>` para operações de usuário

### 4. Serviços
- `PasswordService` pode usar `IPasswordHasher<User>`
- `TokenService` pode integrar com `SignInManager<User>`

## Segurança em Produção

### ⚠️ Importantes para Produção

1. **Chave Secreta**: Use uma chave forte de pelo menos 256 bits
2. **HTTPS**: Configure `RequireHttpsMetadata: true`
3. **Refresh Token Storage**: Use banco de dados em vez de memória
4. **Rate Limiting**: Implemente rate limiting nos endpoints de login
5. **Logging**: Configure logs de segurança adequados
6. **CORS**: Configure CORS apropriadamente
7. **Environment Variables**: Use variáveis de ambiente para secrets

### Exemplo de configuração para produção:

```json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "https://api.meudominio.com",
    "Audience": "https://app.meudominio.com",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "RequireHttpsMetadata": true,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "ClockSkewSeconds": 5
  }
}
```

## Validações Implementadas

### Senhas
- Mínimo 8 caracteres
- Pelo menos 1 letra minúscula
- Pelo menos 1 letra maiúscula
- Pelo menos 1 número
- Pelo menos 1 caractere especial

### Dados de Usuário
- Email válido e único
- Nome de usuário único (3-50 caracteres)
- Confirmação de senha

## Tratamento de Erros

- **401 Unauthorized**: Token inválido, expirado ou não fornecido
- **403 Forbidden**: Usuário não tem permissão para acessar o recurso
- **409 Conflict**: Email ou nome de usuário já existem
- **400 Bad Request**: Dados de entrada inválidos

## Performance e Escalabilidade

- **Tokens stateless**: Não requerem consulta ao banco a cada validação
- **Cache de usuários**: Implementação em memória para demonstração
- **Async/await**: Todas as operações são assíncronas
- **Dependency Injection**: Facilita testes e mocking

## Testes

A arquitetura facilita a criação de testes unitários:

- **Interfaces bem definidas**: Fácil mocking
- **Separação de responsabilidades**: Testes focados
- **Repositório in-memory**: Testes rápidos sem banco de dados

---

**Próximos Passos Sugeridos:**
1. Implementar rate limiting
2. Adicionar logs de auditoria
3. Implementar política de senha mais robusta
4. Adicionar suporte a 2FA
5. Migrar para Entity Framework com Identity
6. Implementar testes unitários e de integração
