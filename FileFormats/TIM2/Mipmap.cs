namespace FileFormats.TIM2;

public class Mipmap
{
    public int MiptbpRegister1 { get; set; }     // 00: Miptbp register
    public int MiptbpRegister2 { get; set; }     // 04: Miptbp register
    public int MiptbpRegister3 { get; set; }     // 08: Miptbp register
    public int MiptbpRegister4 { get; set; }     // 0c: Miptbp register
    public int[] Sizes { get; set; } = new int[8]; // 10: Array of sizes
}