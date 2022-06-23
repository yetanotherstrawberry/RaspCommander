using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaspCommander
{

    public class Wpis
    {

        public string Sciezka { get; set; }
        public string Nazwa { get; set; }
        public bool CzyLewak { get; set; }
        public DateTime Data { get; set; }

    }

    public partial class MainWindow : Window
    {

        private const string nazwa = "RaspCommander";
        private Wpis wybrany_wpis;
        private Point kursor;
        private string lewy_katalog, prawy_katalog;
        private Vector wymiary_okna;

        public ObservableCollection<Wpis> LeweItemki { get; set; }
        public ObservableCollection<Wpis> PraweItemki { get; set; }

        private void DajBlad(ObservableCollection<Wpis> do_zmiany, Exception wyjatek, string tytul = null)
        {

            MessageBox.Show(wyjatek.Message, tytul != null ? nazwa + " - " + tytul : nazwa);
            if (do_zmiany != null) ZmienZawartosc(do_zmiany, do_zmiany == LeweItemki ? lewy_katalog : prawy_katalog);

        }

        private void ZmienZawartosc(ObservableCollection<Wpis> do_zmiany, string sciezka)
        {

            try
            {

                do_zmiany.Clear();

                if (sciezka == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                {

                    foreach (string wpis in Environment.GetLogicalDrives())
                        do_zmiany.Add(new Wpis
                        {
                            Nazwa = wpis,
                            Sciezka = wpis,
                            Data = Directory.GetCreationTime(wpis),
                            CzyLewak = do_zmiany == LeweItemki
                        });

                }
                else
                {

                    if (Directory.GetParent(sciezka) != null)
                        do_zmiany.Add(new Wpis
                        {
                            Nazwa = "..",
                            Sciezka = Directory.GetParent(sciezka).ToString(),
                            Data = Directory.GetCreationTime(Directory.GetParent(sciezka).ToString()),
                            CzyLewak = do_zmiany == LeweItemki
                        });
                    else
                        do_zmiany.Add(new Wpis
                        {
                            Nazwa = "..",
                            Sciezka = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}",
                            Data = DateTime.Now,
                            CzyLewak = do_zmiany == LeweItemki
                        });

                    foreach (string wpis in Directory.EnumerateFileSystemEntries(sciezka))
                        do_zmiany.Add(new Wpis
                        {
                            Nazwa = Path.GetFileName(wpis),
                            Sciezka = wpis,
                            Data = File.GetCreationTime(wpis),
                            CzyLewak = do_zmiany == LeweItemki
                        });

                }

                if (do_zmiany == LeweItemki) lewy_katalog = sciezka;
                else prawy_katalog = sciezka;

            }
            catch (Exception wyjatek)
            {

                if (wyjatek is Win32Exception || wyjatek is UnauthorizedAccessException || wyjatek is FileNotFoundException)
                {

                    DajBlad(do_zmiany, wyjatek, "błąd podczas zmiany katalogu");

                }
                else throw wyjatek;
                
            }

        }

        private void Inicjuj()
        {

            wymiary_okna = new Vector(Window.GetWindow(this).Width, Window.GetWindow(this).Height);

            LeweItemki = new ObservableCollection<Wpis>();
            PraweItemki = new ObservableCollection<Wpis>();

            ZmienZawartosc(LeweItemki, "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
            ZmienZawartosc(PraweItemki, Directory.GetCurrentDirectory());

            lewak.ItemsSource = LeweItemki;
            prawak.ItemsSource = PraweItemki;

        }

        private void Interakcja(string sciezka, MouseButtonEventArgs klik, bool czyLewo)
        {

            if (klik.ChangedButton == MouseButton.Left)
            {

                try
                {

                    if(sciezka == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                        ZmienZawartosc(czyLewo ? LeweItemki : PraweItemki, sciezka);
                    else if (!File.GetAttributes(sciezka).HasFlag(FileAttributes.Directory))
                        Process.Start(sciezka);
                    else
                        ZmienZawartosc(czyLewo ? LeweItemki : PraweItemki, sciezka);

                }
                catch (Exception wyjatek)
                {

                    if (wyjatek is Win32Exception || wyjatek is UnauthorizedAccessException || wyjatek is IOException)
                        DajBlad(null, wyjatek, "błąd podczas uruchamiania");
                    else
                        throw wyjatek;

                }

            }

        }

        private void Selekcja(object obiekt, RoutedEventArgs klik)
        {

            wybrany_wpis = (Wpis)((DataGridRow)obiekt).Item;

        }

        private void ItemLewoKlikniety(object obiekt, MouseButtonEventArgs klik)
            => Interakcja(((Wpis)((DataGridRow)obiekt).Item).Sciezka, klik, true);

        private void ItemPrawoKlikniety(object obiekt, MouseButtonEventArgs klik)
            => Interakcja(((Wpis)((DataGridRow)obiekt).Item).Sciezka, klik, false);

        private void Klawisz(object obiekt, KeyEventArgs guziczek)
        {

            if (guziczek.Key == Key.L)
            {

                MessageBox.Show(Properties.Resources.LICENSE, "Licencja programu");

            }
            else if (guziczek.Key == Key.L)
            {

                MessageBox.Show(Properties.Resources.README, "Instrukcja do programu");

            }
            else if (wybrany_wpis == null && guziczek.Key != Key.F1 && (guziczek.Key == Key.F8 || guziczek.Key == Key.F7))
            {

                DajBlad(null, new Exception("Nie wybrano żadnego pliku."), "błąd podczas akcji");

            }
            else
            {

                if (guziczek.Key == Key.F7)
                {

                    try
                    {

                        if ((wybrany_wpis.CzyLewak ? lewy_katalog : prawy_katalog) == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                        {

                            DajBlad(null, new Exception("Nie możesz modyfikować tego folderu."), "folder wirtualny");
                            return;

                        }

                        string nazwa = Interaction.InputBox("Nazwa?", "Nowy folder");
                        if (nazwa.Length == 0) return;

                        Directory.CreateDirectory(Path.Combine(wybrany_wpis.CzyLewak ? lewy_katalog : prawy_katalog, nazwa));
                        if (wybrany_wpis.CzyLewak)
                        {

                            ZmienZawartosc(LeweItemki, lewy_katalog);
                            if (lewy_katalog == prawy_katalog) ZmienZawartosc(PraweItemki, prawy_katalog);
                            wybrany_wpis = (from item in LeweItemki where item.Nazwa == nazwa select item).Single();
                            lewak.SelectedItem = wybrany_wpis;

                        }
                        else
                        {

                            ZmienZawartosc(PraweItemki, prawy_katalog);
                            if (prawy_katalog == lewy_katalog) ZmienZawartosc(LeweItemki, lewy_katalog);
                            wybrany_wpis = (from item in PraweItemki where item.Nazwa == nazwa select item).Single();
                            prawak.SelectedItem = wybrany_wpis;

                        }

                    }
                    catch (Exception wyjatek)
                    {

                        if (wyjatek is Win32Exception || wyjatek is UnauthorizedAccessException || wyjatek is IOException || wyjatek is InvalidOperationException)
                        {

                            DajBlad(null, wyjatek, "błąd podczas tworzenia katalogu");

                        }
                        else throw wyjatek;

                    }

                }
                else if (guziczek.Key == Key.F8)
                {

                    if ((wybrany_wpis.CzyLewak ? lewy_katalog : prawy_katalog) == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                    {

                        DajBlad(null, new Exception("Nie możesz modyfikować tego folderu."), "folder wirtualny");
                        return;

                    }

                    try
                    {

                        if (File.GetAttributes(wybrany_wpis.Sciezka).HasFlag(FileAttributes.Directory))
                            Directory.Delete(wybrany_wpis.Sciezka, true);
                        else
                            File.Delete(wybrany_wpis.Sciezka);

                        ZmienZawartosc(LeweItemki, lewy_katalog);
                        ZmienZawartosc(PraweItemki, prawy_katalog);

                    }
                    catch (Exception wyjatek)
                    {

                        if (wyjatek is Win32Exception || wyjatek is UnauthorizedAccessException || wyjatek is IOException)
                            DajBlad(null, wyjatek, "błąd podczas usuwania");
                        else
                            throw wyjatek;

                    }

                }

            }

        }

        private void DragKlikniecie(object obiekt, MouseButtonEventArgs klik)
        {

            kursor = klik.GetPosition((DataGridRow)obiekt);

        }

        private void DragPrzesuniecie(object obiekt, MouseEventArgs myszka)
        {

            Vector przesuniecie = myszka.GetPosition((DataGridRow)obiekt) - kursor;

            if (
                myszka.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(przesuniecie.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(przesuniecie.Y) > SystemParameters.MinimumVerticalDragDistance
            )
            {

                if (!wymiary_okna.Equals(new Vector(Window.GetWindow(this).Width, Window.GetWindow(this).Height)))
                {

                    wymiary_okna = new Vector(Window.GetWindow(this).Width, Window.GetWindow(this).Height);

                    return;

                }

                DataGridRow przesuwany = (DataGridRow)obiekt;
                Wpis dane = (Wpis)przesuwany.Item;
                DragDrop.DoDragDrop(przesuwany, dane, DragDropEffects.Move);

            }

        }

        public MainWindow()
        {

            Title = nazwa;
            DataContext = this;
            InitializeComponent();
            Inicjuj();

        }

        private void DataGrid_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(typeof(Wpis)) || e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            else e.Effects = DragDropEffects.None;

        }

        private void Kopiuj(string co, string gdzie)
        {

            try
            {

                if(gdzie == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}")
                {

                    DajBlad(null, new Exception("Nie możesz kopiować do tego folderu."), "kopiowanie do folderu wirtualnego");
                    return;

                }

                if (File.GetAttributes(co).HasFlag(FileAttributes.Directory))
                {

                    string folder_docelowy = Path.Combine(gdzie, Path.GetFileName(co));

                    if (co != folder_docelowy)
                    {

                        Directory.CreateDirectory(folder_docelowy);

                        foreach (string katalog in Directory.GetDirectories(co)) Kopiuj(katalog, folder_docelowy);
                        foreach (string pliczek in Directory.GetFiles(co)) Kopiuj(pliczek, folder_docelowy);

                    }
                    else
                        DajBlad(null, new Exception("Taki folder już istnieje:\n" + folder_docelowy), "błąd podczas kopiowania folderu");

                }
                else
                {

                    string plik_docelowy = Path.Combine(gdzie, Path.GetFileName(co));

                    if (co == plik_docelowy)
                        DajBlad(null, new Exception("Ten plik już istnieje:\n" + plik_docelowy), "błąd podczas kopiowania pliku");
                    else
                        File.Copy(co, plik_docelowy);

                }

            }
            catch (Exception wyjatek)
            {

                if (wyjatek is Win32Exception || wyjatek is UnauthorizedAccessException || wyjatek is IOException)
                    DajBlad(null, wyjatek, "błąd podczas kopiowania");
                else
                    throw wyjatek;

            }

        }

        private void Dropniecie(object sender, DragEventArgs dropik)
        {

            string dokad = ((DataGrid)sender).ItemsSource == LeweItemki ? lewy_katalog : prawy_katalog;

            if (dropik.Data.GetDataPresent(typeof(Wpis)))
            {

                Wpis skad = (Wpis)dropik.Data.GetData(typeof(Wpis));

                if (((ObservableCollection<Wpis>)((DataGrid)sender).ItemsSource).Contains(skad)) return;
                
                Kopiuj(skad.Sciezka, dokad);

            }
            else if (dropik.Data.GetDataPresent(DataFormats.FileDrop))
                foreach (string element in (string[])dropik.Data.GetData(DataFormats.FileDrop)) Kopiuj(element, dokad);
            else
                DajBlad(null, new NotSupportedException(), "nieobsługiwany format");

            ZmienZawartosc((ObservableCollection<Wpis>)((DataGrid)sender).ItemsSource, dokad);

        }

    }

}
