using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoordinatesDemo
{
    public partial class MainWindow : Window
    {
        private FrameworkElement selectedElement;
        private bool isDragging = false;
        private Point previousMousePosition;

        public MainWindow()
        {
            InitializeComponent();
            selectedElement = MyEllipse;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCoordinates();
        }

        // Из координат элемента в координаты окна.
        private Point ConvertLocalToGlobal(
            UIElement element, Point localPoint)
        {
            return element
                .TransformToAncestor(Window.GetWindow(element))
                .Transform(localPoint);
        }

        // Из координат окна в координаты элемента.
        private Point ConvertGlobalToLocal(
            UIElement element, Point globalPoint)
        {
            return Window.GetWindow(element)
                .TransformToDescendant(element)
                .Transform(globalPoint);
        }

        private string PointToText(Point point)
        {
            return "X = " + Math.Round(point.X, 2)
                + "; Y = " + Math.Round(point.Y, 2);
        }

        private void UpdateCoordinates()
        {
            // Берём центр выбранного элемента.
            Point localPoint = new Point(
                selectedElement.ActualWidth / 2,
                selectedElement.ActualHeight / 2
            );

            Point globalPoint = ConvertLocalToGlobal(
                selectedElement, localPoint
            );

            // Переводим полученную точку обратно для проверки.
            Point backPoint = ConvertGlobalToLocal(
                selectedElement, globalPoint
            );

            // Отдельно проверяем левый верхний угол.
            Point cornerPoint = ConvertLocalToGlobal(
                selectedElement, new Point(0, 0)
            );

            CoordinatesText.Text =
                "Элемент: " + selectedElement.Name
                + "\n\nЦентр внутри элемента:\n"
                + PointToText(localPoint)
                + "\n\nЦентр относительно окна:\n"
                + PointToText(globalPoint)
                + "\n\nОбратное преобразование:\n"
                + PointToText(backPoint)
                + "\n\nТочка (0; 0) элемента в окне:\n"
                + PointToText(cornerPoint);
        }

        private void ShowCoordinates_Click(
            object sender, RoutedEventArgs e)
        {
            UpdateCoordinates();

            MessageBox.Show(
                CoordinatesText.Text,
                "Координаты"
            );
        }

        private void Transform_ValueChanged(
            object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // При создании окна элементы ещё могут быть не готовы.
            if (IsLoaded == false)
            {
                return;
            }

            EllipseRotate.Angle = AngleSlider.Value;
            EllipseScale.ScaleX = ScaleSlider.Value;
            EllipseScale.ScaleY = ScaleSlider.Value;

            selectedElement = MyEllipse;
            ConvertedPointText.Text = "";
            UpdateCoordinates();
        }

        private void Element_MouseDown(
            object sender, MouseButtonEventArgs e)
        {
            selectedElement = (FrameworkElement)sender;

            previousMousePosition = e.GetPosition(WorkCanvas);
            isDragging = true;

            // Продолжаем получать события даже за пределами элемента.
            selectedElement.CaptureMouse();

            ConvertedPointText.Text = "";
            UpdateCoordinates();

            e.Handled = true;
        }

        private void Element_MouseMove(
            object sender, MouseEventArgs e)
        {
            if (isDragging == false)
            {
                return;
            }

            if (e.RightButton != MouseButtonState.Pressed)
            {
                isDragging = false;
                selectedElement.ReleaseMouseCapture();
                return;
            }

            Point currentMousePosition = e.GetPosition(WorkCanvas);

            double changeX =
                currentMousePosition.X - previousMousePosition.X;

            double changeY =
                currentMousePosition.Y - previousMousePosition.Y;

            double left = Canvas.GetLeft(selectedElement);
            double top = Canvas.GetTop(selectedElement);

            Canvas.SetLeft(selectedElement, left + changeX);
            Canvas.SetTop(selectedElement, top + changeY);

            previousMousePosition = currentMousePosition;

            // Применяем новое положение перед чтением координат.
            WorkCanvas.UpdateLayout();

            UpdateCoordinates();

            e.Handled = true;
        }

        private void Element_MouseUp(
            object sender, MouseButtonEventArgs e)
        {
            if (isDragging == true)
            {
                isDragging = false;
                selectedElement.ReleaseMouseCapture();
                UpdateCoordinates();
            }

            e.Handled = true;
        }

        private void ConvertPoint_Click(
            object sender, RoutedEventArgs e)
        {
            double x;
            double y;

            bool correctX = double.TryParse(
                GlobalXTextBox.Text, out x
            );

            bool correctY = double.TryParse(
                GlobalYTextBox.Text, out y
            );

            if (correctX == false || correctY == false)
            {
                MessageBox.Show("Введите числа в поля X и Y.");
                return;
            }

            Point globalPoint = new Point(x, y);

            Point localPoint = ConvertGlobalToLocal(
                selectedElement, globalPoint
            );

            ConvertedPointText.Text =
                "Точка внутри " + selectedElement.Name
                + ":\n" + PointToText(localPoint);
        }
    }
}