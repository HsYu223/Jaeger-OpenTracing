# Jaeger-OpenTracing

#### Jaeger Container

```bash
# container參數查詢
docker run \
  -d \
  --rm \
  -it \
  jaegertracing/{images name} \
  --help

# collector
docker run \
  --name=collector \
  -e SPAN_STORAGE_TYPE=elasticsearch \
  -p 44268:14268 \
  -p 44267:14267 \
  -p 59411:9411 \
  -p 44269:14269 \
  jaegertracing/jaeger-collector:1.7 \
  --es.server-urls srvdocker2-t:49200

# agent
docker run \
  -d \
  --name agent \
  -p 45775:5775/udp \
  -p 46831:6831/udp \
  -p 46832:6832/udp \
  -p 45778:5778/tcp \
  jaegertracing/jaeger-agent:1.7 \
  --collector.host-port=srvdocker2-t:44267

# query
docker run \
  -d \
  --name query \
  -e SPAN_STORAGE_TYPE=elasticsearch \
  -e ES_SERVER_URLS=http://srvdocker2-t:49200 \
  -p 46686:16686 \
  jaegertracing/jaeger-query:1.7

# 移除指定天數前的index
docker run \
   --rm \
   -it \
   jaegertracing/jaeger-es-index-cleaner:1.8.0 \
   10 \
   srvdocker2-t:49200
```

#### Jaeger UI

![ui](images/jaeger%20ui.PNG)

![ui query](images/jaeger%20ui%20query.PNG)

![ui query by id](images/jaeger%20ui%20query%20by%20id.PNG)

![tracing record](images/tracing%20record.png)

![tracing detail](images/tracing%20record%20detail.png)

#### Kiban查詢語法

```json
{
  "query": {
    "nested": {
      "path": "logs.fields",
      "query": {
        "bool": {
          "must": [
            {
              "match": {
                "logs.fields.value": "{\"keyword\":\"陳冠\",\"IsLeave\":true,\"Num\":1000}"
              }
            }
          ]
        }
      }
    }
  }
}
```

#### Semantic Conventions
[Semantic Conventions](https://github.com/opentracing/specification/blob/master/semantic_conventions.md)

#### The OpenTracing Semantic Specification
[The OpenTracing Semantic Specification](https://github.com/opentracing/specification/blob/master/specification.md)

#### Demo Sample
http://srvdocker2-t:16686

