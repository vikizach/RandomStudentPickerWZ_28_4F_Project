using Microsoft.Extensions.DependencyInjection;

namespace RandomStudentPickerWZ_28_4F
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new RandomStudentPickerWZ_28_4F.Views.HomePage());
        }
    }
}