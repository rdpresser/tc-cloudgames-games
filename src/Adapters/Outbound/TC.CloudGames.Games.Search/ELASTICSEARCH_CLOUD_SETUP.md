# ?? Configura��o para Elasticsearch Cloud

## ? Configura��o para sua URL espec�fica

Para conectar ao seu Elasticsearch Cloud em `https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443`, use a seguinte configura��o:

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

### Vari�veis de Ambiente (alternativa)

```bash
ELASTICSEARCH__ENDPOINTURL=https://my-elasticsearch-project-f61917.es.eastus.azure.elastic.cloud:443
ELASTICSEARCH__APIKEY=your-api-key-here
ELASTICSEARCH__INDEXPREFIX=search-xn8c
```

## ?? Como obter sua API Key

1. Acesse [Elasticsearch Cloud Console](https://cloud.elastic.co/)
2. Selecione seu deployment: `my-elasticsearch-project-f61917`
3. V� em **Security** ? **API Keys**
4. Clique em **Create API key**
5. D� um nome (ex: "Games Search API")
6. Defina as permiss�es necess�rias:
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

## ?? Testando a Conex�o

1. **Substitua a API Key** no arquivo de configura��o
2. **Inicie a aplica��o**
3. **Verifique os logs** - voc� deve ver:
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

## ?? Exemplo Completo de Configura��o

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

## ?? Considera��es de Seguran�a

1. **Nunca commite a API Key** no c�digo fonte
2. **Use vari�veis de ambiente** ou secrets management
3. **Rotacione as API Keys** regularmente
4. **Monitore o uso** das API Keys
5. **Use permiss�es m�nimas** necess�rias

## ?? Troubleshooting

### Erro: "Authentication failed"
- Verifique se a API Key est� correta
- Confirme que a API Key n�o expirou
- Verifique as permiss�es da API Key

### Erro: "Connection refused"
- Confirme se a URL est� correta
- Verifique conectividade de rede
- Teste a URL no browser (deve retornar erro 401, n�o erro de conex�o)

### Erro: "Index not found"
- Execute o reindex: `POST /game/reindex`
- Verifique se o �ndice foi criado: `GET /_cat/indices/search-xn8c*`

## ?? Monitoramento

Para monitorar sua integra��o:

1. **Logs da aplica��o**: Verifique os logs do servi�o de busca
2. **Elasticsearch Cloud Console**: Monitor de performance e uso
3. **Health Check**: Implemente um endpoint de health check
4. **M�tricas**: Monitor n�mero de buscas, tempo de resposta, etc.

## ?? Status da Configura��o

? **EndpointUrl**: Configurado para sua inst�ncia espec�fica  
? **Autentica��o**: API Key suportada  
? **Index**: search-xn8c configurado  
? **Compatibilidade**: Elasticsearch 9.x  
? **Pronto para produ��o**: Sim  

Sua configura��o est� pronta para funcionar com o Elasticsearch Cloud!