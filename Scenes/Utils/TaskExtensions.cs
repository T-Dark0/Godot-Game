using System.Threading.Tasks;

public static class TaskExtensions
{
    public static async Task FirstOrBoth(this Task first, Task second)
    {
        var actualFirst = await Task.WhenAny(first, second);
        if (actualFirst == first) return;
        else await first;
    }
}