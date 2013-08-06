/*
 * Copyright (c) 2013 Yoichi Imai, All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Runtime.Serialization;
using System.ComponentModel;

namespace MsbuildLauncher.Common
{
    [DataContract]
    public class PropertyItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Items { get; set; }

        public string DefaultValue { get; set; }


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
        
        void isEnabledPropertyChanged()
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; isEnabledPropertyChanged(); }
        }
    }
}
