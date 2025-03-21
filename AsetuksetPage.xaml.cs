namespace Ristinolla;

public partial class AsetuksetPage : ContentPage
{
    private List<Pelaaja> pelaajat;
    public double vaikeustasoX = 0.5; // Vaikeustasot 0-1, oletus 0.5
    public double vaikeustasoO = 0.5;

    public AsetuksetPage()
    {
        InitializeComponent();
        LataaPelaajat();
    }

    private void LataaPelaajat()
    {
        // Haetaan kaikki pelaajat ja asetetaan ne PelaajaPickereihin
        pelaajat = PelaajaManager.LataaPelaajat();
        Pelaaja1Picker.ItemsSource = pelaajat.Select(p => p.KokoNimi).ToList();
        Pelaaja2Picker.ItemsSource = pelaajat.Select(p => p.KokoNimi).ToList();
        // Uuden pelaajan lisäämisen kentät piiloon aluksi
        UusiPelaajaGrid.IsVisible = false;
        UusiPelaaja2Grid.IsVisible = false;
    }

    private void VaikeusMuuttunut(object sender, ValueChangedEventArgs e)
    {
        vaikeustasoX = e.NewValue; // Vasemman puolen tietokoneen vaikeustaso
    }

    private void Vaikeus2Muuttunut(object sender, ValueChangedEventArgs e)
    {
        vaikeustasoO = e.NewValue; // Oikean puolen tietokoneen vaikeustaso
    }

    private void VastustajaSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        Pelaaja2Picker.SelectedIndex = 0; // Oletusarvo ensimmäinen pelaaja
        if (e.Value) // Oikean puolen pelaajan valinta
        {
            Pelaaja2Picker.IsVisible = true;
            UusiPelaaja2Switch.IsVisible = true;
            UusiPelaaja2Label.IsVisible = true;
            Vaikeustaso2Slider.IsVisible = false;
            Vaikeustaso2Label.IsVisible = false;
        }
        else // Oikean puolen pelaaja on tietokone
        {
            Pelaaja2Picker.IsVisible = false;
            UusiPelaaja2Switch.IsToggled = false;
            UusiPelaaja2Switch.IsVisible = false;
            UusiPelaaja2Label.IsVisible = false;
            Vaikeustaso2Slider.IsVisible = true;
            Vaikeustaso2Label.IsVisible = true;
        }
    }

    private void PelaajaSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        Pelaaja1Picker.SelectedIndex = 0; // Oletusarvo ensimmäinen pelaaja
        if (e.Value) // Vasemman puolen pelaajan valinta
        {
            Pelaaja1Picker.IsVisible = true;
            UusiPelaajaSwitch.IsVisible = true;
            UusiPelaajaLabel.IsVisible = true;
            VaikeustasoSlider.IsVisible = false;
            VaikeustasoLabel.IsVisible = false;
        }
        else // Vasemman puolen pelaaja on tietokone
        {
            Pelaaja1Picker.IsVisible = false;
            UusiPelaajaSwitch.IsToggled = false;
            UusiPelaajaSwitch.IsVisible = false;
            UusiPelaajaLabel.IsVisible = false;
            VaikeustasoSlider.IsVisible = true;
            VaikeustasoLabel.IsVisible = true;
        }
    }

    private void UusiPelaajaSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        // Uuden pelaajan lisääminen vasemmalle puolelle
        if (Pelaaja1Picker.SelectedIndex == -1)
        {
            UusiPelaajaGrid.IsVisible = e.Value;
        }
        else
        {
            Pelaaja1Picker.SelectedIndex = -1;
            UusiPelaajaGrid.IsVisible = e.Value;
        }
    }

    private void UusiPelaaja2Switch_Toggled(object sender, ToggledEventArgs e)
    {
        // Uuden pelaajan lisääminen oikealle puolelle
        if (Pelaaja2Picker.SelectedIndex == -1)
        {
            UusiPelaaja2Grid.IsVisible = e.Value;
        }
        else
        {
            Pelaaja2Picker.SelectedIndex = -1;
            UusiPelaaja2Grid.IsVisible = e.Value;
        }
    }

    private void Pelaaja1Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Vasemman puolen pelaajan valinta Pickeristä
        if (Pelaaja1Picker.SelectedIndex != -1)
        {
            UusiPelaajaSwitch.IsToggled = false;
            EtuNimiEntry.Text = string.Empty;
            SukuNimiEntry.Text = string.Empty;
            SyntymaVuosiEntry.Text = string.Empty;

            // jos tietokone valitaan, niin kytkin pois päältä ja tyhjennetään valinta
            var selectedPlayer = pelaajat[Pelaaja1Picker.SelectedIndex];
            if (selectedPlayer.Etunimi == "Tietokone")
            {
                PelaajaSwitch.IsToggled = false;
                Pelaaja1Picker.SelectedIndex = -1;
            }
        }
    }

    private void Pelaaja2Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Oikean puolen pelaajan valinta Pickeristä
        if (Pelaaja2Picker.SelectedIndex != -1) 
        {
            UusiPelaaja2Switch.IsToggled = false;
            EtuNimi2Entry.Text = string.Empty;
            SukuNimi2Entry.Text = string.Empty;
            SyntymaVuosi2Entry.Text = string.Empty;

            // jos tietokone valitaan, niin kytkin pois päältä
            var selectedPlayer = pelaajat[Pelaaja2Picker.SelectedIndex];
            if (selectedPlayer.Etunimi == "Tietokone")
            {
                VastustajaSwitch.IsToggled = false;
                Pelaaja2Picker.SelectedIndex = -1;
            }
        }
    }

    private void Tallenna_Clicked(object sender, EventArgs e) // Tallennetaan pelaaja listaan
    {
        var etuNimi = EtuNimiEntry.Text?.Trim(); // Poistetaan tyhjät välilyönnit
        var sukuNimi = SukuNimiEntry.Text?.Trim();

        // Tarkistetaan, että syntymävuosi on numero
        if (!int.TryParse(SyntymaVuosiEntry.Text, out var syntymaVuosi))
        {
            DisplayAlert("Virhe", "Syntymävuosi on virheellinen.", "OK");
            return;
        }

        // Tarkistetaan, että etunimi ja sukunimi eivät ole tyhjiä
        if (string.IsNullOrEmpty(etuNimi) || string.IsNullOrEmpty(sukuNimi))
        {
            DisplayAlert("Virhe", "Etunimi ja sukunimi ovat pakollisia.", "OK");
            return;
        }

        // Tarkistetaan, että etunimi ja sukunimi sisältävät vain kirjaimia (ei numeroita)
        if (!etuNimi.All(char.IsLetter) || !sukuNimi.All(char.IsLetter))
        {
            DisplayAlert("Virhe", "Etunimessä ja sukunimessä saa olla vain kirjaimia.", "OK");
            return;
        }

        // Viedään tiedot PelaajaManagerin EtsiTaiLuoPelaaja metodiin
        var uusiPelaaja = PelaajaManager.EtsiTaiLuoPelaaja(etuNimi, sukuNimi, syntymaVuosi);
        pelaajat.Add(uusiPelaaja);
        PelaajaManager.TallennaPelaajat(pelaajat);
        LataaPelaajat();
        DisplayAlert("Onnistui", "Pelaaja tallennettu onnistuneesti.", "OK");

        // Tyhjennetään kentät
        EtuNimiEntry.Text = string.Empty;
        SukuNimiEntry.Text = string.Empty;
        SyntymaVuosiEntry.Text = string.Empty;
        UusiPelaajaSwitch.IsToggled = false;
        UusiPelaajaGrid.IsVisible = false;
    }

    private void Tallenna2_Clicked(object sender, EventArgs e) // Sama kuin edellisessä metodissa, mutta oikealle puolelle
    {
        var etuNimi = EtuNimi2Entry.Text?.Trim();
        var sukuNimi = SukuNimi2Entry.Text?.Trim();
        if (!int.TryParse(SyntymaVuosi2Entry.Text, out var syntymaVuosi))
        {
            DisplayAlert("Virhe", "Syntymävuosi on virheellinen.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(etuNimi) || string.IsNullOrEmpty(sukuNimi))
        {
            DisplayAlert("Virhe", "Etunimi ja sukunimi ovat pakollisia.", "OK");
            return;
        }

        var uusiPelaaja = PelaajaManager.EtsiTaiLuoPelaaja(etuNimi, sukuNimi, syntymaVuosi);
        pelaajat.Add(uusiPelaaja);
        PelaajaManager.TallennaPelaajat(pelaajat);
        LataaPelaajat();
        DisplayAlert("Onnistui", "Pelaaja tallennettu onnistuneesti.", "OK");
        EtuNimi2Entry.Text = string.Empty;
        SukuNimi2Entry.Text = string.Empty;
        SyntymaVuosi2Entry.Text = string.Empty;
        UusiPelaaja2Switch.IsToggled = false;
        UusiPelaaja2Grid.IsVisible = false;
    }

    private async void AloitaPeli_Clicked(object sender, EventArgs e)
    {
        // Pelin aloitus, logiikka tarkistaa pelaajat ja vaikeustasot
        Pelaaja pelaajaX;
        if (PelaajaSwitch.IsToggled && Pelaaja1Picker.SelectedIndex != -1)
        {
            pelaajaX = pelaajat[Pelaaja1Picker.SelectedIndex];
        }
        else
        {
            pelaajaX = PelaajaManager.EtsiTaiLuoPelaaja("Tietokone", "", 2025);
        }

        Pelaaja pelaajaO;
        if (VastustajaSwitch.IsToggled && Pelaaja2Picker.SelectedIndex != -1)
        {
            pelaajaO = pelaajat[Pelaaja2Picker.SelectedIndex];
        }
        else
        {
            pelaajaO = PelaajaManager.EtsiTaiLuoPelaaja("Tietokone", "", 2025);
        }

        // Viedään pelaajat ja vaikeustasot MatsiPageen
        await Navigation.PushAsync(new MatsiPage(pelaajaX, pelaajaO, vaikeustasoX, vaikeustasoO));
    }
}