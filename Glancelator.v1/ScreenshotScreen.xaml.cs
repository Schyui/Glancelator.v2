#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;
using Tesseract;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Web.WebView2.Core;
#endif

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading; // For the Timer
using System.Web;
using Glancelator.v1;      // For HttpUtility

namespace Glancelator.v1;

public partial class ScreenshotScreen : ContentPage
{
    private Timer _autoScanTimer;
    private bool isAutoScanning = false;
    public ScreenshotScreen()
	{
		InitializeComponent();
        InitializePickers();
    }
    private void InitializePickers()
    {
        var languages = new List<string>
            {
                "English", "Spanish", "French", "German", "Japanese", "Tagalog",
                "Korean", "Italian", "Portuguese", "Russian", "Arabic"
            };
        FromLanguagePicker.ItemsSource = languages;
        ToLanguagePicker.ItemsSource = languages;
        FromLanguagePicker.SelectedIndex = 0;
        ToLanguagePicker.SelectedIndex = 1;

        var intervals = new List<string> { "2 seconds", "5 seconds" };
        IntervalPicker.ItemsSource = intervals;
        IntervalPicker.SelectedIndex = 1;
    }
    private void OnSearchButtonClicked(object sender, EventArgs e)
    {
        string inputText = UrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(inputText)) { return; }
        Uri uri;
        if (inputText.StartsWith("http://") || inputText.StartsWith("https://")) { uri = new Uri(inputText); }
        else if (inputText.Contains(".")) { uri = new Uri("https://" + inputText); }
        else { uri = new Uri($"https://www.google.com/search?q={HttpUtility.UrlEncode(inputText)}"); }
        MyWebView.Source = uri;
    }

    private async void capture_Screen(object sender, EventArgs e)
	{
		var MainPage = new MainPage();

		await Navigation.PushAsync(MainPage);

	}

    private async void upload_File(object sender, EventArgs e)
    {
        var pickOptions = new PickOptions
        {
            PickerTitle = "Please select a PDF file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".pdf" } }
                })
        };

        try
        {
            var result = await FilePicker.PickAsync(pickOptions);
            if (result != null)
            {
                // For local files, we use the "file:///" URI scheme to open them in the WebView
                MyWebView.Source = new Uri("file:///" + result.FullPath);
            }
        }
        catch (Exception ex)
        {
            TranslatedTextLabel.Text = "Error picking file: " + ex.Message;
        }
    }

    private async void live_Capture(object sender, EventArgs e)
	{

	}
	private async void OnManualScanClicked(object sender, EventArgs e)
	{
        await ScanAndTranslateAsync();
    }
	private async void OnAutoScanToggleClicked(object sender, EventArgs e)
	{
        if (isAutoScanning) { StopAutoScan(); } else { StartAutoScan(); }
    }
    private void StartAutoScan()
    {
        isAutoScanning = true;
        AutoScanToggleButton.Text = "Stop Auto-Scan";
        AutoScanToggleButton.BackgroundColor = Colors.IndianRed;
        SetControlsEnabled(false);
        string selectedInterval = IntervalPicker.SelectedItem.ToString();
        int intervalMilliseconds = (selectedInterval == "2 seconds") ? 2000 : 5000;
        _autoScanTimer = new Timer(AutoScanTimerCallback, null, 0, intervalMilliseconds);
    }

    private void StopAutoScan()
    {
        isAutoScanning = false;
        _autoScanTimer?.Dispose();
        AutoScanToggleButton.Text = "Start Auto-Scan";
        AutoScanToggleButton.BackgroundColor = Colors.MediumSeaGreen;
        SetControlsEnabled(true);
    }
    private void SetControlsEnabled(bool isEnabled)
    {
        ManualScanButton.IsEnabled = isEnabled;
        FromLanguagePicker.IsEnabled = isEnabled;
        ToLanguagePicker.IsEnabled = isEnabled;
        IntervalPicker.IsEnabled = isEnabled;
    }

    private void AutoScanTimerCallback(object state)
    {
        MainThread.BeginInvokeOnMainThread(async () => await ScanAndTranslateAsync());
    }
    private async Task ScanAndTranslateAsync()
        {
#if WINDOWS
            try
            {
                var handler = MyWebView.Handler as Microsoft.Maui.Handlers.WebViewHandler;
                var webView2 = handler?.PlatformView as WebView2;

                if (webView2 != null && webView2.CoreWebView2 != null)
                {
                    string tempFile = Path.Combine(Path.GetTempPath(), "webview_capture.png");
                    using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        await webView2.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream.AsRandomAccessStream());
                    }

                    string tessDataPath = Path.Combine(AppContext.BaseDirectory, "TessData");
                    var fromLanguageName = FromLanguagePicker.SelectedItem.ToString();
                    var tesseractLangCode = GetTesseractLanguageCode(fromLanguageName);
                    using var engine = new TesseractEngine(tessDataPath, tesseractLangCode, EngineMode.Default);
                    using var img = Pix.LoadFromFile(tempFile);
                    using var page = engine.Process(img);
                    string text = page.GetText().Trim();

                    // Clear previous results
                    OriginalTextLabel.Text = "";
                    TranslatedTextLabel.Text = "";

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var toLanguage = ToLanguagePicker.SelectedItem.ToString();

                        // UPDATED: Use the new separated labels
                        OriginalTextLabel.Text = text;
                        TranslatedTextLabel.Text = $"Translating to {toLanguage}...";

                        string translatedText = await TranslateTextAsync(text, toLanguage);

                        TranslatedTextLabel.Text = translatedText;
                    }
                    else
                    {
                       OriginalTextLabel.Text = "No text could be extracted from the WebView.";
                    }
                }
            }
            catch (Exception ex)
            {
                TranslatedTextLabel.Text = "Error: " + ex.Message;
            }
#else
           OriginalTextLabel.Text = "This feature only works on Windows.";
#endif
        }
    private async Task<string> TranslateTextAsync(string textToTranslate, string toLanguageName)
    {
        var webAppUrl = "https://script.google.com/macros/s/AKfycbzIZBm7HZIWazGLvsF8YyPaOaakduSbVbZSVuzETvbve1zXd0ja4GtLYaMRoLmJAS15/exec";
        string GetLanguageCode(string languageName)
        {
            switch (languageName.ToLower())
            {
                case "english": return "en";
                case "spanish": return "es";
                case "french": return "fr";
                case "german": return "de";
                case "japanese": return "ja";
                case "tagalog": return "tl";
                case "korean": return "ko";
                case "italian": return "it";
                case "portuguese": return "pt";
                case "russian": return "ru";
                case "arabic": return "ar";
                default: return "en";
            }
        }
        try
        {
            var encodedText = HttpUtility.UrlEncode(textToTranslate);
            var targetCode = GetLanguageCode(toLanguageName);
            var fullUrl = $"{webAppUrl}?text={encodedText}&target={targetCode}";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(fullUrl);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : $"API Error: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            return $"An exception occurred: {ex.Message}";
        }
    }

    string GetTesseractLanguageCode(string languageName)
    {
        switch (languageName.ToLower())
        {
            case "english": return "eng";
            case "spanish": return "spa";
            case "french": return "fra";
            case "german": return "deu";
            case "japanese": return "jpn";
            case "italian": return "ita";
            case "portuguese": return "por";
            case "russian": return "rus";
            case "korean": return "kor";
            case "arabic": return "ara";
            default: return "eng";
        }
    }
    private bool isAutoTranslateEnabled = false;

    private void OnAutoTranslateToggled(object sender, ToggledEventArgs e)
    {
        isAutoTranslateEnabled = e.Value;

        if (AutoTranslateLabel != null)
            AutoTranslateLabel.Text = isAutoTranslateEnabled ? "Auto" : "Manual";

        // Enable or disable the manual button based on mode
        ManualTranslateButton.IsEnabled = !isAutoTranslateEnabled;
    }

    private async void OnOriginalTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        try
        {
            // Only trigger if auto-translate mode is ON
            if (!isAutoTranslateEnabled)
                return;

            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                TranslatedTextLabel.Text = string.Empty;
                return;
            }

            var toLanguage = ToLanguagePicker.SelectedItem?.ToString() ?? "English";
            var originalText = e.NewTextValue.Trim();

            TranslatedTextLabel.Text = $"Translating to {toLanguage}...";

            string translatedText = await TranslateTextAsync(originalText, toLanguage);

            TranslatedTextLabel.Text = translatedText;
        }
        catch (Exception ex)
        {
            TranslatedTextLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnManualTranslateClicked(object sender, EventArgs e)
    {
        try
        {
            // Manual translation when switch is OFF
            if (isAutoTranslateEnabled)
            {
                TranslatedTextLabel.Text = "Switch to Manual mode to use this button.";
                return;
            }

            string originalText = OriginalTextLabel.Text?.Trim();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                TranslatedTextLabel.Text = "Please enter text first.";
                return;
            }

            var toLanguage = ToLanguagePicker.SelectedItem?.ToString() ?? "English";

            TranslatedTextLabel.Text = $"Translating to {toLanguage}...";

            string translatedText = await TranslateTextAsync(originalText, toLanguage);

            TranslatedTextLabel.Text = translatedText;
        }
        catch (Exception ex)
        {
            TranslatedTextLabel.Text = $"Error: {ex.Message}";
        }
    }




}
