using System;
using System.Windows.Input;

namespace ExcelClaimGenerator
{
    public class RelayCommand : ICommand
    {
 
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _canExecute = canExecute;
            _execute = execute;
        } 

        private Action<object> _execute;
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        private Predicate<object> _canExecute;
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


    }
}
