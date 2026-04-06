using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TimeTrack
{
    public partial class MainWindow : Window
    {
        // Modèle Personnalité
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

        #region Variables
        private List<Personnalite> _personnalites;
        
        private double _zoomLevel = 1.0;
        private const double ZoomMin = 0.1;
        private const double ZoomMax = 10.0;
        private const double ZoomStep = 0.1;
        
        private DateTime _anneeDebut;
        private DateTime _anneeFin;
        
        private bool _isDragging;
        private Point _lastMousePosition;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
            InitialiserDonneesExemples();
            InitialiserTimeline();
            DessinerTimeline();
            
            MettreAJourInformations();
        }

        #region Initialisation
        private void InitialiserDonneesExemples()
        {
            _personnalites = new List<Personnalite>
            {
                new Personnalite { Nom = "Einstein", Prenom = "Albert", DateNaissance = new DateTime(1879, 3, 14), DateDeces = new DateTime(1955, 4, 18), Profession = "Physicien théoricien", Description = "Père de la relativité" },
                new Personnalite { Nom = "Newton", Prenom = "Isaac", DateNaissance = new DateTime(1643, 1, 4), DateDeces = new DateTime(1727, 3, 31), Profession = "Mathématicien, Physicien", Description = "Lois du mouvement, gravitation" },
                new Personnalite { Nom = "Da Vinci", Prenom = "Léonard", DateNaissance = new DateTime(1452, 4, 15), DateDeces = new DateTime(1519, 5, 2), Profession = "Polymathe", Description = "Artiste, inventeur, scientifique" },
                new Personnalite { Nom = "Curie", Prenom = "Marie", DateNaissance = new DateTime(1867, 11, 7), DateDeces = new DateTime(1934, 7, 4), Profession = "Physicienne, Chimiste", Description = "Radioactivité, 2 prix Nobel" },
                new Personnalite { Nom = "Tesla", Prenom = "Nikola", DateNaissance = new DateTime(1856, 7, 10), DateDeces = new DateTime(1943, 1, 7), Profession = "Ingénieur, Inventeur", Description = "Courant alternatif" },
                new Personnalite { Nom = "Hawking", Prenom = "Stephen", DateNaissance = new DateTime(1942, 1, 8), DateDeces = new DateTime(2018, 3, 14), Profession = "Physicien théoricien", Description = "Trous noirs, cosmologie" },
                new Personnalite { Nom = "Mozart", Prenom = "Wolfgang Amadeus", DateNaissance = new DateTime(1756, 1, 27), DateDeces = new DateTime(1791, 12, 5), Profession = "Compositeur", Description = "Génie musical classique" },
                new Personnalite { Nom = "LRH", Prenom = "Lafayette Ronald", DateNaissance = new DateTime(1913, 3, 13), DateDeces = new DateTime(1986, 1, 1), Profession = "Humaniste", Description = "Humaniste" },
                //new Personnalite { Nom = "Buddha", Prenom = "Siddhartha Gautama", DateNaissance = new DateTime(563, 4, 8), DateDeces = new DateTime(483, 1, 1), Profession = "Maitre spirituel", Description = "Fondateur religion" },
                 new Personnalite { Nom = "Freud", Prenom = "Sigmund", DateNaissance = new DateTime(1813, 5, 6), DateDeces = new DateTime(1939, 9, 23), Profession = "Neurologue Autrichien", Description = "Fondateur de la psychanalyse" },
            };
        }

        private void InitialiserTimeline()
        {
            _anneeDebut = _personnalites.Min(p => p.DateNaissance).AddYears(-50);
            _anneeFin = _personnalites.Max(p => p.DateDeces ?? DateTime.Now).AddYears(50);
            
            double largeurTotale = (_anneeFin.Year - _anneeDebut.Year) * 5;
            TimelineCanvas.Width = largeurTotale;
            TimelineCanvas.Height = 800;
        }
        #endregion

        #region Dessin Timeline
        private void DessinerTimeline()
        {
            TimelineCanvas.Children.Clear();
            
            double largeurCanvas = TimelineCanvas.Width;
            int nombreAnnees = _anneeFin.Year - _anneeDebut.Year;
            double pixelsParAnnee = largeurCanvas / nombreAnnees;
            
            // Ligne horizontale principale
            Line lignePrincipale = new Line
            {
                X1 = 0,
                Y1 = 100,
                X2 = largeurCanvas,
                Y2 = 100,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            TimelineCanvas.Children.Add(lignePrincipale);
            
            // Graduations des années
            for (int annee = _anneeDebut.Year; annee <= _anneeFin.Year; annee += 50)
            {
                double positionX = (annee - _anneeDebut.Year) * pixelsParAnnee;
                
                Line graduation = new Line
                {
                    X1 = positionX,
                    Y1 = 90,
                    X2 = positionX,
                    Y2 = 110,
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 1
                };
                TimelineCanvas.Children.Add(graduation);
                
                TextBlock texteAnnee = new TextBlock
                {
                    Text = annee.ToString(),
                    FontSize = 12,
                    Foreground = Brushes.DimGray
                };
                Canvas.SetLeft(texteAnnee, positionX - 15);
                Canvas.SetTop(texteAnnee, 115);
                TimelineCanvas.Children.Add(texteAnnee);
            }
            
            // Dessiner les personnalités
            for (int i = 0; i < _personnalites.Count; i++)
            {
                Personnalite p = _personnalites[i];
                DessinerPersonnalite(p, i, pixelsParAnnee);
            }
        }

        private void DessinerPersonnalite(Personnalite p, int index, double pixelsParAnnee)
        {
            double positionNaissance = (p.DateNaissance.Year - _anneeDebut.Year) * pixelsParAnnee;
            double positionDeces = p.DateDeces.HasValue 
                ? (p.DateDeces.Value.Year - _anneeDebut.Year) * pixelsParAnnee 
                : TimelineCanvas.Width;
            
            // Barre de vie
            Rectangle barreVie = new Rectangle
            {
                Width = positionDeces - positionNaissance,
                Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                RadiusX = 3,
                RadiusY = 3,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(barreVie, positionNaissance);
            Canvas.SetTop(barreVie, 97);
            barreVie.ToolTip = $"{p.Prenom} {p.Nom}\n{p.DateNaissance.Year} - {(p.DateDeces.HasValue ? p.DateDeces.Value.Year.ToString() : "Aujourd'hui")}\n{p.Profession}";
            TimelineCanvas.Children.Add(barreVie);
            
            // Carte d'identité
            Border carte = new Border
            {
                Style = (Style)FindResource("PersonCard"),
                Width = 140
            };
            
            StackPanel contenuCarte = new StackPanel();
            contenuCarte.Children.Add(new TextBlock { Text = $"{p.Prenom} {p.Nom}", FontWeight = FontWeights.Bold, FontSize = 12 });
            contenuCarte.Children.Add(new TextBlock { Text = $"{p.DateNaissance.Year} - {(p.DateDeces.HasValue ? p.DateDeces.Value.Year.ToString() : "✝")}", FontSize = 10, Foreground = Brushes.DimGray });
            contenuCarte.Children.Add(new TextBlock { Text = p.Profession, FontSize = 10, FontStyle = FontStyles.Italic, Margin = new Thickness(0,3,0,0) });
            
            carte.Child = contenuCarte;
            
            // Alterner les cartes au dessus et en dessous de la ligne
            double positionYCarte = index % 2 == 0 ? 130 + (index / 2) * 90 : 100 - 80 - (index / 2) * 90;
            
            Canvas.SetLeft(carte, positionNaissance - 70);
            Canvas.SetTop(carte, positionYCarte);
            TimelineCanvas.Children.Add(carte);
            
            // Ligne de connexion entre carte et barre de vie
            Line ligneConnexion = new Line
            {
                X1 = positionNaissance,
                Y1 = 100,
                X2 = positionNaissance,
                Y2 = index % 2 == 0 ? 130 : 100,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2, 2 }
            };
            TimelineCanvas.Children.Add(ligneConnexion);
        }
        #endregion

        #region Gestion du Zoom
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                
                double ancienZoom = _zoomLevel;
                
                if (e.Delta > 0)
                    _zoomLevel = Math.Min(_zoomLevel + ZoomStep, ZoomMax);
                else
                    _zoomLevel = Math.Max(_zoomLevel - ZoomStep, ZoomMin);
                
                // Appliquer le zoom
                ScaleTransform zoom = new ScaleTransform(_zoomLevel, 1.0);
                TimelineCanvas.RenderTransform = zoom;
                
                MettreAJourInformations();
            }
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = 1.0;
            TimelineCanvas.RenderTransform = Transform.Identity;
            TimelineScrollViewer.ScrollToHorizontalOffset(0);
            
            MettreAJourInformations();
        }
        #endregion

        #region Navigation par glisser
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(TimelineScrollViewer);
                Mouse.Capture(this);
                Cursor = Cursors.ScrollAll;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(TimelineScrollViewer);
                double delta = _lastMousePosition.X - currentPosition.X;
                
                TimelineScrollViewer.ScrollToHorizontalOffset(TimelineScrollViewer.HorizontalOffset + delta);
                
                _lastMousePosition = currentPosition;
                
                MettreAJourInformations();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
                Cursor = Cursors.Arrow;
            }
        }
        #endregion

        private void MettreAJourInformations()
        {
            ZoomLevelText.Text = $"{Math.Round(_zoomLevel * 100)} %";
            
            double positionActuelle = TimelineScrollViewer.HorizontalOffset / TimelineCanvas.Width;
            int anneeActuelle = _anneeDebut.Year + (int)(positionActuelle * (_anneeFin.Year - _anneeDebut.Year));
            PositionText.Text = $"{anneeActuelle}";
        }
    }
}