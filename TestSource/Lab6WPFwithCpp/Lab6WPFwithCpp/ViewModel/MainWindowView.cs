using Lab6WPFwithCpp.Commands;
using Lab6WPFwithCpp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lab6WPFwithCpp.ViewModel
{
    class MainWindowView
    {
        private string _textInputFile;
        private string _textOutputFile;
        private RLELib.CUnpackMachine unpackMachine;

        public MainWindowView()
        {

        }

        public string TextInputFile { get => _textInputFile; set => _textInputFile = value; }
        public string TextOutputFile { get => _textOutputFile; set => _textOutputFile = value; }

        private RelayCommand _pack;
        public RelayCommand Pack
        {
            get
            {
                return _pack ?? (_pack = new RelayCommand(async obj =>
                {
                    
                    Task packTask = new Task(new Action(async () => 
                    {
                        try
                        {
                            MessageBox.Show("Start");
                            RLELib.CPackMachine packMachine = new RLELib.CPackMachine(TextOutputFile);
                            packMachine.SendFile(TextInputFile);
                            packMachine.close();
                            MessageBox.Show("OK");
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }));
                    packTask.Start();
                }));
            }
        }

        private RelayCommand _unPack;
        public RelayCommand UnPack
        {
            get
            {
                return _unPack ?? (_unPack = new RelayCommand(async obj =>
                {

                    Task packTask = new Task(new Action(async () => 
                    {
                        try
                        {
                            MessageBox.Show("Start");
                            RLELib.CUnpackMachine unpackMachine = new RLELib.CUnpackMachine(TextOutputFile);
                            unpackMachine.SendFile(TextInputFile);
                            unpackMachine.close();
                            MessageBox.Show("OK");
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }));
                    packTask.Start();
                }));
            }
        }
    }
}
