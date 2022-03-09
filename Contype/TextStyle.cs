internal readonly record struct TextStyle(ConsoleColor ForegroundColor, ConsoleColor BackgroundColor)
{
    public static TextStyle GetCurrent()
    {
        return new TextStyle(Console.ForegroundColor, Console.BackgroundColor);
    }

    public void Set()
    {
        Console.ForegroundColor = ForegroundColor;
        Console.BackgroundColor = BackgroundColor;
    }

    public void Write(string value)
    {
        var previousStyle = GetCurrent();
        Set();
        try
        {
            Console.Write(value);
        }
        finally
        {
            previousStyle.Set();
        }
    }

    public void Write(char value)
    {
        var previousStyle = GetCurrent();
        Set();
        try
        {
            Console.Write(value);
        }
        finally
        {
            previousStyle.Set();
        }
    }
}
