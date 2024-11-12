namespace FileFormats.TIM2;

public enum PixelStorageMode
{
    PSMCT32 = 0,         // RGBA32, uses 32-bit per pixel.
    PSMCT24 = 1,         // RGB24, uses 24-bit per pixel with the upper 8 bits unused.
    PSMCT16 = 2,         // RGBA16 unsigned, packs two pixels in 32-bit in little-endian order.
    PSMCT16S = 10,       // RGBA16 signed, packs two pixels in 32-bit in little-endian order.
    PSMT8 = 19,          // 8-bit indexed, packing 4 pixels per 32-bit.
    PSMT4 = 20,          // 4-bit indexed, packing 8 pixels per 32-bit.
    PSMT8H = 27,         // 8-bit indexed, with the upper 24 bits unused.
    PSMT4HL = 26,        // 4-bit indexed, with the upper 24 bits unused.
    PSMT4HH = 44,        // 4-bit indexed, evaluating bits 4-7 and discarding the rest.
    PSMZ32 = 48,         // 32-bit Z buffer.
    PSMZ24 = 49,         // 24-bit Z buffer with the upper 8 bits unused.
    PSMZ16 = 50,         // 16-bit unsigned Z buffer, packs two pixels in 32-bit in little-endian order.
    PSMZ16S = 58         // 16-bit signed Z buffer, packs two pixels in 32-bit in little-endian order.
}