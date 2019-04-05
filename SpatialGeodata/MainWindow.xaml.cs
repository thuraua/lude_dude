using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Program
{
    public partial class MainWindow : Window
    {
        private Database db;
        private Polygon currentPolygon;
        private ObservableCollection<Building> obsBuildings = new ObservableCollection<Building>();
        private ObservableCollection<Point> obsPoints = new ObservableCollection<Point>();
        private List<Point> pointsCurrentPolygon = new List<Point>();
        private double size_factor = 1;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                db = Database.GetInstance();

                listBuildings.ItemsSource = obsBuildings;
                dgCoordinates.ItemsSource = obsPoints;
                FillObsBuildings();
                DrawSelectedBuilding();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void DrawSelectedBuilding()
        {
            if (currentPolygon != null)
                cvMap.Children.Remove(currentPolygon);
            currentPolygon = new Polygon();
            currentPolygon.Stroke = Brushes.Black;
            currentPolygon.Fill = Brushes.LightBlue;
            currentPolygon.StrokeThickness = 1;
            currentPolygon.HorizontalAlignment = HorizontalAlignment.Left;
            currentPolygon.VerticalAlignment = VerticalAlignment.Center;
            currentPolygon.Points = new PointCollection(pointsCurrentPolygon);
            cvMap.Children.Add(currentPolygon);
        }
        /// <summary>
        /// Fills observable points list with Drawing.Points from DB by converting them to System.Windows.Points
        /// </summary>
        /// <param name="list">Drawing.Points list from DB</param>
        private void UpdateControlsToSelectedBuilding(List<System.Drawing.Point> list)
        {
            obsPoints.Clear();
            pointsCurrentPolygon.Clear();
            size_factor = (cvMap.ActualHeight < cvMap.ActualWidth ? cvMap.ActualHeight : cvMap.ActualWidth) / 1000.0;
            list.ForEach(point => { obsPoints.Add(new Point(point.X, point.Y)); pointsCurrentPolygon.Add(new Point(point.X * size_factor, point.Y * size_factor)); });
            DrawSelectedBuilding();
        }
        private void FillObsBuildings()
        {
            obsBuildings.Clear();
            foreach (Building building in db.GetBuildings())
                obsBuildings.Add(building);
        }
        private void ListBuildings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateControlsToSelectedBuilding(((Building)listBuildings.SelectedItem).GetCollPoints());
        }
        /// <summary>
        /// Resize the polygon acordingly to canvas when window is resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dashboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((Building)listBuildings.SelectedItem != null)
                UpdateControlsToSelectedBuilding(((Building)listBuildings.SelectedItem).GetCollPoints());
        }
    }
}