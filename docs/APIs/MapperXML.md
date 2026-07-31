
# Mapper XML

The minimum configuration for a mapper consists of:

- The `mapper` element
- containing at least one `properties` element 
- with at least one `property`.

## `mapper`

The `mapper` element **MUST** specify the following attributes:

- `id` - A unique identifier for the mapper in the form of a GUID.
- `name` - A descriptive name for the mapper - it's recommended to use the name of the game.
- `platform` - Which platform the game runs on. Valid values are: NES, "SNES", "GB", "GBC", "GBA", "PSX", "NDS"

The `mapper` element **SHOULD** specify a `version` attribute. This can be a number, but it's recommended to use a version number ala "1.0.0".

The mapper element **SHOULD** specify a `syntax` attribute if configuration features are used that are recent additions to Poke-A-Byte. The syntax versions relate to Poke-A-Byte releases as follows:

| Syntax | Release |
| ------ | ------- |
| *None* | 0.8.1   |
| 1      | 0.9.0   |
| 2      | 0.9.1   |
| 3      | 0.9.1   |
| 4      | 0.10.0  |

If a user tries to load a mapper for a syntax version that is higher than the currently supported one, they will instead get a warning and directions to upgrade Poke-A-Byte. This is especially important if you use certain JavaScript functionality, detailed in the [JavaScript documentation](MapperScript.md).

## `property`

The `property` element has the following attributes:

| Name                        | Required? | Type             | Member                         |
| --------------------------- | --------- | ---------------- | ------------------------------ |
| type                        | Yes       | PropertyType[1]  | Type                           |
| memoryContainer             | No        | string           | MemoryContainer                |
| address                     | No        | AddressString[2] | OriginalAddressString          |
| length                      | No        | integer          | Length                         |
| size                        | No        | integer          | Size                           |
| bits                        | No        | BitIndices[3]    | Bits                           |
| reference                   | No        | string           | Reference                      |
| description                 | No        | string           | Description                    |
| value                       | No        | any              | Value                          |
| read-function               | No        | string           | *see JavaScript documentation* |
| write-function              | No        | string           | *see JavaScript documentation* |
| after-read-value-expression | No        | string           | *see JavaScript documentation* |
| after-read-value-function   | No        | string           | *see JavaScript documentation* |
| before-write-value-function | No        | string           | *see JavaScript documentation* |

[1]: One of the following strings: "binaryCodedDecimal", "bitArray", "bool", "int", "string", "uint", "byteArray". "byteArray" requires mapper syntax >= 2.  
[2]: Either a hexadecimal memory address like `0x89AB` or a mathematical expression such as `{var} + 2`.  
[3]: Either a single integer like `2`, a range like `0-3`, or list of integers like `1, 3, 5, 7`  

For a detailed explanation of what the attributes do, please refer to the XML documentation on the IPokeAByteProperty members, as indicated by the "members" column.

Within the `<properties />` element, you can group properties by using an arbitrarily named element:

```xml
<properties>
	<header>
		<property name="title" type="string" address="0x0134" lenght="16" />
		<property name="manufacturer" type="string" address="0x013F" lenght="4" />
		<property name="cartridge_type" type="int" address="0x0147" lenght="1" />
	</header>
</properties>
```

These properties would get the following paths assinged: `header.title`, `header.manufacturer`, `header.cartridge_type`.

You can also nest groups and have properties and groups within the same parent:

```xml
<properties>
	<player>
		<!-- player.name -->
		<property name="name" type="string" address="0xf729" lenght="16" />
		<!-- player.level -->
		<property name="level" type="int" address="0x1691" lenght="2" />
		<location>
			<!-- player.location.map -->
			<property name="map" type="int" address="0xec80" lenght="2" />
			<!-- player.location.x -->
			<property name="x" type="int" address="0xec88" lenght="1" />
			<!-- player.location.y -->
			<property name="y" type="int" address="0xec89" lenght="1" />
		<location>
	</player>
</properties>
```

## `references`

References serve to translate an integer value into some other arbitrary value (often a string) via lookup. This is easier to explain via example:

```xml
<mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA" version="1.0.0">
	<properties>
		<test>
			<property name="item" type="string" address="0x00" reference="boolean" />
		</test>
	</properties>
	<references>
		<boolean>
			<entry key="0" value="False" />
			<entry key="1" value="True" />
		</boolean>
	</references>
</mapper>
``` 

The `<boolean />` names the reference. Each `entry` **must** have a key and **should** have a value and **can** have a type ("number" or "string"). In the example the property `test.item` would read the first byte of the game's memory and if that byte is `0` the property value would be `"False"` and if the byte is `1` then the value would be `"True"`.

A special case is the `<defaultCharacterMap />` reference, which is used whenever a property of type `string` is read, as games often use specialized character encodings that need to be translated:

```xml
<references>
	<defaultCharacterMap>
		<entry key="0x01" value="A" />	
		<entry key="0x02" value="B" />	
		<entry key="0x03" value="C" />	
		<!-- ... -->
	</defaultCharacterMap>
</references>
```

In this case, a property of type string for which the bytes `[0x02, 0x01, 0x02, 0x1]` is read, the value would be `BABA`. You *can* specify an alternate value for the `reference` attribute on a string property, but it defaults to `defaultCharacterMap` if you don't.

## memory

The memory section can be used to instruct Poke-A-Byte to only request certain memory blocks from the emulator. This can improve performance for both the emulator and Poke-A-Byte. 

The memory section contains a number of `read` elements with a `start` and `end` attribute each, taking a hexadecimal memory address. The read range is inclusive.

Example:

```xml
<memory>
	<!-- The first 4096 bytes of the game memory: -->
	<read start="0x0000" end="0x0FFF"/> 
	<!-- The third 4096 bytes of the game memory: -->
	<read start="0x2000" end="0x2FFF"/> 
</memory>
```

## classes

Within the `<classes/>` section of the mapper configuration, you can define resuable property groups.

```xml
<classes>
	<item>
		<property name="id"     address="{address}" type="int" length="1" />
		<property name="amount" address="{address} + 1" type="int" length="1" />
	</item>
</classes>
```

The `{address}` will be resolved when parsing the XML, rather than during property processing. See below.

To use the class, you specify the class name as the `type` on the `<property />` element:

```xml
<properties>
	<inventory>
		<class name="0" type="item" var:address="0x1691 + 0" />
		<class name="1" type="item" var:address="0x1691 + 2" />
		<!-- ... -->
	</inventory>
<properties>
```

Which result in these properties:
- `inventory.0.id`
- `inventory.0.amount`
- `inventory.1.id`
- `inventory.1.amount`


The `var:address` is what will be used to resolve the `{address}` placeholders for the properties. You can pass variables with any name using the `var:` prefix. 