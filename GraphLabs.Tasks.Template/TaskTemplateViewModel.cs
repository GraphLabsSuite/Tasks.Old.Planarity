using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using GraphLabs.Common;
using GraphLabs.Common.Utils;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Utils;
using GraphLabs.Graphs.DataTransferObjects.Converters;

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
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = {  new Version(1, 0) };
        
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

        public static readonly DependencyProperty OnLoadedCmdProperty =
            DependencyProperty.Register("OnLoadedCmd", typeof(ICommand), typeof(TaskTemplateViewModel), new PropertyMetadata(default(ICommand)));

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


         /*private void SubscribeToViewEvents()
          {
              View.VertexClicked += (sender, args) => OnVertexClick(args.Control);
              View.Loaded += (sender, args) => StartVariantDownload();
          }*/

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
        }



        /// <summary> Задание загружено </summary>
        /// <param name="e"></param>
        protected override void OnTaskLoadingComlete(VariantDownloadedEventArgs e)
        {
            /* Изначально было
            // Мы вызваны из другого потока. Поэтому работаем с UI-элементами через Dispatcher.
            Dispatcher.BeginInvoke(() => { GivenGraph = VariantSerializer.Deserialize(e.Data)[0]; });

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
            */

            GivenGraph = DirectedGraph.CreateEmpty(0);
            MatrixGraph = (DirectedGraph)GraphSerializer.Deserialize(e.Data);

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
    }
}
