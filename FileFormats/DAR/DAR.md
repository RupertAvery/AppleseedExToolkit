# DAR format

The DAR format is a simple container format used in the AFS files of Appleseed EX and Crimson Tears Playstation 2 games.

# File structure


## Header

The format consists of a header followed by blocks of data, the number of which is specified by **Block count**.

 Offset | Type     | Description
:------:|----------|------------------
 00     | int      | MAGIC, always `DAR\0`
 04     | int      | File format revision? Usually 1
 08     | int      | Block count
 0C     | int      | RESERVED, always 0
 10     | int*     | List of Offsets ,repeated Block count times, padded
 (a)    | byte*    | List of Attributes? ,repeated Block count times, padded
 (b)    | Block*   | List of blocks

* (a) - 0x10 + Block count * 4 + padding
* (b) - 0x10 + Block count * 4 + padding + (a) + padding. This should be equal to the first offset. 

To calculate the padding, use the following algorithm:

```c#
position % 0x10 > 0 ? 0x10 - (position % 0x10) : 0
```

Where `position` is the current offset from the start of the file.

The **list of Offsets** is an array of unsigned integers indicating the start of each Block relative to the beginning of the DAR container. The list of offsets is zero-padded to align to a 16-byte boundary.

After the list of Offsets, there appears to be some bytes of unknown purpose. All that is known is that the number of bytes appear to coincide with the Block count. i.e. if there are 15 blocks, there are 15 attribute bytes. The attributes are zero-added to align to a 16-byte boundary.

There appears to be no correlation between block type, position, or size and the attribute. Sometimes the values appear to be increasing, other times the values are set to zero.

If these attributes are zeroed out, the game will refuse to boot.

## Block

A Block is an arbitrary block of data. It may contain any other file structure, such as TM2 textures or even DAR containers, which themselves may contain DAR containers. The block may also contain unheadered, raw data.

A Block must be zero-padded to align to a 16-byte boundary.

# Malformed DAR files

There are some DAR files that have zero entries, consisting of only the header. There are also DAR files that have a block count set, and a set of offsets, but no blocks, or at least, the blocks seem to belong to a parent DAR.

There was also at least one DAR file where an offset was repeated, leading to two entries that point to the same block.

Parsing these entries could lead to an infinite recursion, so care must be taken.
