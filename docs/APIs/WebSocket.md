# WebSocket

Poke-A-Byte sends out all game property updates via a SignalR websocket on `http://localhost:8085/updates`.

You can use [microsofts SignalR npm package](https://www.npmjs.com/package/@microsoft/signalr) to connect:

```ts
import { HubConnectionBuilder } from "@microsoft/signalr";
const connection = new HubConnectionBuilder()
	.withUrl("http://localhost:8085/updates")
	.build();
```

## MapperLoaded

This message is sent over the socket when a new mapper is loaded by Poke-A-Byte.

The payload is:

```ts
type MapperLoadedPayload = {
	Meta: {
		Id: string,
		GameName: string,
		GamePlatform: string,
		Version: string,
		Path: string,
	},
	Properties: IPokeAByteProperty[], // see below
	Glossary: Record<string, { key: string, value: string|number }>,
}
```

## PropertiesChanged

This message is sent after Poke-A-Byte read new memory if at least one property changed a tracked attribute. These attributes are:

```ts
type FieldChanges = "value"
	| "bytes"
	| "frozen"
	| "memoryContainer"
	| "address"
	| "length"
	| "size"
	| "bits"
	| "reference"
	| "description";
```

The payload of the message is: 

```ts
type PropertiesChangedPayload = IPokeAByteProperty[];

type IPokeAByteProperty = {
	path: string
	type: "binaryCodedDecimal" | "bitArray" | "bool" | "bit" | "int" | "string" | "uint" | "byteArray",
	memoryContainer: string | null,
	address: string | null,
	length: number,
	size: number | null,
	reference: string | null,
	bits: string | null,
	description: string | null,
	value: number | number[] | boolean | boolean[] | string | null, 
	bytes: number[],
	isFrozen: boolean,
	isReadOnly: boolean,
	fieldsChanged: FieldChanges
}
```

## InstanceReset

This message is sent when Poke-A-Byte unloaded a mapper and disconnected from the emulator.

This message has no payload.

## Error

Sent whenever Poke-A-Byte encountered an error during processing that needs the users attention. This is mainly intended for the Browser UI and only documented here for completeness' sake.

The payload is: 

```ts
type PokeAByteError = {
	Title: string,
	Detail: string,
}
```