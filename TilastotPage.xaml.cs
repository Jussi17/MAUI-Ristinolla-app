using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ristinolla;

public partial class TilastotPage : ContentPage
{
    private ObservableCollection<Pelaaja> pelaajat; // Pelaajat, jotka näytetään PelaajaPickerissä

    public TilastotPage()
    {
        InitializeComponent();
        pelaajat = new ObservableCollection<Pelaaja>(PelaajaManager.LataaPelaajat());
        PelaajaPicker.ItemsSource = pelaajat.Select(p => p.KokoNimi).ToList();
        BindingContext = this;
        TilastotLabel.IsVisible = false; // Tilastot piilossa, kunnes pelaaja valitaan
        StatsGrid.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        TilastotLabel.IsVisible = false; // Tilastot piilossa, kunnes pelaaja valitaan
        StatsGrid.IsVisible = false;
        PelaajaPicker.SelectedIndex = -1; // Tyhjennetään valinta
        base.OnAppearing();
        LataaPelaajat(); 
    }

    private void LataaPelaajat() 
    {
        // Ladataan pelaajat uudelleen, jos niitä on lisätty tai poistettu
        pelaajat = new ObservableCollection<Pelaaja>(PelaajaManager.LataaPelaajat());
        PelaajaPicker.ItemsSource = pelaajat.Select(p => p.KokoNimi).ToList();
        BindingContext = this;
        TilastotLabel.IsVisible = false; // Tilastot piilossa, kunnes pelaaja valitaan
        StatsGrid.IsVisible = false;
    }

    private void PelaajaPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Jos pelaajaa ei ole valittu, piilotetaan tilastot
        if (PelaajaPicker.SelectedIndex == -1)
        {
            TilastotLabel.IsVisible = false;
            StatsGrid.IsVisible = false;
            Borderi.IsVisible = false;
        }
        else
        {
            TilastotLabel.IsVisible = true;
            StatsGrid.IsVisible = true;
            Borderi.IsVisible = true;
            StatsGrid.BindingContext = pelaajat[PelaajaPicker.SelectedIndex]; // Asetetaan BindingContext valitulle pelaajalle
        }
    }
}