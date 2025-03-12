//Class to keep hold of Enum for team names
using UnityEngine;
public static class teamsEnum
{
    public enum teams { Unassigned, Red, Blue, Yellow, Green, Purple };
    public enum roles { Unassigned, Pet, Wizard};

    public static Color ChangeColour(teams teamName) 
    {
        Color colour = Color.black;
        switch (teamName) 
        {
            case teams.Red:

                colour = Color.red;
                break;

            case teams.Blue:
                colour = Color.blue;
                break;

            case teams.Yellow:
                colour = Color.yellow;
                break;

            case teams.Green:
                colour = Color.green;
                break;

            case teams.Purple:
                colour = Color.magenta;
                break;
        }

        return colour;
    }
}
