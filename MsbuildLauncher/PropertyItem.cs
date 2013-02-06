using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using MsbuildLauncher.ViewModel;
using System.ComponentModel;

namespace MsbuildLauncher
{
    [DataContract]
    public class PropertyItem : INotifyPropertyChanged
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Items { get; set; }

        public string DefaultValue { get; set; }

        // for WPF
        void valuePropertyChanged()
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }

        private string value;
        public string Value {
            get { return value; }
            set { 
                this.value = value;

                valuePropertyChanged();
                IsChanged = true;
            }
        }

        void isChangedPropertyChanged()
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsChanged"));
        }

        private bool isChanged;
        public bool IsChanged
        {
            get { return isChanged; }
            set
            {
                isChanged = value;

                // if isChanged set to false, set value by default value
                if (isChanged == false)
                {
                    this.value = this.DefaultValue;
                    valuePropertyChanged();
                }

                isChangedPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
