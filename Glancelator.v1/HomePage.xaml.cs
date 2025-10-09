using Glancelator.v1;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Glancelator.v1
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }
        private async void Take_Screenshot(object sender, EventArgs e)
        {
            var ScreenshotScreen = new ScreenshotScreen();

            await Navigation.PushAsync(ScreenshotScreen);
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            var SettingsPage = new Settings();
            await Navigation.PushAsync(SettingsPage);
        }
        

        //// 🔹 Handle Screenshot click
        //private async void OnScreenshotClicked(object sender, EventArgs e)
        //{
        //    await OnTabClicked("Screenshot");
        //}

        //// 🔹 Handle Help click
        //private async void OnHelpClicked(object sender, EventArgs e)
        //{
        //    await OnTabClicked("Help");
        //}


        //// 🔹 Main logic for switching tabs
        /* private async Task OnTabClicked(string tab)
         {
             // Reset all to normal state
            // await ResetAllTabs();

             switch (tab)
             {
                 case "Settings":
                     await HighlightTab(SettingsIcon, SettingsLabel);
                     await Shell.Current.GoToAsync(nameof(Settings));
                     break;

                    /* case "Screenshot":
                         await HighlightTab(ScreenshotIcon, ScreenshotLabel, ScreenshotFrame);
                         break;

                     case "Help":
                         await HighlightTab(HelpIcon, HelpLabel);
                         break; 
            }
        } */

        //// 🔹 Visual highlight + zoom animation
        //private async Task HighlightTab(Image icon, Label label, Frame frame = null)
        //{
        //    label.TextColor = Color.FromArgb("#418B41");
        //    await icon.ScaleTo(1.2, 100, Easing.CubicOut);
        //    await icon.ScaleTo(1.0, 100, Easing.CubicIn);

        //    if (frame != null)
        //        frame.BackgroundColor = Color.FromArgb("#133C13");
        //}

        //// 🔹 Reset all tabs back to default
        //private async Task ResetAllTabs()
        //{
        //    SettingsLabel.TextColor = Colors.Gray;
        //    ScreenshotLabel.TextColor = Colors.Gray;
        //    HelpLabel.TextColor = Colors.Gray;

        //    ScreenshotFrame.BackgroundColor = Color.FromArgb("#D0D0D0");

        //    await Task.CompletedTask;
        //}
    }
}
