# Essentials Lutrion Quantum Plugin

## Device Config

### Lutron Quantum

``` json
{
    "key": "lights1",
	"name": "Lutron QS",
	"type": "lutronQuantum",
	"group": "plugin",
	"properties": {
		"control": {
			"method": "com",
			"controlPortDevKey": "processor",
			"controlPortNumber": 1,
			"comParams": {
				"protocol": "RS232",
				"baudRate": 115200,
				"dataBits": 8,
				"parity": "None",
				"stopBits": 1,
				"softwareHandshake": "None",
				"hardwareHandshake": "None"
			},
			"tcpSshProperties": {
				"address": "",
				"port": 23,
				"username": "",
				"password": "",
				"autoReconnect": true,
				"autoReconnectIntervalMs": 10000
			}
		},
		"username": "admin",
		"password": "lutron",
		"integrationId": "1",
		"shadeGroup1Id": "11",
		"shadeGroup2Id": "12",
		"scenes": [
			{
				"name": "Lights Full",
				"id": 1
			},
			{
				"name": "Lights Med",
				"id": 2
			},
			{
				"name": "Lights Low",
				"id": 3
			},
			{
				"name": "Lights Off",
				"id": 0
			}
		]
	}
}       
```

### Lutron QSE-IO

```json
{
	"key": "qse-io1",
	"name": "Lutron QSE-IO",
	"type": "lutronQseIo",
	"group": "plugin",
	"properties": {
		"lightingDeviceKey": "lights1",
		"integrationId": "21"
	}
}
```

### Bridge

```json
{
	"key": "plugin-bridge",
	"uid": 11,
	"name": "Plugin Bridge",
	"group": "api",
	"type": "eiscApiAdvanced",
	"properties": {
		"control": {
			"tcpSshProperties": {
				"address": "127.0.0.2",
				"port": 0
			},
			"ipid": "A1",
			"method": "ipidTcp"
		},
		"devices": [
			{
				"deviceKey": "lights1",
				"joinStart": 1
			},
			{
				"deviceKey": "qse-io1",
				"joinStart": 71
			}

		]
	}
}
```

## Join Map

### Lutron Quantum Lighing BridgeJoinMap

#### Digitals

| Join Number | Join Span | Description                               | Type          | Capabilities |
| ----------- | --------- | ----------------------------------------- | ------------- | ------------ |
| 1           | 1         | Lighting Controller Online                | Digital       | ToSIMPL      |
| 1           | 1         | Lighting Controller Select Scene By Index | Digital       | FromSIMPL    |
| 2           | 1         | Raise lighting level                      | Digital       | FromSIMPL    |
| 3           | 1         | Raise lighting level                      | Digital       | FromSIMPL    |
| 11          | 10        | Lighting Controller Select Scene          | DigitalSerial | ToFromSIMPL  |
| 41          | 10        | Lighting Controller Button Visibility     | Digital       | ToSIMPL      |
| 61          | 1         | Raise Shade Group 1                       | Digital       | FromSIMPL    |
| 62          | 1         | Lower Shade Group 1                       | Digital       | FromSIMPL    |
| 63          | 1         | Raise Shade Group 2                       | Digital       | FromSIMPL    |
| 64          | 1         | Lower Shade Group 2                       | Digital       | FromSIMPL    |

#### Analogs

| Join Number | Join Span | Description                                  | Type   | Capabilities |
| ----------- | --------- | -------------------------------------------- | ------ | ------------ |
| 1           | 1         | Device communication monitor status feedback | Analog | ToSIMPL      |
| 2           | 1         | Device socket status feedback                | Analog | ToSIMPL      |

#### Serials

| Join Number | Join Span | Description                            | Type          | Capabilities |
| ----------- | --------- | -------------------------------------- | ------------- | ------------ |
| 1           | 1         | Device Name                            | Serial        | ToSIMPL      |
| 1           | 1         | Lighting Controller Set Integration Id | Serial        | FromSIMPL    |
| 2           | 1         | Sets the Shade Group 1 ID              | Serial        | FromSIMPL    |
| 3           | 1         | Sets the Shade Group 2 ID              | Serial        | FromSIMPL    |
| 4           | 1         | Command Passthru                       | Serial        | ToFromSIMPL  |
| 11          | 10        | Lighting Controller Select Scene       | DigitalSerial | ToFromSIMPL  |

### Lutron QSE-IO BridgeJoinMap

#### Digitals

| Join Number | Join Span | Description              | Type    | Capabilities |
| ----------- | --------- | ------------------------ | ------- | ------------ |
| 1           | 5         | Contact closure feedback | Digital | ToSIMPL      |

#### Analogs

| Join Number | Join Span | Description | Type | Capabilities |
| ----------- | --------- | ----------- | ---- | ------------ |

#### Serials

| Join Number | Join Span | Description | Type   | Capabilities |
| ----------- | --------- | ----------- | ------ | ------------ |
| 1           | 1         | Device Name | Serial | ToSIMPL      |

## DEVJSON Commands

```json
devjson:1 {"deviceKey":"lights1", "methodName":"PrintScenes", "params":[]}
devjson:1 {"deviceKey":"lights1", "methodName":"SelectScene", "params":[1]}
devjson:1 {"deviceKey":"lights1", "methodName":"MasterRaise", "params":[]}
devjson:1 {"deviceKey":"lights1", "methodName":"MasterLower", "params":[]}
devjson:1 {"deviceKey":"lights1", "methodName":"MasterRaiseLowerStop", "params":[]}
devjson:1 {"deviceKey":"lights1", "methodName":"ShadeGroupRaise", "params":[1]}
devjson:1 {"deviceKey":"lights1", "methodName":"ShadeGroupLower", "params":[1]}

devjson:1 {"deviceKey":"lights1", "methodName":"ResetDebugLevels", "params":[]}
devjson:1 {"deviceKey":"lights1", "methodName":"SetDebugLevels", "params":[2]}
```