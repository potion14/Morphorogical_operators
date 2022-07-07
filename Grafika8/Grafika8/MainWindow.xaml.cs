using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Grafika8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private byte[] t, t_bufor;
        private byte[] original;
        private int tmp, w, h;

        private void Load_image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "" && openFileDialog.FileName.Contains(".PNG") || openFileDialog.FileName.Contains(".png") || openFileDialog.FileName.Contains(".jpg"))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                PixelFormat pf = PixelFormats.Bgra32;
                int stride = (bitmap.PixelWidth * pf.BitsPerPixel + 7) / 8;
                byte[] bufor = new byte[stride * bitmap.PixelHeight];
                bitmap.CopyPixels(bufor, stride, 0);
                t = bufor;
                original = bufor;
                tmp = stride;
                w = bitmap.PixelWidth;
                h = bitmap.PixelHeight;
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t, tmp);
                Binaryzacja_iteracja();
            }
            else
            {
                MessageBox.Show("Nieprawidłowy format pliku!");
            }
        }

        private void Binaryzacja_iteracja()
        {
            if (t != null)
            {
                byte[] t3 = new byte[t.Length];
                for (int i = 0; i < t.Length; i += 4)
                {
                    int tmp = (t[i] + t[i + 1] + t[i + 2]) / 3;
                    t3[i] = (byte)tmp;
                    t3[i + 1] = (byte)tmp;
                    t3[i + 2] = (byte)tmp;
                    t3[i + 3] = 255;
                }

                int srednia_tla = (t3[0] + t3[w] + t3[w * h - w] + t[w * h]) / 4;
                int srednia_obiektu = 0;
                int suma_tla = 0;
                int suma_obiektu = 0;
                int licznik_tla = 0;
                int licznik_obiektu = 0;
                int T = 0;
                int T2 = 0;

                for (int i = 0; i < t.Length; i += 4)
                {
                    srednia_obiektu += t3[i];
                }
                srednia_obiektu -= srednia_tla;
                srednia_obiektu = srednia_obiektu / (t.Length / 4) - 4;
                T = (srednia_obiektu + srednia_tla) / 2;

                byte[] t2 = new byte[t.Length];
                Image_placeholder.Source = null;
                while (T != T2)
                {
                    T2 = T;
                    for (int i = 0; i < t.Length; i += 4)
                    {
                        if (t3[i] >= T)
                        {
                            t2[i] = 255;
                            t2[i + 1] = 255;
                            t2[i + 2] = 255;
                            licznik_obiektu++;
                            suma_obiektu += t3[i];
                        }
                        else
                        {
                            t2[i] = 0;
                            t2[i + 1] = 0;
                            t2[i + 2] = 0;
                            licznik_tla++;
                            suma_tla += t3[i];
                        }
                        t2[i + 3] = 255;
                    }
                    srednia_tla = suma_tla / licznik_tla;
                    srednia_obiektu = suma_obiektu / licznik_obiektu;
                    T = (srednia_tla + srednia_obiektu) / 2;
                    licznik_obiektu = 0;
                    licznik_tla = 0;
                    suma_obiektu = 0;
                    suma_tla = 0;
                }
                //MessageBox.Show("Treshold: " + Convert.ToString(T));
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                t_bufor = t2;
            }   
        }

        private void Dilation_button(object sender, RoutedEventArgs e)
        {
            byte[] array = t_bufor;
            Dilation(array);
        }

        private void Dilation(byte[] _t)
        {
            if (_t != null)
            {
                byte[] t2 = new byte[_t.Length];
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        byte[] tab = new byte[]
                        { _t[i - (w * 4) - 4], _t[i - (w * 4)], _t[i - (w * 4) + 4], _t[i - 4], _t[i], _t[i + 4], _t[i + (w * 4) - 4], _t[i - (w * 4)], _t[i + (w * 4) + 4] };
                        foreach (byte b in tab)
                        {
                            if (b == 255) t2[i] = t2[i + 1] = t2[i + 2] = 255;
                        }
                        t2[i + 3] = 255;
                    }
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                t_bufor = t2;
            }
        }

        private void Erozja_button(object sender, RoutedEventArgs e)
        {
            byte[] array = t_bufor;
            Erozja(array);
        }

        private void Erozja(byte[] _t)
        {
            if (_t != null)
            {
                byte[] t2 = new byte[_t.Length];
                for (int i = 0; i < _t.Length; i++) t2[i] = 0;
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        byte[] tab = new byte[]
                        { _t[i - (w * 4) - 4], _t[i - (w * 4)], _t[i - (w * 4) + 4], _t[i - 4], _t[i], _t[i + 4], _t[i + (w * 4) - 4], _t[i - (w * 4)], _t[i + (w * 4) + 4] };
                        int iterator = 0;
                        foreach (byte b in tab)
                        {
                            if (b == 255) iterator++;
                        }
                        t2[i + 3] = 255;
                        if(iterator == 9) t2[i] = t2[i + 1] = t2[i + 2] = 255;
                    }
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t2, tmp);
                Image_placeholder.Source = new_bitmap;
                t_bufor = t2;
            }
        }

        private void Otwarcie_button(object sender, RoutedEventArgs e)
        {
            byte[] array = t_bufor;
            Erozja(array);
            byte[] array1 = t_bufor;
            Dilation(array1);
        }

        private void Domkniecie_button(object sender, RoutedEventArgs e)
        {
            byte[] array = t_bufor;
            Dilation(array);
            byte[] array1 = t_bufor;
            Erozja(array1);
        }

        private void Hit_or_miss()
        {
            byte[] _t = t_bufor;
            if (_t != null)
            {
                byte[] t2 = new byte[_t.Length];
                byte[] t3 = new byte[_t.Length];
                byte[] t4 = new byte[_t.Length];
                byte[] t5 = new byte[_t.Length];
                for (int i = 0; i < _t.Length; i++)
                {
                    t2[i] = 0;
                    t3[i] = 0;
                    t4[i] = 0;
                    t5[i] = 0;
                }
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        byte[] tab = new byte[]
                        { _t[i - (w * 4) - 4], _t[i - (w * 4)], _t[i - (w * 4) + 4],
                          _t[i - 4], _t[i], _t[i + 4],
                          _t[i + (w * 4) - 4], _t[i - (w * 4)], _t[i + (w * 4) + 4] };
                        //0 0 0   0 1 0
                        //0 1 0   0 0 0
                        //0 1 0   0 0 0
                        if (tab[4] == 255 && tab[7] == 255 && tab[1] == 255 && tab[3] == 255 && tab[5] == 255) t2[i] = t2[i + 1] = t2[i + 2] = 255;
                        t2[i + 3] = 255;
                        t3[i + 3] = 255;
                        t4[i + 3] = 255;
                        t5[i + 3] = 255;
                    }
                }
                t3 = _t;
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        if (t3[i] == 255) t3[i] = t3[i + 1] = t3[i + 2] = 0;
                        else t3[i] = t3[i + 1] = t3[i + 2] = 255;
                    }
                }
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        byte[] tab = new byte[]
                        { t3[i - (w * 4) - 4], t3[i - (w * 4)], t3[i - (w * 4) + 4],
                          t3[i - 4], t3[i], t3[i + 4],
                          t3[i + (w * 4) - 4], t3[i - (w * 4)], t3[i + (w * 4) + 4] };
                        if (tab[0] == 255 && tab[2] == 255 && tab[6] == 255 && tab[8] == 255) t4[i] = t4[i + 1] = t4[i + 2] = 255;
                    }
                }
                for (int i = (w * 4) + 4; i < _t.Length - ((w * 4) + 4); i += 4)
                {
                    if (i % (w * 4) != (w * 4) - 4 && i % (w * 4) != 0)
                    {
                        if (t2[i] == t4[i]) t5[i] = t5[i + 1] = t5[i + 2] = 255;
                        else t5[i] = t5[i + 1] = t[i + 2] = 0;
                    }
                }
                BitmapSource new_bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, t5, tmp);
                Image_placeholder.Source = new_bitmap;
                t_bufor = t5;
            }
        }

        private void Hom(object sender, RoutedEventArgs e)
        {
            Hit_or_miss();
        }
    }
}
