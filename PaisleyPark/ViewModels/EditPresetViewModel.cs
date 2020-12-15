using PaisleyPark.Common;
using PaisleyPark.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace PaisleyPark.ViewModels
{
    public class EditPresetViewModel : BindableBase
    {
        public bool GameReady { get; set; }
        public bool? DialogResult { get; private set; }
        public Preset EditPreset { get; set; }
        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand GetMapIDCommand { get; private set; }
        public ICommand GetWayMarksCommand { get; private set; }
        private Memory GameMemory;

        public EditPresetViewModel(IEventAggregator ea) {
            ea.GetEvent<GameMemoryUpdateEvent>().Subscribe(OnGameMemoryUpdate);
            OKCommand = new DelegateCommand<Window>(OnEdit);
            CancelCommand = new DelegateCommand<Window>(OnCancel);
            GetMapIDCommand = new DelegateCommand<Window>(OnGetMapID);
            GetWayMarksCommand = new DelegateCommand<Window>(OnGetWayMarks);
        }
        private void OnGameMemoryUpdate(object gameMemory) {
            // Assign the GameMemory to the memory from the update method.
            GameMemory = gameMemory as Memory;
        }
        /// <summary>
		/// When clicking the edit button.
		/// </summary>
		/// <param name="window">Window this was called from.</param>
		private void OnEdit(Window window) {
            // Set DidCreate to true.
            DialogResult = true;
            // Close the window.
            window.Close();
        }

        /// <summary>
		/// When clicking cancel button.
		/// </summary>
		/// <param name="window">Window this was called from.</param>
		private void OnCancel(Window window) {
            // Set DidCreate to false.
            DialogResult = false;
            // Close the window.
            window.Close();
        }

        private void OnGetMapID(Window window) {
            if (GameReady)
                EditPreset.MapID = GameMemory.MapID;
            else
                MessageBox.Show("You can't create or modify a preset right now.", "Paisley Park", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void OnGetWayMarks(Window window) {
            if (GameReady) {
                EditPreset.MapID = GameMemory.MapID;
                EditPreset.A = GameMemory.A;
                EditPreset.B = GameMemory.B;
                EditPreset.C = GameMemory.C;
                EditPreset.D = GameMemory.D;
                EditPreset.One = GameMemory.One;
                EditPreset.Two = GameMemory.Two;
                EditPreset.Three = GameMemory.Three;
                EditPreset.Four = GameMemory.Four;
            }
            else
                MessageBox.Show("You can't create or modify a preset right now.", "Paisley Park", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
