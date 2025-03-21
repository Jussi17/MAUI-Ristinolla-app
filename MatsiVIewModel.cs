using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ristinolla;

public class MatsiViewModel : INotifyPropertyChanged
{
    private string nykyinenPelaaja;        
    private string nykyinenVuoroText;      
    private string matsiTitle;             
    private readonly MatsiPage page;       
    private readonly Random random = new Random();

    // Kun muutetaan ominaisuuden arvoa, kutsutaan PropertyChanged-tapahtumaa
    public event PropertyChangedEventHandler PropertyChanged;

    // Kutsutaan kun ominaisuuden arvo muuttuu käyttöliittymän päivittämiseksi
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyNimi = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyNimi));
    }

    // Asettaa ominaisuuden arvon ja ilmoittaa muutoksesta
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyNimi = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyNimi);
        return true;
    }

    public Pelaaja PelaajaX { get; }            
    public Pelaaja PelaajaO { get; }            
    public double VaikeustasoX { get; }         // X-pelaajan vaikeustaso
    public double VaikeustasoO { get; }         // O-pelaajan vaikeustaso
    public string[,] Kentta { get; private set; } = new string[3, 3];  // 3x3 pelikenttä
    public DateTime AloitusAika { get; private set; }                   

    public string NykyinenPelaaja
    {
        get => nykyinenPelaaja;
        set => SetProperty(ref nykyinenPelaaja, value);
    }


    public string NykyinenVuoroText
    {
        get => nykyinenVuoroText;
        set => SetProperty(ref nykyinenVuoroText, value);
    }

    public string MatsiTitle
    {
        get => matsiTitle;
        set => SetProperty(ref matsiTitle, value);
    }

    public MatsiViewModel(Pelaaja pelaaja1, Pelaaja pelaaja2, MatsiPage page, double vaikeustasoX, double vaikeustasoO)
    {
        PelaajaX = pelaaja1;
        PelaajaO = pelaaja2;
        this.page = page;
        VaikeustasoX = vaikeustasoX;
        VaikeustasoO = vaikeustasoO;
        NykyinenPelaaja = "X";
        AloitusAika = DateTime.Now;
        MatsiTitle = $"{PelaajaX.KokoNimi} vs. {PelaajaO.KokoNimi}";
        NykyinenVuoroText = $"Vuoro: {PelaajaX.KokoNimi} (X)";
        ResetKentta();
    }

    public bool TietokoneVsTietokone() // Tarkistaa onko tietokone vs. tietokone
    {
        return PelaajaX.Etunimi == "Tietokone" && PelaajaO.Etunimi == "Tietokone";
    }

    public bool OnkoLoppu()
    {
        return TarkistaVoitto() || OnkoTaysi();
    }

    public void ResetKentta()
    {
        NykyinenPelaaja = "X";
        NykyinenVuoroText = $"Vuoro: {PelaajaX.KokoNimi} (X)";
        AloitusAika = DateTime.Now;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Kentta[i, j] = string.Empty;
            }
        }
        page.onPaattynyt = false;
    }

    public void TeeSiirto(int rivi, int col, Button button) // Pelaajan siirto ja värin vaihto
    {
        Kentta[rivi, col] = NykyinenPelaaja;
        button.Text = NykyinenPelaaja;
        button.TextColor = NykyinenPelaaja == "X" ? Colors.Pink : Colors.LightGreen;

        if (TarkistaVoitto() || OnkoTaysi())
        {
            page.PaivitysNappi(rivi, col, NykyinenPelaaja);
            return;
        }

        NykyinenPelaaja = NykyinenPelaaja == "X" ? "O" : "X";
        NykyinenVuoroText = $"Vuoro: {(NykyinenPelaaja == "X" ? PelaajaX.KokoNimi : PelaajaO.KokoNimi)} ({NykyinenPelaaja})";
    }

    public bool TietokoneenVuoro() // Tarkistaa onko tietokoneen vuoro
    {
        if (NykyinenPelaaja == "X" && PelaajaX.Etunimi == "Tietokone" && PelaajaX.Sukunimi == "")
            return true;
        if (NykyinenPelaaja == "O" && PelaajaO.Etunimi == "Tietokone" && PelaajaO.Sukunimi == "")
            return true;
        return false;
    }

    public void TietokoneenSiirto() // Tietokoneen siirto
    {
        var (bestRivi, bestCol) = EtsiParasSiirto();
        if (bestRivi != -1 && bestCol != -1)
        {
            Kentta[bestRivi, bestCol] = NykyinenPelaaja;
            page.PaivitysNappi(bestRivi, bestCol, NykyinenPelaaja);

            if (!TarkistaVoitto() && !OnkoTaysi())
            {
                NykyinenPelaaja = NykyinenPelaaja == "X" ? "O" : "X";
                NykyinenVuoroText = $"Vuoro: {(NykyinenPelaaja == "X" ? PelaajaX.KokoNimi : PelaajaO.KokoNimi)} ({NykyinenPelaaja})";
            }
        }
    }
    private (int rivi, int col) EtsiParasSiirto() // Etsii parhaan siirron tietokoneelle
    {
        double vaikeus = NykyinenPelaaja == "X" ? VaikeustasoX : VaikeustasoO;
        double randomness = 1.0 - vaikeus; // Vaikeustaso 0.0-1.0

        if (random.NextDouble() < randomness)
        {
            var peliLiikkeet = new List<(int rivi, int col)>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(Kentta[i, j]))
                    {
                        peliLiikkeet.Add((i, j));
                    }
                }
            }
            if (peliLiikkeet.Count > 0)
            {
                // Tietokoneen peliin lisätään satunnaisuutta
                peliLiikkeet = peliLiikkeet.OrderBy(_ => random.Next()).ToList();

                if (randomness > 0.7)
                {
                    foreach (var siirto in peliLiikkeet)
                    {
                        Kentta[siirto.rivi, siirto.col] = NykyinenPelaaja;
                        bool onVoittoSiirto = TarkistaVoitto();
                        Kentta[siirto.rivi, siirto.col] = string.Empty;
                        if (!onVoittoSiirto) return siirto;
                    }
                }
                return peliLiikkeet[random.Next(peliLiikkeet.Count)];
            }
        }

        int parasTulos = int.MinValue;
        var parasSiirto = new List<(int row, int col)>();
        string aiSymboli = NykyinenPelaaja;

        for (int i = 0; i < 3; i++) // Silmukassa käydään läpi ensin kaikki mahdolliset siirrot
        {
            for (int j = 0; j < 3; j++) // Tarkistetaan onko ruutu tyhjä
            {
                if (string.IsNullOrEmpty(Kentta[i, j]))
                {
                    Kentta[i, j] = aiSymboli;
                    int tulos = TekoAly(0, false, aiSymboli);
                    Kentta[i, j] = string.Empty;

                    if (tulos > parasTulos)
                    {
                        parasTulos = tulos;
                        parasSiirto.Clear();
                        parasSiirto.Add((i, j));
                    }
                    else if (tulos == parasTulos)
                    {
                        parasSiirto.Add((i, j)); // Lisätään parhaat siirrot listaan
                    }
                }
            }
        }
        // Valitaan satunnainen siirto parhaista siirroista
        return parasSiirto.Count > 0 ? parasSiirto[random.Next(parasSiirto.Count)] : (-1, -1);
    }

    // Tietokoneen logiikkaa
    private int TekoAly(int syvyys, bool onTosissaan, string aiSymboli)
    {
        string ihmisSymboli = aiSymboli == "X" ? "O" : "X"; // Määritetään vastustajan merkki

        if (TarkistaVoitto()) // Jos peli on voitettu
        {
            return onTosissaan ? -10 + syvyys : 10 - syvyys; // Palautetaan pistemäärä voiton perusteella
        }
        if (OnkoTaysi()) // Jos kenttä on täynnä (tasapeli)
            return 0;

        if (onTosissaan) // Tekoälyn vuoro: maksimoidaan tulos
        {
            int parasTulos = int.MinValue;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(Kentta[i, j])) // Jos ruutu on tyhjä
                    {
                        Kentta[i, j] = aiSymboli; // Testataan tekoälyn siirtoa
                        int tulos = TekoAly(syvyys + 1, false, aiSymboli);
                        Kentta[i, j] = string.Empty;
                        parasTulos = Math.Max(tulos, parasTulos); // Valitaan paras tulos
                    }
                }
            }
            return parasTulos;
        }
        else // Ihmisen vuoro: minimoidaan tulos
        {
            int parasTulos = int.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(Kentta[i, j])) // Jos ruutu on tyhjä
                    {
                        Kentta[i, j] = ihmisSymboli; // Testataan ihmisen siirtoa
                        int score = TekoAly(syvyys + 1, true, aiSymboli);
                        Kentta[i, j] = string.Empty;
                        parasTulos = Math.Min(score, parasTulos); // Valitaan huonoin tulos
                    }
                }
            }
            return parasTulos;
        }
    }

    public bool TarkistaVoitto() // Voittajan tarkistus
    {
        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(Kentta[i, 0]) && Kentta[i, 0] == Kentta[i, 1] && Kentta[i, 1] == Kentta[i, 2])
            {
                NykyinenVuoroText = "Ottelu päättynyt";
                return true;
            }
            if (!string.IsNullOrEmpty(Kentta[0, i]) && Kentta[0, i] == Kentta[1, i] && Kentta[1, i] == Kentta[2, i])
            {
                NykyinenVuoroText = "Ottelu päättynyt";
                return true;
            }
        }
        if (!string.IsNullOrEmpty(Kentta[0, 0]) && Kentta[0, 0] == Kentta[1, 1] && Kentta[1, 1] == Kentta[2, 2])
        {
            NykyinenVuoroText = "Ottelu päättynyt";
            return true;
        }
        if (!string.IsNullOrEmpty(Kentta[0, 2]) && Kentta[0, 2] == Kentta[1, 1] && Kentta[1, 1] == Kentta[2, 0])
        {
            NykyinenVuoroText = "Ottelu päättynyt";
            return true;
        }
        return false;
    }

    public bool OnkoTaysi() // Tarkistaa onko kenttä täynnä
    {
        foreach (var cell in Kentta) // Käy silmukassa läpi koko kentän
        {
            if (string.IsNullOrEmpty(cell))
                return false;
        }
        NykyinenVuoroText = "Ottelu päättynyt";
        return true;
    }

    public void PaivitaTilastot(Pelaaja voittaja, bool onVoittanut) // Päivittää pelaajien tilastot
    {
        var kesto = DateTime.Now - AloitusAika;
        var pelaajat = PelaajaManager.LataaPelaajat();

        var pelaajaX = pelaajat.FirstOrDefault(p => p.Etunimi == PelaajaX.Etunimi && p.Sukunimi == PelaajaX.Sukunimi) ?? PelaajaX;
        var pelaajaO = pelaajat.FirstOrDefault(p => p.Etunimi == PelaajaO.Etunimi && p.Sukunimi == PelaajaO.Sukunimi) ?? PelaajaO;

        if (voittaja != null) // Jos voittaja on olemassa
        {
            if (voittaja == PelaajaX)
            {
                pelaajaX.Voitot++;
                pelaajaO.Haviot++;
            }
            else if (voittaja == PelaajaO)
            {
                pelaajaO.Voitot++;
                pelaajaX.Haviot++;
            }
        }
        else // Tasapeli
        {
            pelaajaX.Tasapelit++;
            pelaajaO.Tasapelit++;
        }
        pelaajaX.Kokonaispeliaika += kesto; // Lisätään kokonaispeliaikaan kesto
        pelaajaO.Kokonaispeliaika += kesto;

        PelaajaX.Voitot = pelaajaX.Voitot;
        PelaajaX.Haviot = pelaajaX.Haviot;
        PelaajaX.Tasapelit = pelaajaX.Tasapelit;
        PelaajaX.Kokonaispeliaika = pelaajaX.Kokonaispeliaika;

        PelaajaO.Voitot = pelaajaO.Voitot;
        PelaajaO.Haviot = pelaajaO.Haviot;
        PelaajaO.Tasapelit = pelaajaO.Tasapelit;
        PelaajaO.Kokonaispeliaika = pelaajaO.Kokonaispeliaika;

        var xIndex = pelaajat.FindIndex(p => p.Etunimi == PelaajaX.Etunimi && p.Sukunimi == PelaajaX.Sukunimi);
        if (xIndex != -1) pelaajat[xIndex] = PelaajaX;
        else pelaajat.Add(PelaajaX);

        var oIndex = pelaajat.FindIndex(p => p.Etunimi == PelaajaO.Etunimi && p.Sukunimi == PelaajaO.Sukunimi);
        if (oIndex != -1 && oIndex != xIndex) pelaajat[oIndex] = PelaajaO;
        else if (oIndex == -1) pelaajat.Add(PelaajaO);

        PelaajaManager.TallennaPelaajat(pelaajat);
    }
}