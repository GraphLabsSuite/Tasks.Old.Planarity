using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Utils;

namespace GraphLabs.Tasks.Template
{
    public partial class TaskTemplateViewModel
    {
        private const string ImageResourcesPath = @"/GraphLabs.Tasks.Template;component/Images/";

        private Uri GetImageUri(string imageFileName)
        {
            return new Uri(ImageResourcesPath + imageFileName, UriKind.Relative);
        }

        private void InitToolBarCommands()
        {
            ToolBarCommands = new ObservableCollection<ToolBarCommandBase>();

            // Перемещение вершин - toogleCommand
            var moveCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    _state = State.MoveVertex;
                    UserActionsManager.RegisterInfo("Включено перемещение вершин.");
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo("Отключено перемещение вершин.");
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Move.png")),
                Description = "Перемещение вершин"
            };

            //удаление вершин
            var removeVertex = new ToolBarToggleCommand(
                () =>
                {
                    UserActionsManager.RegisterInfo("Нажмите на вершину для удаления (все ее связи удалятся тоже)");
                    _state = State.RemoveVertex;
                },
                () => _state = State.Nothing,
                () => _state == State.Nothing,
                () => _state == State.RemoveVertex
                );
            removeVertex.Image = new BitmapImage(GetImageUri("DontTouch.png"));
            removeVertex.Description = "Удаление вершины";

            //Добавление вершинки
            var testBut = new ToolBarToggleCommand(
                () =>
                {
                    UserActionsManager.RegisterInfo("Нажмите в то место, куда ходите добавить вершину.");
                    _state = State.AddVertex;
                },
                () =>
                {
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo("Добавление вершины отменено");
                },
                () => _state == State.Nothing,
                () => _state == State.AddVertex
                );
            testBut.Image = new BitmapImage(GetImageUri("NewVertex.png"));
            testBut.Description = "Добавить вершину";

            //Добавление ребрышка
            var addEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    _state = State.AddEdge1;
                    UserActionsManager.RegisterInfo("Добавление ребра: Выберите выходную вершину.");

                },
                () =>
                {
                    UserActionsManager.RegisterInfo("Добавление ребра завершено.");
                    _state = State.Nothing;
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("NewEdge.png")),
                Description = "Добавление дуги"
            };

            /*  // Завершение работы
              var finishTask = new ToolBarInstantCommand(
                  () =>
                  {
                      UserActionsManager.ReportThatTaskFinished();
                  },
                  () => _state == State.Nothing
                  )
              {
                  Image = new BitmapImage(GetImageUri("Complete.png")),
                  Description = "Завершить задание"
              };*/


            var checkGraphBut = new ToolBarInstantCommand(
                 CheckGraph,
                 () => _state == State.Nothing
                 )
            {
                Image = new BitmapImage(GetImageUri("CondReady.png")),
                Description = "Проверить правильность построения графа"
            };

            var checkAllBut = new ToolBarInstantCommand(
              CheckPlan,
              () => _state == State.Nothing
              )
            { Image = new BitmapImage(GetImageUri("CondReady.png")), Description = "Проверить, плоский ли граф" };

            ToolBarCommands.Add(moveCommand);
            ToolBarCommands.Add(removeVertex);
            // ToolBarCommands.Add(finishTask);
            ToolBarCommands.Add(testBut);
            ToolBarCommands.Add(addEdgeCommand);
            ToolBarCommands.Add(checkGraphBut);
            ToolBarCommands.Add(checkAllBut);
        }
    }
}
