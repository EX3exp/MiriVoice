using Mirivoice.ViewModels;

namespace Mirivoice.Commands
{
    public enum ExpVITS2
    {
        Speed = 0,
        Noise1 = 1,
        Noise2 = 2,
    }
    public class VITS2SetExpOriginator : MOriginator<int>
    {
        private int index;

        private readonly ExpressionEditViewModelVITS2 v;

        ExpVITS2 type;
        public VITS2SetExpOriginator(ref int index, ExpressionEditViewModelVITS2 v, ExpVITS2 type) : base(ref index)
        {
            this.index = index;
            this.v = v;
            this.type = type;
        }


        public override void UpdateProperties()
        {
            //Log.Debug("[Updating Properties] -- {obj}", obj);
            switch (type)
            {
                case ExpVITS2.Speed:
                    v.NotProcessingSpeedCommand = true; // prevent recursion loop
                    v._vits2Speed = obj;
                    v.OnPropertyChanged(nameof(v.VITS2Speed));
                    break;
                case ExpVITS2.Noise1:
                    v.NotProcessingNoise1Command = true; // prevent recursion loop
                    v._vits2Noise1 = obj;
                    v.OnPropertyChanged(nameof(v.VITS2Noise1));
                    break;
                case ExpVITS2.Noise2:
                    v.NotProcessingNoise2Command = true; // prevent recursion loop
                    v._vits2Noise2 = obj;
                    v.OnPropertyChanged(nameof(v.VITS2Noise2));
                    break;
            }
        }
    }
}
