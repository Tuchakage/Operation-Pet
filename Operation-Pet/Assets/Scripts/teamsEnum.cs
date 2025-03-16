//Class to keep hold of Enum for team names
using UnityEngine;
public static class teamsEnum
{
    public enum teams { Unassigned, Dog, Cat, Mouse, Squirrel, Horse };
    public enum roles { Unassigned, Pet, Wizard};

    public static Color ChangeColour(teams teamName) 
    {
        Color colour = Color.black;
        switch (teamName) 
        {
            case teams.Dog:

                colour = Color.red;
                break;

            case teams.Cat:
                colour = Color.blue;
                break;

            case teams.Mouse:
                colour = Color.yellow;
                break;

            case teams.Squirrel:
                colour = Color.green;
                break;

            case teams.Horse:
                colour = Color.magenta;
                break;
        }

        return colour;
    }
}
