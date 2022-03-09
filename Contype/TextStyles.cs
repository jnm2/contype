internal static class TextStyles
{
    public static TextStyle Uncompleted { get; } = new(ConsoleColor.White, ConsoleColor.Black);
    public static TextStyle Error { get; } = new(ConsoleColor.White, ConsoleColor.Red);
    public static TextStyle Completed { get; } = new(ConsoleColor.DarkGreen, ConsoleColor.Black);
    public static TextStyle StatsLabel { get; } = new(ConsoleColor.DarkMagenta, ConsoleColor.Black);
    public static TextStyle StatsValue { get; } = new(ConsoleColor.DarkMagenta, ConsoleColor.Black);
}
