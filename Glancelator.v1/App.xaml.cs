namespace Glancelator.v1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MenuTabbedPage());
            
            //MainPage = new MenuTabbedPage();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //Original and if gagamitin tabBar instead of TabbedPage
            //return new Window(new AppShell());

            //TabbedPage
            return new Window(MainPage);
            

        }
    }
}