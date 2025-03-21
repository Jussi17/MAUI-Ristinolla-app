namespace Ristinolla
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadThemePreference();
        }

        private void TeemaToggled(object sender, ToggledEventArgs e)
        {
            bool isDarkMode = e.Value;
            Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("AppTheme", isDarkMode ? "Dark" : "Light");

            if (TeemaSwitch != null)
            {
                TeemaSwitch.OnColor = isDarkMode ? Colors.Red : Colors.White;  
                TeemaSwitch.ThumbColor = isDarkMode ? Colors.White : Colors.Red; 

            }
        }

        private void LoadThemePreference()
        {
            string savedTheme = Preferences.Get("AppTheme", "Light");
            bool isDarkMode = savedTheme == "Dark";

            Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

            if (TeemaSwitch != null)
            {
                TeemaSwitch.IsToggled = isDarkMode; // Asetetaan kytkin oikeaan asentoon
            }
        }
    }
}
