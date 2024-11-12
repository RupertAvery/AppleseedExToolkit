namespace FileFormats.TIM2;

public enum ColorType
{
    Undefined = 0,       // Undefined
    RGBA16 = 1,          // 16-bit RGBA (A1B5G5R5)
    RGB32 = 2,           // 32-bit RGB (X8B8G8R8)
    RGBA32 = 3,          // 32-bit RGBA (A8B8G8R8)
    Indexed4Bit = 4,     // 4-bit indexed
    Indexed8Bit = 5      // 8-bit indexed
}