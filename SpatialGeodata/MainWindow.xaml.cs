using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private ObservableCollection<Visitor> obsVisitors = new ObservableCollection<Visitor>();
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
                dgVisitors.ItemsSource = obsVisitors;
                FillObsBuildings();
                FillObsVisitors();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FillObsVisitors()
        {
            obsVisitors.Clear();
            foreach (Visitor visitor in db.GetVisitors())
                obsVisitors.Add(visitor);
        }

        private void DrawVisitors()
        {
            cvMap.Children.RemoveRange(1, obsVisitors.Count);
            foreach (Visitor v in obsVisitors)
            {
                Ellipse blueRectangle = new Ellipse();
                blueRectangle.Height = 10;
                blueRectangle.Width = 10;
                SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
                SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
                blueRectangle.StrokeThickness = 4;
                blueRectangle.Stroke = blackBrush;
                blueRectangle.Fill = blueBrush;
                size_factor = (cvMap.ActualHeight < cvMap.ActualWidth ? cvMap.ActualHeight : cvMap.ActualWidth) / 1000.0;
                blueRectangle.SetValue(Canvas.LeftProperty, v.Position.X * size_factor);
                blueRectangle.SetValue(Canvas.TopProperty, v.Position.Y * size_factor);
                cvMap.Children.Add(blueRectangle);
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
            cvMap.Children.Clear();
            size_factor = (cvMap.ActualHeight < cvMap.ActualWidth ? cvMap.ActualHeight : cvMap.ActualWidth) / 1000.0;
            list.ForEach(point => { obsPoints.Add(new Point(point.X, point.Y)); pointsCurrentPolygon.Add(new Point(point.X * size_factor, point.Y * size_factor)); });
            DrawSelectedBuilding();
            DrawVisitors();
        }

        private void FillObsBuildings()
        {
            obsBuildings.Clear();
            foreach (Building building in db.GetBuildings())
                obsBuildings.Add(building);
        }

        private void ListBuildings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControlsToSelectedBuilding(((Building)listBuildings.SelectedItem).GetCollPoints());
            IList<Visitor> list = db.ReadVisitorOfBuilding((Building)listBuildings.SelectedItem);
            string text = "";
            foreach (var visitor in list)
                text += visitor.Name + ", ";
            if (text != "")
                text = text.Substring(0, text.Length - 2);
            txtVisitors.Content = text;
        }

        /// <summary>
        /// Resize the polygon and canvas accordingly when window is resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dashboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cvMap.Width = (border.ActualWidth < border.ActualHeight ? border.ActualWidth : border.ActualHeight) - 20;
            cvMap.Height = (border.ActualHeight < border.ActualWidth ? border.ActualHeight : border.ActualWidth) - 20;
            if ((Building)listBuildings.SelectedItem != null)
                UpdateControlsToSelectedBuilding(((Building)listBuildings.SelectedItem).GetCollPoints());
        }

        private void CvMap_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(cvMap);
            txtCurrentCoordinates.Text = "X: " + (int)(p.X / size_factor) + ", Y: " + (int)(p.Y / size_factor);
        }

        private void CvMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(relativeTo: cvMap);
            Visitor newVisitor = new Visitor(txtNewVisitorName.Text, new System.Drawing.Point((int)(p.X / size_factor), (int)(p.Y / size_factor)));
            try
            {
                db.AddVisitor(newVisitor);
                obsVisitors.Add(newVisitor);
                DrawVisitors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgVisitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string result = db.ReadBuildingWhereVisitorOccurs((Visitor)dgVisitors.SelectedItem);
            MessageBox.Show(result == "" ? "Außerhalb von Gebäuden!" : result);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int x = Convert.ToInt32(txtX.Text);
                int y = Convert.ToInt32(txtY.Text);
                int radius = Convert.ToInt32(txtRadius.Text);
                string str = db.ReadVisitorsWithinRadius(x, y, radius);
                MessageBox.Show(str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}