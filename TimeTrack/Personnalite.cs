using System;

namespace TimeTrack
{
  public class Personnalite
  {
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public DateTime DateNaissance { get; set; }
    public DateTime? DateDeces { get; set; }
    public string Profession { get; set; }
    public string Description { get; set; }

    public bool EstVivant => !DateDeces.HasValue;
  }
}
