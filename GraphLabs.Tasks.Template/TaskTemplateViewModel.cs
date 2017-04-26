using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using GraphLabs.Common;
using GraphLabs.Common.Utils;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Common.UserActionsRegistrator;
using GraphLabs.Graphs;
using GraphLabs.Utils;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using Edge = GraphLabs.Graphs.Edge;
using Vertex = GraphLabs.Graphs.Vertex;

namespace GraphLabs.Tasks.Template
{
    /// <summary> ViewModel для TaskTemplate </summary>
    public partial class TaskTemplateViewModel : TaskViewModelBase<TaskTemplate>
    {
        /// <summary> Текущее состояние </summary>
        private enum State
        {
            /// <summary> Пусто </summary>
            Nothing,
            /// <summary> Перемещение вершин </summary>
            MoveVertex,
            RemoveVertex,
            AddVertex,
            /// <summary> Добавление связи (этап 1) </summary>
            AddEdge1,
            /// <summary> Добавление связи (этап 2) </summary>
            AddEdge2,
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = { new Version(1, 0) };

        /// <summary> Допустимые версии генератора </summary>
        protected override Version[] AllowedGeneratorVersions
        {
            get { return _allowedGeneratorVersions; }
        }


        #region Public свойства вьюмодели

        /// <summary> Идёт загрузка данных? </summary>
        public static readonly DependencyProperty IsLoadingDataProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsLoadingData),
            typeof(bool),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Разрешено перемещение вершин? </summary>
        public static readonly DependencyProperty IsMouseVerticesMovingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsMouseVerticesMovingEnabled),
            typeof(bool),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Команды панели инструментов</summary>
        public static readonly DependencyProperty ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));

        /// <summary> Выданный в задании граф </summary>
        public static readonly DependencyProperty GivenGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.GivenGraph),
            typeof(IGraph),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(IGraph)));


        /// <summary> Идёт загрузка данных? </summary>
        public bool IsLoadingData
        {
            get { return (bool)GetValue(IsLoadingDataProperty); }
            private set { SetValue(IsLoadingDataProperty, value); }
        }

        /// <summary> Разрешено перемещение вершин? </summary>
        public bool IsMouseVerticesMovingEnabled
        {
            get { return (bool)GetValue(IsMouseVerticesMovingEnabledProperty); }
            set { SetValue(IsMouseVerticesMovingEnabledProperty, value); }
        }

        /// <summary> Команды панели инструментов </summary>
        public ObservableCollection<ToolBarCommandBase> ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(ToolBarCommandsProperty); }
            set { SetValue(ToolBarCommandsProperty, value); }
        }

        /// <summary> Выданный в задании граф </summary>
        public IGraph GivenGraph
        {
            get { return (IGraph)GetValue(GivenGraphProperty); }
            set { SetValue(GivenGraphProperty, value); }
        }

        //до этого ничего существенно не было изменено 

        /// <summary> Вершины из визуализатора </summary>
        public static DependencyProperty VertVisColProperty =
            DependencyProperty.Register("VertVisCol",
            typeof(ReadOnlyCollection<Graphs.UIComponents.Visualization.Vertex>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ReadOnlyCollection<Graphs.UIComponents.Visualization.Vertex>)));

        /// <summary> Вершины из визуализатора </summary>
        public ReadOnlyCollection<Graphs.UIComponents.Visualization.Vertex> VertVisCol
        {
            get { return (ReadOnlyCollection<Graphs.UIComponents.Visualization.Vertex>)GetValue(VertVisColProperty); }
            set { SetValue(VertVisColProperty, value); }
        }


        /// <summary> Клик по вершине </summary>
        public static readonly DependencyProperty VertexClickCmdProperty =
            DependencyProperty.Register("VertexClickCmd", typeof(ICommand), typeof(TaskTemplateViewModel), new PropertyMetadata(default(ICommand)));

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public static readonly DependencyProperty OnLoadedCmdProperty =
            DependencyProperty.Register("OnLoadedCmd", typeof(ICommand), typeof(TaskTemplateViewModel), new PropertyMetadata(default(ICommand)));

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public ICommand OnLoadedCmd
        {
            get { return (ICommand)GetValue(OnLoadedCmdProperty); }
            set { SetValue(OnLoadedCmdProperty, value); }
        }

        /// <summary> Клик по вершине </summary>
        public ICommand VertexClickCmd
        {
            get { return (ICommand)GetValue(VertexClickCmdProperty); }
            set { SetValue(VertexClickCmdProperty, value); }
        }
        /// <summary> Клик по визуализатору </summary>
        public static readonly DependencyProperty VisualizerClickCmdProperty = DependencyProperty.Register(
           "VisualizerClickCmd",
           typeof(DelegateCommand),
           typeof(TaskTemplateViewModel),
           new PropertyMetadata(default(DelegateCommand)));

        /// <summary> Клик по визуализатору </summary>
        public DelegateCommand VisualizerClickCmd
        {
            get { return (DelegateCommand)GetValue(VisualizerClickCmdProperty); }
            set { SetValue(VisualizerClickCmdProperty, value); }
        }
        /// <summary> Заполняемая студентом матрица </summary>
        public static readonly DependencyProperty MatrixProperty = DependencyProperty.Register(
            "Matrix",
            typeof(ObservableCollection<MatrixRowViewModel<string>>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<MatrixRowViewModel<string>>)));

        /// <summary> Заполняемая студентом матрица </summary>
        public ObservableCollection<MatrixRowViewModel<string>> Matrix
        {
            get { return (ObservableCollection<MatrixRowViewModel<string>>)GetValue(MatrixProperty); }
            set { SetValue(MatrixProperty, value); }
        }
        #endregion


        //добавление-удаление
        /// <summary> Инициализация </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            InitToolBarCommands();

            OnLoadedCmd = new DelegateCommand(
                          o =>
                          {
                              VariantProvider.DownloadVariantAsync();
                              ToolBarCommands.ForEach(c => c.RefreshState());
                          }, o => true);
            var outV = new Vertex("a");
            VertexClickCmd = new DelegateCommand(
                o =>
                {
                    if (_state == State.AddEdge1)
                    {
                        outV = GivenGraph.Vertices.Single(v => v.Name == ((IVertex)o).Name) as Vertex;
                        VertVisCol.Single(v => v.Name == outV.Name).Radius = 23;
                        VertVisCol.Single(v => v.Name == outV.Name).Background = new SolidColorBrush(Colors.Magenta);
                        UserActionsManager.RegisterInfo(string.Format("Выходная вершина - [{0}]. Выберите входную вершину.", ((IVertex)o).Name));
                        _state = State.AddEdge2;
                        return;
                    }
                    if (_state == State.AddEdge2)
                    {
                        var inV = GivenGraph.Vertices.Single(v => v.Name == ((IVertex)o).Name) as Vertex;
                        UserActionsManager.RegisterInfo(string.Format("Входная вершина - [{0}]. Добавьте другое ребро или выйдете из режима добавления ребер", ((IVertex)o).Name));
                        var newEdge = new DirectedEdge(outV, inV);
                        if (GivenGraph.Edges.Any(newEdge.Equals))
                        {
                            ReportMistake("Указанная дуга уже существует.");
                            return;
                        }
                        VertVisCol.Single(v => v.Name == outV.Name).Radius = 20;
                        VertVisCol.Single(v => v.Name == outV.Name).Background = new SolidColorBrush(Colors.LightGray);

                        GivenGraph.AddEdge(newEdge);
                        _state = State.AddEdge1;
                    }
                    if (_state == State.RemoveVertex)
                    {
                        GivenGraph.RemoveVertex(GivenGraph.Vertices.Single(v => v.Name == ((IVertex)o).Name));
                        UserActionsManager.RegisterInfo(string.Format("Вершина [{0}] удалена со всеми связями", ((IVertex)o).Name));
                    }
                },
                    o => true);
            VisualizerClickCmd = new DelegateCommand(
                o =>
                {
                    if (_state == State.AddVertex)
                    {
                        UserActionsManager.RegisterInfo((string.Format("Вершина добавлена")));
                        var vertex = (Graphs.UIComponents.Visualization.Vertex)o;
                        vertex.Name = (GivenGraph.VerticesCount).ToString();
                        int i = 0;
                        while (GivenGraph.Vertices.Any(vertex.Equals))
                        {
                            i = i + 1;
                            vertex.Name = (i).ToString();
                        }
                    }
                },
                o => true);

        }
        //конец добавлени-удаления

        /// <summary> Задание загружено </summary>
        /// <param name="e"></param>
        protected override void OnTaskLoadingComlete(VariantDownloadedEventArgs e)
        {

            // Мы вызваны из другого потока. Поэтому работаем с UI-элементами через Dispatcher.
            Dispatcher.BeginInvoke(() => { GivenGraph = VariantSerializer.Deserialize(e.Data)[0]; });

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;


            GivenGraph = DirectedGraph.CreateEmpty(0);
            MatrixGraph = (DirectedGraph)VariantSerializer.Deserialize(e.Data)[0];

            Matrix = new ObservableCollection<MatrixRowViewModel<string>>();
            for (var i = 0; i < MatrixGraph.VerticesCount; ++i)
            {
                var row = new ObservableCollection<string> { i.ToString() };
                for (var j = 0; j < MatrixGraph.VerticesCount; ++j)
                {
                    var testEdge = new DirectedEdge(MatrixGraph.Vertices[i], MatrixGraph.Vertices[j]);
                    row.Add(MatrixGraph.Edges.Any(testEdge.Equals)
                        //row.Add((MatrixGraph[MatrixGraph.Vertices[i],MatrixGraph.Vertices[j]] != null)
                        ? "1"
                        : "0");
                }
                row.CollectionChanged += RowChanged;
                Matrix.Add(new MatrixRowViewModel<string>(row));
            }
        }



        private void RecalculateIsLoadingData()
        {
            IsLoadingData = VariantProvider.IsBusy || UserActionsManager.IsBusy;
        }

        private ObservableCollection<string> _changedCollection;
        private NotifyCollectionChangedEventArgs _cellChangedArgs;
        private void RowChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _cellChangedArgs = e;
            _changedCollection = (ObservableCollection<string>)sender;
        }

        private DirectedGraph MatrixGraph;

        private void ReportMistake(string message)
        {
            MessageBox.Show("Вы допустили ошибку!\n" + message);
        }



        private void CheckGraph()
        {
            if (MatrixGraph.VerticesCount != GivenGraph.VerticesCount)
            {
                MessageBox.Show("Проверка графа завершена!\n" + "Неправильное количество вершин");
                UserActionsManager.RegisterMistake("Ошибка!", 10);
                return;
            }
            for (int i = 0; i < MatrixGraph.EdgesCount; ++i)
            {
                if (!GivenGraph.Edges.Any(MatrixGraph.Edges[i].Equals))
                {
                    MessageBox.Show("Проверка графа завершена!\n" + "Не хватает дуги: " + MatrixGraph.Edges[i].Vertex1.Name + "->" + MatrixGraph.Edges[i].Vertex2.Name);
                    UserActionsManager.RegisterMistake("Ошибка!", 5);
                    return;
                }
            }
            for (int i = 0; i < GivenGraph.EdgesCount; ++i)
            {
                if (!MatrixGraph.Edges.Any(GivenGraph.Edges[i].Equals))
                {
                    MessageBox.Show("Проверка графа завершена!\n" + "Лишняя дуга: " + GivenGraph.Edges[i].Vertex1.Name + "->" + GivenGraph.Edges[i].Vertex2.Name);
                    UserActionsManager.RegisterMistake("Ошибка!", 5);
                    return;
                }
            }
            MessageBox.Show("Проверка графа завершена!\n" + "Вы молодец!");
            _state = State.Nothing;
        }

        public void CheckPlan()
        {
            for (int i = 0; i < GivenGraph.EdgesCount; ++i)
            {
                for (int j = 0; j < GivenGraph.EdgesCount; ++j)
                {

                    var V1 = VertVisCol.Single(v => v.Name == GivenGraph.Edges[i].Vertex1.Name);
                    var V2 = VertVisCol.Single(v => v.Name == GivenGraph.Edges[i].Vertex2.Name);
                    var V3 = VertVisCol.Single(v => v.Name == GivenGraph.Edges[j].Vertex1.Name);
                    var V4 = VertVisCol.Single(v => v.Name == GivenGraph.Edges[j].Vertex2.Name);

                    if (HaveCollision(V1, V2, V3, V4))
                    {
                        MessageBox.Show("Проверка графа завершена!\n" + "Граф не плоский");
                        UserActionsManager.RegisterMistake("Ошибка!", 10);
                        return;
                    }
                }
            }
            MessageBox.Show("Проверка графа завершена!\n" + "Граф плоский! Поздравляем!");
        }
        /*private void SubscribeToViewEvents()
         {
             View.VertexClicked += (sender, args) => OnVertexClick(args.Control);
             View.Loaded += (sender, args) => StartVariantDownload();
         }


        /// <summary> Начать загрузку варианта </summary>
        public void StartVariantDownload()
          {
              VariantProvider.DownloadVariantAsync();
          }

        /// <summary> Клик по вершине </summary>
        public void OnVertexClick(IVertex vertex)
        {
            UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}]", vertex.Name));
        }

        private void HandlePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ExpressionUtility.NameForMember((IUiBlockerAsyncProcessor p) => p.IsBusy))
            {
                // Нас могли дёрнуть из другого потока, поэтому доступ к UI - через Dispatcher.
                Dispatcher.BeginInvoke(RecalculateIsLoadingData);
            }
        }*/

        private bool HaveCollision(Graphs.UIComponents.Visualization.Vertex a, Graphs.UIComponents.Visualization.Vertex b, Graphs.UIComponents.Visualization.Vertex c, Graphs.UIComponents.Visualization.Vertex d)
        {
            double A1 = b.Y - a.Y;
            double B1 = a.X - b.X;
            double C1 = -A1 * a.X - B1 * a.Y;

            double A2 = d.Y - c.Y;
            double B2 = c.X - d.X;
            double C2 = -A2 * c.X - B2 * c.Y;

            double f1 = A1 * c.X + B1 * c.Y + C1;
            double f2 = A1 * d.X + B1 * d.Y + C1;
            double f3 = A2 * a.X + B2 * a.Y + C2;
            double f4 = A2 * b.X + B2 * b.Y + C2;

            return f1 * f2 < 0 && f3 * f4 < 0;
        }


    }
}
