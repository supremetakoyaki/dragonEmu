namespace DragonEmu.Domain;

public enum GameTeam
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
}

public static class GameTeams
{
    public static GameTeam OfClassname(string classname)
    {
        int underscore = classname.LastIndexOf('_');
        if (underscore < 0 || underscore != classname.Length - 2)
        {
            return GameTeam.None;
        }

        return classname[^1] switch
        {
            'r' => GameTeam.Red,
            'b' => GameTeam.Blue,
            'g' => GameTeam.Green,
            'y' => GameTeam.Yellow,
            _ => GameTeam.None,
        };
    }
}
