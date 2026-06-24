using UnityEngine;

public static class SceneDatabase
{
    public class Slots
    {
        public const string Menu = "Menu";
        public const string Session = "Session";
        public const string SessionContent = "SessionContent";
        public const string House = "House";
    }
    public class Scenes
    {
        //for character selection thing - first menu that pops out before MainMenu
        public const string FirstMenu = "FirstMenu";
        public const string SecondMenu = "SecondMenu";
        public const string MainMenu = "MainMenu";
        public const string MainWorld = "MainWorld";
        public const string Session = "Session";

        //Iadd ko lang to for scene management for A & I
        public const string HouseA = "HouseA";
        public const string HouseI = "HouseI";
        public const string HouseE = "HouseE";
        public const string HouseO = "HouseO";
        public const string HouseU = "HouseU";
    }
}
