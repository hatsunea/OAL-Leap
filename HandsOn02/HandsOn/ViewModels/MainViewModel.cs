using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace HandsOn.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Models.LeapModel Model = new Models.LeapModel();

        public MainViewModel()
        {
            this.Model.PropertyChanged += Model_PropertyChanged;
        }

        public string Message
        {
            get { return this.Model.Message; }
            set
            {
                this.Model.Message = value;
            }
        }

        public HandsOn.Models.LeapModel.ResultState Result
        {
            get { return this.Model.Result; }
            set
            {
                this.Model.Result = value;
            }
        }

        public void LeapStart()
        {
            this.Model.LeapStart();
        }

        public void LeapStop()
        {
            this.Model.LeapStop();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
