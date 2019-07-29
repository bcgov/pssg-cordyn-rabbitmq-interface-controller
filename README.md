#  pssg-cordyn-rabbitmq-interface-controller

	API to interface with RabbitMQ instance.
## About 
	This api will give some ability to review/re-queue/remove messages that have failed and made it to the RabbitMQ parking lot.

## Technologies

	.NET Core 2.1 
	RabbitMQ

## Development Project Setup 

	Required: 
		Visual Studio 2019 
		Docker

## Usage
	
	In Visual Studio run in IIS Express

	Run the rabbitmq instance in docker. The managment port must also be mapped. 	
```
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management	
```	

## API
Swagger:
```
{
  "x-generator": "NSwag v13.0.3.0 (NJsonSchema v10.0.21.0 (Newtonsoft.Json v11.0.0.0))",
  "swagger": "2.0",
  "info": {
    "title": "Rabbit MQ Interface",
    "version": "1.0.0"
  },
  "host": "localhost:58086",
  "schemes": [
    "http"
  ],
  "paths": {
    "/api/Rabbit/messages": {
      "get": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_GetMessages",
        "produces": [
          "text/plain",
          "application/json",
          "text/json"
        ],
        "responses": {
          "200": {
            "x-nullable": false,
            "description": "",
            "schema": {
              "$ref": "#/definitions/RabbitMessages"
            }
          }
        }
      }
    },
    "/api/Rabbit/message": {
      "get": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_Get",
        "produces": [
          "text/plain",
          "application/json",
          "text/json"
        ],
        "parameters": [
          {
            "type": "string",
            "name": "id",
            "in": "query",
            "x-nullable": true
          }
        ],
        "responses": {
          "200": {
            "x-nullable": false,
            "description": "",
            "schema": {
              "$ref": "#/definitions/RabbitMessages"
            }
          }
        }
      }
    },
    "/api/Rabbit/requeue": {
      "post": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_ReQueuePost",
        "parameters": [
          {
            "type": "string",
            "name": "id",
            "in": "query",
            "x-nullable": true
          }
        ],
        "responses": {
          "200": {
            "x-nullable": true,
            "description": "",
            "schema": {
              "type": "file"
            }
          }
        }
      }
    },
    "/api/Rabbit/requeueall": {
      "post": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_ReQueueMultiplePost",
        "responses": {
          "200": {
            "x-nullable": true,
            "description": "",
            "schema": {
              "type": "file"
            }
          }
        }
      }
    },
    "/api/Rabbit/deletemessage": {
      "delete": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_DeleteMessagePost",
        "parameters": [
          {
            "type": "string",
            "name": "id",
            "in": "query",
            "x-nullable": true
          },
          {
            "type": "string",
            "name": "guid",
            "in": "query",
            "x-nullable": true
          }
        ],
        "responses": {
          "200": {
            "x-nullable": true,
            "description": "",
            "schema": {
              "type": "file"
            }
          }
        }
      }
    },
    "/api/Rabbit/deletemessages": {
      "delete": {
        "tags": [
          "Rabbit"
        ],
        "operationId": "Rabbit_DeleteMessagesPost",
        "responses": {
          "200": {
            "x-nullable": true,
            "description": "",
            "schema": {
              "type": "file"
            }
          }
        }
      }
    }
  },
  "definitions": {
    "RabbitMessages": {
      "type": "object",
      "properties": {
        "messages": {
          "type": "array",
          "items": {
            "$ref": "#/definitions/ParkingLotMessage"
          }
        }
      }
    },
    "ParkingLotMessage": {
      "type": "object",
      "required": [
        "payload_bytes",
        "redelivered",
        "message_count"
      ],
      "properties": {
        "payload_bytes": {
          "type": "integer",
          "format": "int32"
        },
        "redelivered": {
          "type": "boolean"
        },
        "exchange": {
          "type": "string"
        },
        "routing_key": {
          "type": "string"
        },
        "message_count": {
          "type": "integer",
          "format": "int32"
        },
        "properties": {
          "$ref": "#/definitions/PropertiesHeader"
        },
        "payload": {
          "type": "string"
        },
        "payload_encoding": {
          "type": "string"
        }
      }
    },
    "PropertiesHeader": {
      "type": "object",
      "properties": {
        "headers": {
          "$ref": "#/definitions/HeaderItems"
        }
      }
    },
    "HeaderItems": {
      "type": "object",
      "required": [
        "x-death"
      ],
      "properties": {
        "x-death": {
          "type": "integer",
          "format": "int32"
        },
        "x-request-id": {
          "type": "string"
        },
        "date": {
          "type": "string"
        }
      }
    }
  }
}
```

## License

