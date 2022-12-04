using Firebase.Auth;
using KMusic.ViewModels;
using MVVMEssentials.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KMusic.Commands
{
    public class RegisterCommand : AsyncCommandBase
    {
        private readonly RegisterViewModel _registerViewModel;
        private readonly FirebaseAuthProvider _firebaseAuthProvider;

        public RegisterCommand(RegisterViewModel registerViewModel, FirebaseAuthProvider firebaseAuthProvider)
        {
            _registerViewModel = registerViewModel;
            _firebaseAuthProvider = firebaseAuthProvider;
        }
        protected override async Task ExecuteAsync(object parameter)
        {
            try
            {
                await _firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(
                     _registerViewModel.Email,
                    _registerViewModel.Password,
                    _registerViewModel.Username);
                MessageBox.Show("Đăng kí thành công", "Thành công", MessageBoxButtons.OK);
            }
            catch (Exception)
            {
                MessageBox.Show("Đăng kí thất bại. Thử lại sau","Lỗi..", MessageBoxButtons.OK);
            }
        }
    }
}
