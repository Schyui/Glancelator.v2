namespace Glancelator.v1;

public partial class Settings : ContentPage
{
    const string THEME_PREF_KEY = "user_theme_is_dark";

    public Settings()
    {
        InitializeComponent();
        bool isDark = Preferences.Get(THEME_PREF_KEY, false);
        DarkModeSwitch.IsToggled = isDark;

    }
    private void DarkModeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool useDark = e.Value;
        Application.Current.UserAppTheme = useDark ? AppTheme.Dark : AppTheme.Light;
        Preferences.Set(THEME_PREF_KEY, useDark);
    }



    /*
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    } */
}   