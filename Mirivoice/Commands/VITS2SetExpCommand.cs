using Avalonia;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Serilog;

namespace Mirivoice.Commands
{
    public enum ExpVITS2
    {
        Speed = 0,
        Noise1 = 1,
        Noise2 = 2,
    }

    public class VITS2SetExpCommand : ICommand
    {
        int undoMem;
        int redoMem;

        readonly ExpressionEditViewModelVITS2 viewModel;
        readonly ExpVITS2 mode;
        public VITS2SetExpCommand(ExpressionEditViewModelVITS2 viewModel, ExpVITS2 mode)
        {
            //Log.Debug("LineEditCommand created");
            this.mode = mode;
            this.viewModel = viewModel;
            switch (mode)
            {
                case ExpVITS2.Speed:
                    undoMem = viewModel.VITS2Speed;
                    break;
                case ExpVITS2.Noise1:
                    undoMem = viewModel.VITS2Noise1;
                    break;
                case ExpVITS2.Noise2:
                    undoMem = viewModel.VITS2Noise2;
                    break;
                default:
                    Log.Error("VITS2 Exp editor -- Invalid mode");
                    break;
            }
        }
        

        bool isFirstExec = true;
        public void Execute(bool isRedoing)
        {
            if (isRedoing)
            {
                switch (mode)
                {
                    case ExpVITS2.Speed:
                        viewModel.VITS2Speed = redoMem;
                        break;
                    case ExpVITS2.Noise1:
                        viewModel.VITS2Noise1 = redoMem;
                        break;
                    case ExpVITS2.Noise2:
                        viewModel.VITS2Noise2 = redoMem;
                        break;
                    default:
                        Log.Error("VITS2 Exp editor -- Invalid mode");
                        break;
                }
            }
            else
            {
                if (!isFirstExec)
                {
                    switch (mode)
                    {
                        case ExpVITS2.Speed:
                            undoMem = viewModel.VITS2Speed;
                            break;
                        case ExpVITS2.Noise1:
                            undoMem = viewModel.VITS2Noise1;
                            break;
                        case ExpVITS2.Noise2:
                            undoMem = viewModel.VITS2Noise2;
                            break;
                        default:
                            Log.Error("VITS2 Exp editor -- Invalid mode");
                            break;
                    }
                }
                isFirstExec = false;
            }
        }

        public void UnExecute()
        {
            switch (mode)
            {
                case ExpVITS2.Speed:
                    redoMem = viewModel.VITS2Speed;
                    viewModel.VITS2Speed = undoMem;
                    break;
                case ExpVITS2.Noise1:
                    redoMem = viewModel.VITS2Noise1;
                    viewModel.VITS2Noise1 = undoMem;
                    break;
                case ExpVITS2.Noise2:
                    redoMem = viewModel.VITS2Noise2;
                    viewModel.VITS2Noise2 = undoMem;
                    break;
                default:
                    Log.Error("VITS2 Exp editor -- Invalid mode");
                    break;
            }
        }
    }

}
