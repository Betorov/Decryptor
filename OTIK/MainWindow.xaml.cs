using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace OTIK
{
    public class Symbolstats
    {
        public string Symbol { get; set; }

        public int QuantityRepet { get; set; }

        public double Requency { get; set; }

        public string Oktet { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string fileNames;
        private List<Symbolstats> list;
        private int indexGoodSearch;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void folderOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            FileFolderText.Text = fileDialog.FileName; fileNames = fileDialog.FileName;
        }

        private bool SearchSymbol(string symbol)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].Symbol == symbol)
                {
                    indexGoodSearch = i;
                    return true;
                }                
            }
            return false;
        }

        /*private void DataToExcel()
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Visible = true; //www.ahmetcansever.com
            Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Microsoft.Office.Interop.Excel.Worksheet sheet1 = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

            for (int j = 0; j < dataGrid.Columns.Count; j++) //Başlıklar için
            {
                Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[1, j + 1];
                sheet1.Cells[1, j + 1].Font.Bold = true; //Başlığın Kalın olması için
                sheet1.Columns[j + 1].ColumnWidth = 15; //Sütun genişliği ayarı
                myRange.Value2 = dataGrid.Columns[j].Header;
            }
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            { //www.ahmetcansever.com
                for (int j = 0; j < dataGrid.Items.Count; j++)
                {
                    TextBlock b = dataGrid.Columns[i].GetCellContent(dataGrid.Items[j]) as TextBlock;
                    Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 2, i + 1];
                    myRange.Value2 = b.Text;
                }
            }

        }*/

        private void StartRead_Click(object sender, RoutedEventArgs e)
        {
            list = new List<Symbolstats>();
            int SumAll = 0;
            try
            {
                using(StreamReader reader = new StreamReader(fileNames))
                {
                    while (!reader.EndOfStream)
                    {
                        string symbol = reader.ReadLine();
                        // Поиск символа если есть то прибавляем наличие если нет создаем новую колонку
                        for (int i = 0; i < symbol.Length; i++)                           
                        {
                            string oneSymbol = symbol[i].ToString();
                            if (SearchSymbol(oneSymbol))
                            {
                                list[indexGoodSearch].QuantityRepet++;
                                SumAll++;
                            }
                            else
                            {
                                char oktet = oneSymbol.ToCharArray().First();
                                string hexoktet = " ";
                                
                                byte lowByte = (byte)(oktet & 0xFF);
                                byte highByte = (byte)(oktet >> 8 & 0xFF);

                                hexoktet = String.Format("{0:X2}{1:X2}", highByte, lowByte);

                                
                                /*byte[] stringAscii = Encoding.ASCII.GetBytes(oneSymbol);
                                string results = "";
                                foreach (byte element in stringAscii)
                                {
                                    results += ((char)element).ToString();
                                }
                                oneSymbol = results;*/

                                list.Add(new Symbolstats() { Symbol = oneSymbol, QuantityRepet = 1,
                                    Oktet = hexoktet });
                                SumAll++;
                            }
                        }
                    }
                }
                
                for(int i = 0; i < list.Count; i++)
                {
                    list[i].Requency = (double)list[i].QuantityRepet / (double)SumAll;
                }
            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dataGrid.ItemsSource = list;
            //DataToExcel();
        }
    }
}
