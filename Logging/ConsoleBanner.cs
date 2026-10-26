namespace DragonEmu.Logging;

public static class ConsoleBanner
{
    private const string Art = """

     8                                  .oPYo. o     o o    o
     8                                  8.     8b   d8 8    8
.oPYo8 oPYo. .oPYo. .oPYo. .oPYo. odYo. `boo   8`b d'8 8    8
8    8 8  `' .oooo8 8    8 8    8 8' `8 .P     8 `o' 8 8    8
8    8 8     8    8 8    8 8    8 8   8 8      8     8 8    8
`YooP' 8     `YooP8 `YooP8 `YooP' 8   8 `YooP' 8     8 `YooP'
:.....:..:::::.....::....8 :.....:..::..:.....:..::::..:.....:
::::::::::::::::::::::ooP'.:::::::::::::::::::::::::::::::::::
::::::::::::::::::::::...:::::::::::::::::::::::::::::::::::::

""";

    public static void Print()
    {
        Console.WriteLine(Art);
        Console.WriteLine($"{ServerInfo.Name} - build {ServerInfo.Build}");
        Console.WriteLine();
    }
}
