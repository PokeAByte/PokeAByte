# Mapper JavaScript

A JavaScript file with the same name as the mapper XML file will automatically be loaded when the mapper is loaded. E.g. `smw.js` will be loaded for `smw.xml`, if it exists.

The JavaScript will run every time that Poke-A-Byte fetches fresh data from the emulator and can interact with the processing of emulator data in certain ways.

**Note**: Below TypeScript syntax is used to indicate function signatures, but the [JavaScript engine](https://github.com/sebastienros/jint) used by Poke-A-Byte does not support TypeScript.

In general, you should try to minimize the work done in JavaScript, as interpreting the script can be quite slow at times and by default Poke-A-Byte tries to update the properties every 200 times a second. Because the scripts are interpreted, you may also have to write code a little differently from what you might be used to in order to minimize work.

## Available interfaces

Poke-A-Byte provides the following interfaces to the JavaScript as constants:

- `__console`: `ScriptConsole` - A shim around `ILogger` that mirrors the standard `console` JavaScript API. This can be used to write into the Poke-A-Byte logfile for debugging and error reporting.
- `__state`: A `Dictionary<string, object?>` instance for holding data inside the JavaScript. This is not used by Poke-A-Byte itself.
- `__variables`: A `Dictionary<string, object?>` instance for variables shared between Poke-A-Byte and the script. Poke-A-Byte will use this dictionary to resolve variables in `property` address expressions. `reload_addresses` is a reserved variable name.
- `__mapper`: This is the `IPokeAByteMapper` instance and can be used to read processed game properties or manipulate their values.
- `__memory`: An `IMemoryManager` instance. This can be used to read or write to raw game memory. Note that for writing game memory back into the emulator itself, you should use the `__driver`.
- `__driver`: The `IPokeAByteDriver` instance that Poke-A-Byte uses to talk to the emulator. This can be used to write memory back into the game itself (if the emulator and its driver support that functionality).

You can also import the following interfaces from the `game_functions` module:

```js
import { pokemon } from "game_functions";
```

Where `pokemon` is an instance of `PokemonFunctions` with `Encrypt(int gen, byte[] data)` and `Decrypt(int gen, byte[] data)` functions.  

**Important**: The `game_functions` module requires mapper syntax >= 4.

## Standard functions

`preprocessor(): boolean | undefined`

Is called after Poke-A-Byte fetched new memory data from the emulator but before it updates all the properties.

If the function returns false, Poke-A-Byte will not process any properties and will not send any updates to clients.

`postprocessor(): boolean | undefined` 

Is called after Poke-A-Byte evaluated and updated all the properties.

If the function returns false, Poke-A-Byte will not send any property updates to clients.

`containerprocessor(container: string, containerBytes: number[]) ` (requires syntax 4 or higher)

Is called when a property with a non-default `memoryContainer` was updated. This is useful for games that encrypt parts of their memory. You can decrypt that memory in the preprocessor function and write the decrypted data into a `decrypted` memoryContainer. When a user - or an external tool - updates a property that reads from this decrypted container, the `containerprocessor` function is called. You can then re-encrypt the memory and use the `__driver` interface to update the game itself.

**Important**: The `containerprocessor` functionality requires mapper syntax >= 2.

Contrast with `before-write-value-function` and `write-function`.

You have to export these functions (`export preprocessor() ...`).

## Other callbacks

On every property, you can specify one of the following attributes to call an exported script function:

`read-function`: Called when processing the property after fetching new memory.

Note: This function is always executed, even if the game memory has not changed.
The signature is `function_name(property: IPokeAByteProperty): boolean | undefined`.
If the function returns false, Poke-A-Byte will not process the property and thus not update it's value.

`write-function`: Called when the property is updated by the user or an external tool and before new bytes are sent back to the emulator.

The signature is `function_name(property: IPokeAByteProperty): boolean | undefined`.
If the function returns false, Poke-A-Byte will not write the new memory back to the emulator.

`after-read-value-function`: 

The signature is `function_name(property: IPokeAByteProperty): any`.

`before-write-value-function`: 

The signature is `function_name(property: IPokeAByteProperty): boolean | undefined`.

## after-read-value-expression

This is a plain JavaScript expression. You can reference anything exported by your script or provided by Poke-A-Byte documented above. Or just use standard JavaScript functions. For the scope of the expression, `x` is provided by Poke-A-Byte and is the value of the read and preprocessed memory. For example:

```xml
<property name="is_even" type="int" address="0x89AB" after-read-value-expression="x % 2"/>
```

Here `x` is the integer value read from the game. The value assigned to the IPokeAByteProperty will then be the result of the `x % 64` modulo operation, so `1` for an odd number or `0` for an even number.

```xml
<property name="half_floored" type="int" address="0x89AB" after-read-value-expression="Math.Floor(x / 2)"/>
```

In this example the standard `Math.floor()` function is invoked after dividing x by 2. You can consult the [Jint documentation](https://github.com/sebastienros/jint#supported-features) for which APIs are available to use.

## A note on variables

You can use variables to tell Poke-A-Byte where in memory to look for a property's underlying byte values. 

```xml
<property name="c" type="int" address="{some_pointer} + 128" />
```

```js
export function preprocessor() {
	// Provide Poke-A-Byte with the value for "some_pointer":
	__variables['some_pointer'] = determinePointer(); // example function for illustration purposes.
	// Instruct Poke-A-Byte to reevaluate the address of the 'test' property before trying to read it's memory:
	__variables['reload_addresses'] = true;
}
```

Poke-A-Byte will always try to re-evaluate the address expression if doing so failed on a previous iteration. It will also do so if the preprocessor function set the `reload_addresses` variable to true. Poke-A-Byte will also not reset that variable, you will have do that yourself. 

Depending on how many properties use a variable in their address expression, the reevaluation may be computationally intense and can create a signficatnt amount of extra work per iteration and thus introduce additional latency for property updates. For the above example, you could rewrite the preprocessor as follows to reduce the penalty:

```js
export function preprocessor() {
	const newPointer = determinePointer();
	if (newPointer !== __variables['some_pointer']) {
		__variables['some_pointer'] = newPointer;
		__variables['reload_addresses'] = true;
	}
}
```

## `__mapper'

There are other properties on the IPokeAByteMapper interface, but they may not be stable as they are meant for internal use and they are of limited use to the mapper script.

```ts
type PropertyValue = number | number[] | boolean | boolean[] | string | null;
const __mapper: {
	/** The memory regions configured to be read by the mapper */
	memory: { readRanges: { start: number, end: number }[] }
	/** A dictionary of game properties. The key is the respective property path. */
	properties: Record<string, IPokeAByteProperty>,
	/** The glossary or references configured by the mapper XML. */
	references: Record<string, ReferenceItems>,
	/** Get the property for the target path. */
	get_property: (path: string): IPokeAByteProperty,
	/** Get the value of the property for the target path. Throws an exception if the property does not exist. */
	get_property_value: (path: string): PropertyValue,
	/** Set the value for the property for the target path. Throws an exception if the property does not exist. */
	set_property_value: (path: string, value: PropertyValue): void,
	/** 
	 * Copies the attributes of all properties with the given sourcePath prefix to properties of the same 
	 * suffix in the destinationPath prefix. 
	 * E.g. if you have properties `foo.A, foo.B, foo.C` and `bar.B, bar.C` and call this function with
	 * copy_properties("A", "B") then the values for `foo.B` and `foo.C` are copied to `bar.B` and `bar.C` respectively.
	 * **IMPORTANT**: Requires mapper syntax 1
	 */
	copy_properties(sourcePath: string, destinationPath: string ): void,
}
```

## `__driver'

There are other functions available on the IPokeAByteDriver interface for technical reason, but they **SHOULD NOT** be used from the script.

```ts
const __driver: {
	/** The proper name of the emulator (or protocol) the driver is for. */
	properName: string,
	/** How many milliseconds the Poke-A-Byte instance should wait in between reading memory data. */
	delayMsBetweenReads: number,
	/**
	 * Instruct the emulator to write bytes into the games memory.
	 * @param startingMemoryAddress The starting address for the write.
     * @param values An array of bytes that the emulator should write.
	 * @returns A Promise.
	 */
	writeBytes: (startingMemoryAddress: number, values: number[]): Promise;
}
```
