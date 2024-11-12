# DAR format

The DAR format is a simple container format used in the AFS files of Appleseed EX and Crimson Tears Playstation 2 games.

# File structure

The format consists of a header followed by blocks of data, the number of which is specified by **Block count**.

Within the header, a list of offsets indicate the start of each block relative to the beginning of the DAR container.

After the list of offsets, the header is zero-padded to align to a 16-byte boundary.

Each block is an arbitrary block of data that can contain any other file structure, such as TM2 textures or even DAR containers, which themselves can contain DAR containers. The blocks may also contain unheadered, raw data.

Each block is zero-padded to align to a 16-byte boundary.

## Header

The header contains a number  of 

Offset	|     Type     |   Description
:------:|--------------|------------------
   00	|   int	       | MAGIC, always `DAR\0`
   04	|   int 	   | File format revision? Usually 1
   08	|   int 	   | Block count
   0C	|   int	       | RESERVED, always 0
   10	|   int*       | List of offsets
   ??	|              | Padding
   ??	|              | Unknown
   ??	|   Block*     | List of Blocks

### Unknown

After the list of offsets, there appears to be some bytes of unknown purpose. There are always a multiple of 16 bytes, i.e. 0x10, 0x20, 0x30, and sometimes they are not present, i.e. the first block starts immediately after the last padded offset.

The values do not appear to be completely random, they can be zeroed out, or can be the same across different DAR files of similar size and purpose.

# Invalid DAR files

There are some DAR files that have zero entries, consisting of only the header. There are also DAR files that have a block count set, and a set of offsets, but no blocks, or at least, the blocks seem to belong to a parent DAR.

There is also at least one DAR file where an offset was repeated, leading to two entries that point to the same block.

These files could lead to an infinite recursion, so care must be taken when parsing.
