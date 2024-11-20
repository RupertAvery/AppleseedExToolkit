# DAR format

The AFS format is a simple container format

# File structure

## Header

The format consists of a header followed by blocks of data, the number of which is specified by **Block count**.

 Offset | Type        | Description
:------:|-------------|------------------
 00     | int         | MAGIC, always `DAR\0`
 04     | int         | File count
 08     | int*        | List of Offsets ,repeated Block count times, padded
 (a)    | Block*      | List of blocks
 (b)    | Attribute*  | List of Attributes? ,repeated Block count times, padded
