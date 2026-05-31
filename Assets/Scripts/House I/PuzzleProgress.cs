// Tiny shared flag so the puzzle scene can tell MainWorld the puzzle is solved.
// Survives the scene swap because it's static.
public static class PuzzleProgress
{
    public static bool HouseISolved = false;
    public static bool HouseASolved = false;
}
