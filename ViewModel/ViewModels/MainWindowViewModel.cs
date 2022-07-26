﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private StartupViewModel startupViewModel = new StartupViewModel();
        private ServiceViewModel serviceViewModel = new ServiceViewModel();
        private CustomerViewModel customerViewModel = new CustomerViewModel();
        private AppointmentViewModel appointmentViewModel = new AppointmentViewModel();
        private HelpViewModel helpViewModel = new HelpViewModel();
        private BindableBase currentViewModel;

        public AppointmentViewModel AppointmentViewModel
        {
            get { return appointmentViewModel; }
            set
            {
                appointmentViewModel = value;
                OnPropertyChanged("AppointmentViewModel");
            }
        }

        
        public MyICommand<string> NavCommand { get; set; }
        public MyICommand RefreshCommand { get; set; }
        public MainWindowViewModel()
        {
            NavCommand = new MyICommand<string>(OnNav);
            RefreshCommand = new MyICommand(OnRefresh);
            CurrentViewModel = startupViewModel;
        }

        private void OnRefresh()
        {
            OnNav("service");
            OnNav("appointment");
            AppointmentViewModel = new AppointmentViewModel();
        }

        public void OnNav(string obj)
        {
            switch (obj)
            {
                case "home":
                    CurrentViewModel = startupViewModel;
                    break;
                case "customer":
                    CurrentViewModel = customerViewModel;
                    break;
                case "service":
                    CurrentViewModel = serviceViewModel;
                    break;
                case "appointment":
                    CurrentViewModel = appointmentViewModel;
                    break;
                case "help":
                    CurrentViewModel = helpViewModel;
                    break;
            }
        }
       
        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }
    }
}
