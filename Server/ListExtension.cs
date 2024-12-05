using System.Collections;

public static class ListExtension
{
    public static void ShiftLeft(this IList list, int count)
    {
        if (list.Count > 1)
        {
            for (int cycle = 0; cycle < count; cycle++)
            {
                var tmp = list[0];
                for (int i = 0; i < list.Count - 1; i++)
                {
                    list[i] = list[i+1];
                }
                list[^1] = tmp;
            }
        }
    }
}