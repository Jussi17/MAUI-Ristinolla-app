using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Pelaaja : INotifyPropertyChanged
{
    private string etunimi;
    private string sukunimi;
    private int syntymavuosi;
    private int voitot;
    private int haviot;
    private int tasapelit;
    private TimeSpan kokonaispeliaika;

    // Tapahtuma, joka ilmoittaa UI:lle, että arvo on muuttunut
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Yleinen asetusmetodi, joka päivittää arvon vain, jos se muuttuu
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);

        // Päivitetään myös KokoNimi, jos etunimi tai sukunimi muuttuu
        if (propertyName == nameof(Etunimi) || propertyName == nameof(Sukunimi))
            OnPropertyChanged(nameof(KokoNimi));

        return true;
    }

    // Pelaajan ominaisuudet
    public string Etunimi { get => etunimi; set => SetProperty(ref etunimi, value); }
    public string Sukunimi { get => sukunimi; set => SetProperty(ref sukunimi, value); }
    public int Syntymavuosi { get => syntymavuosi; set => SetProperty(ref syntymavuosi, value); }
    public int Voitot { get => voitot; set => SetProperty(ref voitot, value); }
    public int Haviot { get => haviot; set => SetProperty(ref haviot, value); }
    public int Tasapelit { get => tasapelit; set => SetProperty(ref tasapelit, value); }
    public TimeSpan Kokonaispeliaika { get => kokonaispeliaika; set => SetProperty(ref kokonaispeliaika, value); }
    public string KokoNimi => $"{Etunimi} {Sukunimi}";

    public Pelaaja()
    {
        etunimi = string.Empty;
        sukunimi = string.Empty;
    }
}

public static class PelaajaManager
{
    // Tallennustiedoston sijainti
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "ristinollapelaajat.json");

    // Ladataan pelaajat tiedostosta
    public static List<Pelaaja> LataaPelaajat()
    {
        if (!File.Exists(FilePath))
            return new List<Pelaaja>();

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<Pelaaja>>(json) ?? new List<Pelaaja>();
    }

    // Tallennetaan pelaajat tiedostoon
    public static void TallennaPelaajat(List<Pelaaja> pelaajat)
    {
        var json = JsonSerializer.Serialize(pelaajat, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    // Etsii pelaajan tai luo uuden, jos ei löydy
    public static Pelaaja EtsiTaiLuoPelaaja(string etuNimi, string sukuNimi, int syntymaVuosi, Action<string> naytaVaroitus = null)
    {
        var pelaajat = LataaPelaajat();

        // Poistetaan mahdolliset kaksoiskappaleet pelaajista (vain etu- ja sukunimi huomioidaan tässä)
        var uniikkiPelaajat = pelaajat
            .GroupBy(p => new { p.Etunimi, p.Sukunimi })
            .Select(g => g.First())
            .ToList();

        // Jos duplikaatteja oli, tallennetaan puhdistettu lista
        if (uniikkiPelaajat.Count < pelaajat.Count)
        {
            pelaajat = uniikkiPelaajat;
            TallennaPelaajat(pelaajat);
        }

        // Tarkistetaan, löytyykö pelaaja jo listalta
        var pelaaja = pelaajat.FirstOrDefault(p =>
            p.Etunimi == etuNimi &&
            p.Sukunimi == sukuNimi);

        if (pelaaja == null)
        {
            // Luodaan uusi pelaaja, jos ei löytynyt
            pelaaja = new Pelaaja
            {
                Etunimi = etuNimi,
                Sukunimi = sukuNimi,
                Syntymavuosi = syntymaVuosi,
                Voitot = 0,
                Haviot = 0,
                Tasapelit = 0,
                Kokonaispeliaika = TimeSpan.Zero
            };
            pelaajat.Add(pelaaja);
            TallennaPelaajat(pelaajat);
        }
        else
        {
            // Ilmoitetaan käyttäjälle, jos pelaaja on jo olemassa
            naytaVaroitus?.Invoke($"Pelaaja {etuNimi} {sukuNimi} on jo olemassa!");
        }

        return pelaaja;
    }
}
