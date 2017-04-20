using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Helpers;
using GraphLabs.Graphs;
using System;
using GraphLabs.Graphs.UIComponents.Visualization;
using Vertex = GraphLabs.Graphs.UIComponents.Visualization.Vertex;

namespace GraphLabs.Tasks.Template
{
    /// <summary> Поиск компонент сильной связности, построение конденсата </summary>
    public partial class TaskTemplate : TaskViewBase
    {
        private const double START_NEW_VERTEX_R = 30.0;
        #region Команды


        /*/// <summary> Клик по вершине </summary>
        public event EventHandler<VertexClickEventArgs> VertexClicked;*/
        /// <summary> Клик по вершине </summary>
        public static readonly DependencyProperty VertexClickCommandProperty = DependencyProperty.Register(
            "VertexClickCommand",
            typeof(ICommand),
            typeof(TaskTemplate),
            new PropertyMetadata(default(ICommand)));

        /// <summary> Клик по вершине </summary>
        public ICommand VertexClickCommand
        {
            get { return (ICommand)GetValue(VertexClickCommandProperty); }
            set { SetValue(VertexClickCommandProperty, value); }
        }

        /// <summary> Клик по визуализатору </summary>
        public static readonly DependencyProperty VisualizerClickCommandProperty = DependencyProperty.Register(
            "VisualizerClickCommand",
            typeof(ICommand),
            typeof(TaskTemplate),
            new PropertyMetadata(default(ICommand)));

        /// <summary> Клик по визуализатору </summary>
        public ICommand VisualizerClickCommand
        {
            get { return (ICommand)GetValue(VisualizerClickCommandProperty); }
            set { SetValue(VisualizerClickCommandProperty, value); }
        }

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public static readonly DependencyProperty OnLoadedCommandProperty =
            DependencyProperty.Register("OnLoadedCommand", typeof(ICommand), typeof(TaskTemplate), new PropertyMetadata(default(ICommand)));

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public ICommand OnLoadedCommand
        {
            get { return (ICommand)GetValue(OnLoadedCommandProperty); }
            set { SetValue(OnLoadedCommandProperty, value); }
        }

        #endregion


        /// <summary> Вершины из визуализатора </summary>
        public static DependencyProperty VertVisProperty =
            DependencyProperty.Register("VertVis", typeof(ReadOnlyCollection<Vertex>), typeof(TaskTemplate), new PropertyMetadata(default(ReadOnlyCollection<Vertex>)));

        /// <summary> Вершины из визуализатора </summary>
        public ReadOnlyCollection<Vertex> VertVis
        {
            get { return (ReadOnlyCollection<Vertex>)GetValue(VertVisProperty); }
            set { SetValue(VertVisProperty, value); }
        }

        /// <summary> Ctor. </summary>
        public TaskTemplate()
        {
            InitializeComponent();

            // Куча Binding'ов (в реальных заданиях)
            SetBinding(VertexClickCommandProperty, new Binding("VertexClickCmd"));
            SetBinding(OnLoadedCommandProperty, new Binding("OnLoadedCmd"));
            SetBinding(VisualizerClickCommandProperty, new Binding("VisualizerClickCmd"));
            SetBinding(VertVisProperty, new Binding("VertVisCol") { Mode = BindingMode.TwoWay });
        }



        private void OnVertexClick(object sender, VertexClickEventArgs e)
        {
            if (VertexClickCommand != null)
            {
                VertexClickCommand.Execute(e.Control);
            }
            VertVis = Visualizer.Vertices;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (OnLoadedCommand != null)
            {
                OnLoadedCommand.Execute(null);
            }
            VertVis = Visualizer.Vertices;
        }
        private void OnVislualizerClick(object sender, MouseButtonEventArgs e)
        {
            if (VisualizerClickCommand != null)
            {
                var position = e.GetPosition(Visualizer);
                var vertex = new Vertex
                {
                    X = position.X,
                    Y = position.Y,
                    BorderThickness = new Thickness(Visualizer.DefaultVertexBorderThickness),
                    BorderBrush = Visualizer.DefaultVertexBorderBrush,
                    Background = Visualizer.DefaultVertexBackground,
                    Style = Visualizer.DefaultVertexStyle,
                    Radius = START_NEW_VERTEX_R
                };
                VisualizerClickCommand.Execute(vertex);
                if (vertex.Name != "")
                {
                    Visualizer.AddVertex(vertex);
                }
                vertex.X = position.X;
                vertex.Y = position.Y;

                var animation = SilverlightHelper.GetStoryboard(vertex,
                                                                "Radius",
                                                                20.0,
                                                                0.15,
                                                                null);
                animation.Begin();
            }
            VertVis = Visualizer.Vertices;
        }

    }
}
