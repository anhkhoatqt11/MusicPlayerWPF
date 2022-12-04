using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Firebase.Auth;
using KMusic.Commands;
using MVVMEssentials.ViewModels;

namespace KMusic.ViewModels
{
    public class RegisterViewModel:ViewModelBase
    {
        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            } 
                
        }


        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }

        }


        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }

        }


        public ICommand SubmitCommand { get; }

        public RegisterViewModel(FirebaseAuthProvider firebaseAuthProvider)
        {
            SubmitCommand = new RegisterCommand(this, firebaseAuthProvider);
        }

    }
}
