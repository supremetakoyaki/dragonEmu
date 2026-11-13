namespace DragonEmu.Domain;

public enum FootballTeam
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
}

public static class FootballTeams
{
    public static FootballTeam OfClassname(string classname)
    {
        int underscore = classname.LastIndexOf('_');
        if (underscore < 0 || underscore != classname.Length - 2)
        {
            return FootballTeam.None;
        }

        return classname[^1] switch
        {
            'r' => FootballTeam.Red,
            'b' => FootballTeam.Blue,
            'g' => FootballTeam.Green,
            'y' => FootballTeam.Yellow,
            _ => FootballTeam.None,
        };
    }
}
