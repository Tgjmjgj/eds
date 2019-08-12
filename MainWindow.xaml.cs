using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;

using BINT = System.Numerics.BigInteger;

namespace GOST_EDS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int z = 1000;
        private int key_length = 224;

        private EllipticCurve_EDS.EllipticCurve_Point Q = new EllipticCurve_EDS.EllipticCurve_Point();
        private BINT d = new BINT();
        private STRIBOG hash = new STRIBOG(256);

        private byte[] save_Hash;

        public int Z
        {
            get
            { return z; }
            set
            { z = value; }
        }

        public int Key_length
        {
            get
            { return key_length; }
            set
            { key_length = value; }
        }

        private static Dictionary<int, Dictionary<char, BINT>> Base = new Dictionary<int, Dictionary<char, BINT>>()
        { { 0, new Dictionary<char,BINT>() {
            { 'p', BINT.Parse("6277101735386680763835789423207666416083908700390324961279") },
            { 'a', BINT.Parse("-3") },
            { 'b', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("64210519e59c80e70fa7e9ab72243049feb8deecc146b9b1")) },
            { 'x', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("188da80eb03090f67cbf20eb43a18800f4ff0afd82ff1012")) },
            { 'y', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("07192b95ffc8da78631011ed6b24cdd573f977a11e794811")) },
            { 'r', BINT.Parse("6277101735386680763835789423176059013767194773182842284081") },
            { 'f', BINT.One },
            { 'n', new BINT(192) } } },
          { 1, new Dictionary<char, BINT>() {
            { 'p', BINT.Parse("26959946667150639794667015087019630673557916260026308143510066298881") },
            { 'a', BINT.Parse("-3") },
            { 'b', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("b4050a850c04b3abf54132565044b0b7d7bfd8ba270b39432355ffb4")) },
            { 'x', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("b70e0cbd6bb4bf7f321390b94a03c1d356c21122343280d6115c1d21")) },
            { 'y', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("bd376388b5f723fb4c22dfe6cd4375a05a07476444d5819985007e34")) },
            { 'r', BINT.Parse("26959946667150639794667015087019625940457807714424391721682722368061") },
            { 'f', BINT.One },
            { 'n', new BINT(224) } } },
          { 2, new Dictionary<char, BINT>() {
            { 'p', BINT.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951") },
            { 'a', BINT.Parse("-3") },
            { 'b', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")) },
            { 'x', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296")) },
            { 'y', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5")) },
            { 'r', BINT.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369") },
            { 'f', BINT.One },
            { 'n', new BINT(256) } } },
          { 3, new Dictionary<char, BINT>() {
            { 'p', BINT.Parse("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319") },
            { 'a', BINT.Parse("-3") },
            { 'b', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("b3312fa7e23ee7e4988e056be3f82d19181d9c6efe8141120314088f5013875ac656398d8a2ed19d2a85c8edd3ec2aef")) },
            { 'x', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741e082542a385502f25dbf55296c3a545e3872760ab7")) },
            { 'y', BINT.Parse(EllipticCurve_EDS.DecStringFromHexString("3617de4a96262c6f5d9e98bf9292dc29f8f41dbd289a147ce9da3113b5f0b8c00a60b1ce1d7e819d7a431d7c90ea0e5f")) },
            { 'r', BINT.Parse("39402006196394479212279040100143613805079739270465446667946905279627659399113263569398956308152294913554433653942643") },
            { 'f', BINT.One },
            { 'n', new BINT(384) } } }
        }; 

        public MainWindow()
        {
            InitializeComponent();
            textBox.Text = "Введите текст";
            textBox1.Text = "ХЕШ";
            textBox2.Text = "ПОДПИСЬ";
            this.textBlock_info.Text = "ГОСТ Р 34.10-2012 - российский стандарт, описывающий алгоритм" +
                " применения ЭЦП на основе эллиптических кривых. Стойкость этого алгоритма" +
                "основывается на сложности вычисления дискретного логарифма в группе точек эллиптической" +
                " кривой, а также на стойкости хэш-функции Стрибог по стандарту ГОСТ Р 34.11-2012.";               ;
        }

        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void button2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox1.Text = textBox1.Text.ToUpper();
            if (textBox1.Text != "" && textBox1.Text != "ХЕШ")
            {
                string H = "";
                try
                {
                    this.textBox2_TextChanged(sender, e);
                    H = EllipticCurve_EDS.HexStringFromByteArray(this.hash.GetHash(Encoding.Default.GetBytes(textBox.Text)));
                }
                catch (KeyNotFoundException)
                { textBox1.Text = "Недопустимые символы"; }
                catch
                { textBox1.Text = "Непредвиденная ошибка"; }
                if (H == this.textBox1.Text)
                    textBox1.Background = Brushes.LightGreen;
                else
                    textBox1.Background = Brushes.Red;
            }
            else
                textBox1.Background = Brushes.White;
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox2.Text = textBox2.Text.ToUpper();
            if (textBox2.Text != "" && textBox2.Text != "ПОДПИСЬ" && textBox2.Text != "Необходимо вычислить хеш")
            {
                if (this.textBox2.Text.Length != this.Key_length / 2 )
                    textBox2.Background = Brushes.Red;
                else
                {
                    bool result = false;
                    try
                    {
                        result = EllipticCurve_EDS.VerifiedEDS(this.hash.GetHash(Encoding.Default.GetBytes(textBox.Text)), textBox2.Text, Q);
                    }
                    catch (KeyNotFoundException)
                    { textBox2.Text = "Недопустимые символы"; }
                    catch
                    { textBox2.Text = "Непредвиденная ошибка"; }
                    if (result)
                    {
                        textBox2.Background = Brushes.LightGreen;
                    }
                    else
                        textBox2.Background = Brushes.Red;
                }
            }
            else
                textBox2.Background = Brushes.White;
        }

        private byte[] InvertByteArray(byte[] inv)
        {
            byte[] ret = new byte[inv.Length];
            for (int i = 0; i < inv.Length; i++)
                ret[i] = inv[inv.Length - 1 - i];
            return ret;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(this.grid_1, Z++);
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(this.grid_2, Z++);
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                STRIBOG Alg = new STRIBOG(256);
                byte[] hash = Alg.GetHash(Encoding.Default.GetBytes(this.textBox.Text));
                this.save_Hash = hash;
                this.textBox1.Text = EllipticCurve_EDS.HexStringFromByteArray(hash);
            }
            catch
            { textBox1.Text = "Непредвиденная ошибка"; }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.save_Hash == null)
                    throw new DataMisalignedException();
                this.Key_length = (int)Base[this.comboBox_1.SelectedIndex]['n'];
                EllipticCurve_EDS.P = Base[this.comboBox_1.SelectedIndex]['p'];
                EllipticCurve_EDS.A = Base[this.comboBox_1.SelectedIndex]['a'];
                EllipticCurve_EDS.B = Base[this.comboBox_1.SelectedIndex]['b'];
                EllipticCurve_EDS.R = Base[this.comboBox_1.SelectedIndex]['r'];
                EllipticCurve_EDS.G = new EllipticCurve_EDS.EllipticCurve_Point(
                        Base[this.comboBox_1.SelectedIndex]['x'], Base[this.comboBox_1.SelectedIndex]['y']);
                EllipticCurve_EDS.CreateKeys(this.Key_length, out this.d, out this.Q);
                string eds = EllipticCurve_EDS.GenerateEDS(this.save_Hash, this.d);
                this.textBox2.Text = eds;
            }
            catch (DataMisalignedException)
            { textBox2.Text = "Необходимо вычислить хеш"; }
            catch (KeyNotFoundException)
            { textBox2.Text = "Недопустимые символы"; }
            catch (IndexOutOfRangeException)
            { textBox2.Text = "Выход за границы коллекции объектов"; }
            catch
            { textBox2.Text = "Непредвиденная ошибка"; }
        }

        private void comboBox_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBox_1 != null)
            {
                this.ui_p.Text = Base[this.comboBox_1.SelectedIndex]['p'].ToString();
                this.ui_a.Text = Base[this.comboBox_1.SelectedIndex]['a'].ToString();
                this.ui_b.Text = Base[this.comboBox_1.SelectedIndex]['b'].ToString("X");
                this.ui_gx.Text = Base[this.comboBox_1.SelectedIndex]['x'].ToString("X");
                this.ui_gy.Text = Base[this.comboBox_1.SelectedIndex]['y'].ToString("X");
                this.ui_r.Text = Base[this.comboBox_1.SelectedIndex]['r'].ToString();
                this.ui_f.Text = Base[this.comboBox_1.SelectedIndex]['f'].ToString();
            }
        }

        private void listBox_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBox_1 != null && this.textBlock_info != null)
                switch (this.listBox_1.SelectedIndex)
                {
                    case -1: this.textBlock_info.Text = ""; break;
                    case 0:
                        this.textBlock_info.Text = "Порядок эллиптической кривой(p) – это число," + 
                            " которое показывает количество точек кривой над конечным полем." +
                            " Является простым и относится к обобщенным числам Мерсенна, т.е." +
                            " представляет собой сумму различных степеней двойки.";
                        this.image_info.Source = null;
                        break;
                    case 1:
                        this.textBlock_info.Text = "а - первый из коэффициентов уравнения" +
                            " задающего вид эллиптической кривой. Рекомендуется выбор значения a = -3," +
                            " что ускоряет операцию сложения в координатах Якоби.";
                        this.image_info.Source = new BitmapImage(new Uri("Image/ec.png", UriKind.Relative));
                        break;
                    case 2:
                        this.textBlock_info.Text = "b - второй из коэффициентов уравнения" +
                            " задающего вид эллиптической кривой.";
                        this.image_info.Source = new BitmapImage(new Uri("Image/ec.png", UriKind.Relative));
                        break;
                    case 3:
                        this.textBlock_info.Text = "Генерирующая (базисная) точка эллиптической кривой(G)" +
                            " – это такая точка на эллиптической кривой, для которой минимальное значение r," +
                            " такое, что r* G = O, является очень большим простым числом.\n" +
                            " Gx - х-координата точки G.";
                        this.image_info.Source = null;
                        break;
                    case 4:
                        this.textBlock_info.Text = " Абелева подгруппа точек является циклической и задается" +
                            " этой порождающей точкой G.\nGy - y-координата точки G.";
                        this.image_info.Source = null;
                        break;
                    case 5:
                        this.textBlock_info.Text = "r - порядок точки G - наименьшее значение числа q," +
                            " для которого выполняется равенство q* G = О. Порядок  группы  точек" +
                            " эллиптической  кривой равен порядку группы точек ЭК";
                        this.image_info.Source = null;
                        break;
                    case 6:
                        this.textBlock_info.Text = "f - параметр, называемый кофактором. Определяется" +
                            " отношением общего числа точек на эллиптической кривой к порядку точки G." +
                            " Данное число должно быть как можно меньше.\n     f = p / r";
                        this.image_info.Source = null;
                        break;
                }
        }
    }
}
