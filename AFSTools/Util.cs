namespace AFSTools;

public static class Util
{
    public static uint Pad(uint position, uint size)
    {
        if (position % size > 0)
        {
            position = position + size - (position % size);
        }

        return position;
    }
}