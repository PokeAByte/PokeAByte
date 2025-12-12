# EDPS

**Protocol:** Memory mapped file and UDP over port 55356

Memory mapped file: `EDPS_MemoryData.bin`  
On linux: `/dev/shm/EDPS_MemoryData.bin`  
On macOS: `/tmp/EDPS_MemoryData.bin`  

## Metadata

The first 8 bytes of any EDPS instruction consist of the metadata.

| Byte | Explanation                        |
| ---- | ---------------------------------- |
| 0    | Protocol version (current: `0x01`) |
| 1    | Reserved                           |
| 2    | reserved                           |
| 3    | Reserved                           |
| 4    | Instruction type                   |
| 5    | Direction                          |
| 6    | Reserved                           |
| 7    | Reserved                           |

The instruction direction is `0` for messages from Poke-A-Byte to the emulator and `1` for responses from the emulator
back to Poke-A-Byte.

The instruction types are:

| Value  | Type     |
| ------ | -------- |
| `0x00` | NOOP     |
| `0x01` | PING     |
| `0x02` | SETUP    |
| `0x03` | WRITE    |
| `0x04` | FREEZE   |
| `0x05` | UNFREEZE |
| `0xFF` | CLOSE    |

## Header

All EDPS instructions are at least 32 bytes long. Bytes 9 through 32 (or 8 / 31 zero-indexed) are the individual 
instructions headers, with bulk payload data (if there is any) starting directly after the header.

# Instructions

## NOOP

**Length:** Only header (32 bytes).
**Purpose:** Does nothing.

## PING

**Length:** Only header (32 bytes).
**Purpose:** Keep alive / connection check.

The emulator is expected to answer the instruction with another PING instruction that has the `Direction` field set to `0x01`.

## SETUP

**Length:** Header (32 bytes) + block descriptors (128 * 12 bytes) = *1568 bytes*
**Purpose:** Instructs the emulator on which memory should be provided via the memory mapped file and their respective
positions in the MMF.

### Header
|   Bytes | Type  | Name                                                                                         |
| ------: | ----- | -------------------------------------------------------------------------------------------- |
|  8 - 11 | int32 | How many memory blocks are in the payload                                                    |
| 12 - 15 | int32 | A suggestion to the emulator as to how many frames may be skipped between updates of the MMF |
| 16 - 31 | -     | -                                                                                            |

The header defines how many blocks in the payload are actually defined, as the payload is a fixed length for technical reasons.

If the frameskip is set to `-1`, then no frames should be skipped. 

If the frameskip is set to `0`, then the emulator may use it's own default value.

### Payload
The payload consists of 128 memory block descriptors. Their layout is as follows:

|  Bytes | Type   | Description                                   |
| -----: | ------ | --------------------------------------------- |
|  0 - 3 | uint32 | Zero-indexed position of the block in the MFF |
|  4 - 7 | uint32 | The memory address to read from the game      |
| 8 - 11 | int32  | The length of memory to read from the game    |

## WRITE 

**Length:** Variable (at least 33 bytes).
**Purpose:** Instruction to the emulator to write given data to an address in game memory.

### Header
|   Bytes | Type  | Name                                     |
| ------: | ----- | ---------------------------------------- |
|  8 - 15 | int64 | Game address to begin writing at.        |
| 16 - 19 | int32 | The length, in bytes, of the paylod data |

### Payload

The payload (following directly after the header) is the raw bytes to write to the game memory.

## FREEZE 

**Length:** Variable (at least 33 bytes).
**Purpose:** Same as WRITE, but additionally an instruction to the emulator to keep the target address (range) at the
payload value after each frame.

Alternatively, the emulator make the game memory entirely unchangeable.

### Header
|   Bytes | Type  | Name                                  |
| ------: | ----- | ------------------------------------- |
|  8 - 15 | int64 | Game address to freeze.               |
| 16 - 19 | int32 | The length, in bytes, of frozen value |

### Payload

The payload (following directly after the header) is the raw bytes to write to the game memory after each frame.

## UNFREEZE

**Purpose:** Instructs the emulator to re-allow changes to a previousy frozen address.
**Length:** 32 bytes.

### Header
|   Bytes | Type  | Name                                  |
| ------: | ----- | ------------------------------------- |
|  8 - 15 | int64 | Game address to unfreeze.             |

## CLOSE

**Length:** Only header (32 bytes).
**Purpose:** Informs either side of the connection that the other one is shutting down.

The emulator is expected to send this instruction to Poke-A-Byte before shutting down and disposal of the MMF.

The emulator CAN handle a CLOSE instruction from Poke-A-Byte and tear down the MFF and discard the setup.