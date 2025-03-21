using Microsoft.Maui.Graphics.Text;

namespace Ristinolla;

public partial class MatsiPage : ContentPage
{
    private readonly MatsiViewModel viewModel;
    public bool onPaattynyt = false;
    public bool onNaytetty = false;
    private readonly Random random = new Random();

    public MatsiPage(Pelaaja pelaaja1, Pelaaja pelaaja2, double vaikeustasoX, double vaikeustasoO) // Tiedot pelaajista ja vaikeustasoista AsetuksetPagesta
    {
        InitializeComponent();
        viewModel = new MatsiViewModel(pelaaja1, pelaaja2, this, vaikeustasoX, vaikeustasoO);
        BindingContext = viewModel;
    }

    // OnAppearing: Triggeröi tietokoneen ensimmäisen siirron
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (viewModel.TietokoneVsTietokone())
        {
            await TietokoneVsTietokone();
        }
        else if (!onPaattynyt && viewModel.TietokoneenVuoro())
        {
            int viive = random.Next(1200, 2001); // Random viive ensimmäiseen siirtoon 1200ms-2000ms
            await Task.Delay(viive); // Aloituksessa pieni luonnollinen viive
            viewModel.TietokoneenSiirto();
        }
    }
    private async Task TietokoneVsTietokone() // Tietokone vs tietokone
    {
        while (!viewModel.OnkoLoppu())
        {
            viewModel.TietokoneenSiirto();
            int viive = random.Next(500, 2001); // Random viive 500ms-2000ms
            await Task.Delay(viive); 
        }

        if (viewModel.TarkistaVoitto() && !onNaytetty)
        {
            var winner = viewModel.NykyinenPelaaja == "X" ? viewModel.PelaajaX.KokoNimi : viewModel.PelaajaO.KokoNimi;
            await DisplayAlert("Voitto!", $"{winner} voitti!", "OK");
        }
        else if (viewModel.OnkoTaysi() && !onNaytetty)
        {
            await DisplayAlert("Tasapeli", "Peli päättyi tasapeliin.", "OK");
        }
        onPaattynyt = true;
    }

    private async void KentanKlikkaus(object sender, EventArgs e)
    {
        if (viewModel.TietokoneVsTietokone() || onPaattynyt || sender is not Button button)
            return; // Poistetaan klikkaus mahdollisuus, jos tietokone pelaa itseään vastaan tai peli on päättynyt

        int rivi = Grid.GetRow(button);
        int kolumni = Grid.GetColumn(button);
        if (!string.IsNullOrEmpty(viewModel.Kentta[rivi, kolumni]))
            return;

        // Ihmispelaajan siirto
        viewModel.TeeSiirto(rivi, kolumni, button);

        // Jos peli ei ole loppunut, tietokoneen siirto
        if (!onPaattynyt && viewModel.TietokoneenVuoro())
        {
            int viive = random.Next(500, 2001); // Random viive 500ms-2000ms
            await Task.Delay(viive); 
            viewModel.TietokoneenSiirto();
        }
    }

    private async void UusiPeli(object sender, EventArgs e)
    {
        // Annetaan nykyisen pelin parametrit uuden pelin luomiseksi
        await Navigation.PushAsync(new MatsiPage(viewModel.PelaajaX, viewModel.PelaajaO, viewModel.VaikeustasoX, viewModel.VaikeustasoO));
        Navigation.RemovePage(this); // Poistetaan vanha sivu
    }

    private void PeliLoppu() // Peli päättyi - estetään lisäsiirrot silmukassa
    {
        onPaattynyt = true;
        onNaytetty = true;
        foreach (var child in (Content as VerticalStackLayout).Children.OfType<Grid>().First().Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = false;
            }
        }
    }

    public void PaivitysNappi(int row, int col, string value) // Päivitetään buttoni ja vaihdetaan väri
    {
        var grid = (Content as VerticalStackLayout).Children.OfType<Grid>().First();
        foreach (var child in grid.Children)
        {
            if (child is Button button && Grid.GetRow(button) == row && Grid.GetColumn(button) == col)
            {
                button.Text = value;
                button.TextColor = value == "X" ? Colors.Pink : Colors.LightGreen;
                break;
            }
        }

        if (viewModel.TarkistaVoitto()) // Tarkistaa voiton
        {
            DisplayAlert("Voitto!", $"{(viewModel.NykyinenPelaaja == "X" ? viewModel.PelaajaX.KokoNimi : viewModel.PelaajaO.KokoNimi)} voitti!", "OK");
            viewModel.PaivitaTilastot(viewModel.NykyinenPelaaja == "X" ? viewModel.PelaajaX : viewModel.PelaajaO, true);
            PeliLoppu();
        }
        else if (viewModel.OnkoTaysi()) // Tarkistaa tasapelin
        {
            DisplayAlert("Tasapeli", "Peli päättyi tasapeliin.", "OK");
            viewModel.PaivitaTilastot(null, false);
            PeliLoppu();
        }
    }
}