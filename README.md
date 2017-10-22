[![Build status](https://ci.appveyor.com/api/projects/status/t8cgh031a61y7bti?svg=true)](https://ci.appveyor.com/project/Ace-Lightning/graphics-castlevanialegends-gb)
# About
Application for working with graphics in "Castlevania: Legends" game on GameBoy.

### Algorithm of decompressing
The first byte of compressed data is flag-byte. There is the byte after every eight bytes.
> $F5 -> 11110101

If a bit of flag-byte equals 1 then one byte is copied from compressed data as is. If a bit equals 0 then you must take two bytes from compressed data. First byte is junior part of buffer offset, five bits of second byte are part of a buffer byte's count. You must take five bytes and add $03. Sixth and seventh bits of second byte are senior part of buffer offset. For example:
> $DE $6E
1. $6E and $1F = $0E
2. $0E + $03 = $11 - is a count of buffer bytes to be copied
3. Swap the halfs of second byte = $E6
4. $E6 Rsh 1 = $73
5. $73 and $03 = $03
6. $03 $DE - is buffer offset

### Buffer
Size of buffer is $400 (from $000 till $3FF). The first $3DF bytes are $20, the others are $00.