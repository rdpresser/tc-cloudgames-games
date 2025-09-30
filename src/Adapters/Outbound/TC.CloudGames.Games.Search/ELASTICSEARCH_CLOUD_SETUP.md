# ?? Configuração para Elasticsearch Cloud

## ? Configuração para sua URL específica

Para conectar ao seu Elasticsearch Cloud em `https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443`, use a seguinte configuração:

### appsettings.json ou appsettings.Development.json

```json
{
  "Elasticsearch": {
    "EndpointUrl": "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443",
    "ApiKey": "YOUR_API_KEY_HERE",
    "IndexPrefix": "search-xn8c",
    "RequestTimeoutSeconds": 30,
    "MaxSearchSize": 1000
  }
}
```

### Variáveis de Ambiente (alternativa)

```bash
ELASTICSEARCH__ENDPOINTURL=https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
ELASTICSEARCH__APIKEY=your-api-key-here
ELASTICSEARCH__INDEXPREFIX=search-xn8c
```

## ?? Como obter sua API Key

1. Acesse [Elasticsearch Cloud Console](https://cloud.elastic.co/)
2. Selecione seu deployment: `my-elasticsearch-project-f61917`
3. Vá em **Security** ? **API Keys**
4. Clique em **Create API key**
5. Dê um nome (ex: "Games Search API")
6. Defina as permissões necessárias:
   ```json
   {
     "cluster": ["monitor"],
     "indices": [
       {
         "names": ["search-xn8c*"],
         "privileges": ["all"]
       }
     ]
   }
   ```
7. Copie a API key gerada (ela vem codificada em base64)

## ?? Testando a Conexão

1. **Substitua a API Key** no arquivo de configuração
2. **Inicie a aplicação**
3. **Verifique os logs** - você deve ver:
   ```
   ?? Elasticsearch configured for Cloud deployment
   ?? Connecting to: https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
   ```
4. **Teste o reindex**:
   ```bash
   POST https://localhost:7001/game/reindex
   ```
5. **Teste uma busca**:
   ```bash
   GET https://localhost:7001/games/search?query=action
   ```

## ?? Exemplo Completo de Configuração

### appsettings.Production.json
```json
{
  "Elasticsearch": {
    "EndpointUrl": "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443",
    "ApiKey": "${ELASTICSEARCH_API_KEY}",
    "IndexPrefix": "search-xn8c-prod",
    "RequestTimeoutSeconds": 30,
    "MaxSearchSize": 1000
  }
}
```

### docker-compose.yml (para usar com containers)
```yaml
version: '3.8'
services:
  games-api:
    image: tc-cloudgames-games-api:latest
    environment:
      - ELASTICSEARCH__ENDPOINTURL=https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
      - ELASTICSEARCH__APIKEY=${ELASTICSEARCH_API_KEY}
      - ELASTICSEARCH__INDEXPREFIX=search-xn8c
    ports:
      - "80:8080"
```

### Kubernetes Secret
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: elasticsearch-secret
type: Opaque
data:
  api-key: <base64-encoded-api-key>
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: elasticsearch-config
data:
  endpoint-url: "https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443"
  index-prefix: "search-xn8c"
```

## ?? Considerações de Segurança

1. **Nunca commite a API Key** no código fonte
2. **Use variáveis de ambiente** ou secrets management
3. **Rotacione as API Keys** regularmente
4. **Monitore o uso** das API Keys
5. **Use permissões mínimas** necessárias

## ?? Troubleshooting

### Erro: "Authentication failed"
- Verifique se a API Key está correta
- Confirme que a API Key não expirou
- Verifique as permissões da API Key

### Erro: "Connection refused"
- Confirme se a URL está correta
- Verifique conectividade de rede
- Teste a URL no browser (deve retornar erro 401, não erro de conexão)

### Erro: "Index not found"
- Execute o reindex: `POST /game/reindex`
- Verifique se o índice foi criado: `GET /_cat/indices/search-xn8c*`

## ?? Monitoramento

Para monitorar sua integração:

1. **Logs da aplicação**: Verifique os logs do serviço de busca
2. **Elasticsearch Cloud Console**: Monitor de performance e uso
3. **Health Check**: Implemente um endpoint de health check
4. **Métricas**: Monitor número de buscas, tempo de resposta, etc.

## ?? Status da Configuração

? **EndpointUrl**: Configurado para sua instância específica  
? **Autenticação**: API Key suportada  
? **Index**: search-xn8c configurado  
? **Compatibilidade**: Elasticsearch 9.x  
? **Pronto para produção**: Sim  

Sua configuração está pronta para funcionar com o Elasticsearch Cloud!