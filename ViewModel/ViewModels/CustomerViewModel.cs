﻿using Common.Methods;
using Common.Methods.CRUD;
using Model.FrontendModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel.ViewModels.CustomerViewModels;

namespace ViewModel.ViewModels
{
    public class CustomerViewModel : CustomerBindableBase
    {
        private CustomerAddViewModel customerAddViewModel = new CustomerAddViewModel();
        private CustomerFilterViewModel customerFilterViewModel = new CustomerFilterViewModel();
        private CustomerInfoViewModel customerInfoViewModel = new CustomerInfoViewModel();
        private CustomerBindableBase currentCustomerViewModel;
        private CustomerCRUD commonCustomer = new CustomerCRUD();

        private CustomerFront selectedItem;
        private bool canAlter = false;
        private bool canDelete = false;
        private bool canAdd = true;

        public MyICommand<string> NavCommand { get; set; }
        public MyICommand ItemSelectedCommand { get; set; }
        public MyICommand AlterCommand { get; set; }
        public MyICommand DeleteCommand { get; set; }
        public MyICommand CancelCommand { get; set; }

        public static BindingList<CustomerFront> proxy = new BindingList<CustomerFront>();

        private Validation validation = new Validation();

        public CustomerViewModel()
        {
            proxy = commonCustomer.LoadFromDataBase();

            foreach (CustomerFront customer in proxy)
                Customers.Add(customer);

            foreach(CustomerFront customer in Customers)
                CustomersSearch.Add(customer);

            NavCommand = new MyICommand<string>(OnNav);
            ItemSelectedCommand = new MyICommand(OnSelect);
            AlterCommand = new MyICommand(OnAlter);
            DeleteCommand = new MyICommand(OnDelete);
            CancelCommand = new MyICommand(OnCancel);

            OnNav("filter");
        }

        private void OnCancel()
        {
            CanAdd = true;
            if (CurrentCustomerViewModel == customerAddViewModel)
            {
                customerAddViewModel.ClearInput();
                
                CanAlter = false;
                CanDelete = false;

                SelectedItem = null;

                OnNav("filter");
                ValidationReset();
            }
            else if(CurrentCustomerViewModel == customerFilterViewModel)
            {
                customerFilterViewModel.ClearInput();
            }
            else if(CurrentCustomerViewModel == customerInfoViewModel)
            {
                customerInfoViewModel.ClearInput();
                
                SelectedItem = null;
                CanAlter = false;
                CanDelete = false;

                OnNav("filter");
            }
        }

        private void ValidationReset()
        {
            customerAddViewModel.IsFirstNameErrorVisible = "Collapsed";
            customerAddViewModel.IsLastNameErrorVisible = "Collapsed";
            customerAddViewModel.IsPhoneNumberErrorVisible = "Collapsed";
            customerAddViewModel.IsBirthdayErrorVisible = "Collapsed";
            customerAddViewModel.IsEmailErrorVisible = "Collapsed";
            customerAddViewModel.IsLoyaltyCardIdErrorVisible = "Collapsed";
        }

        private void OnDelete()
        {
            if (SelectedItem == null)
                return;

            CustomerFront customerToRemove = SelectedItem;
            commonCustomer.DeleteFromDataBase(customerToRemove);
            CustomersSearch.Remove(customerToRemove);
            Customers.Remove(customerToRemove);
            CanAlter = false;
            CanDelete = false;
            OnNav("filter");
        }

        private void OnAlter()
        {
            if(CurrentCustomerViewModel != customerAddViewModel)
            {
                if(SelectedItem == null)
                {
                    return;
                }
                customerAddViewModel.HeadText = "Alter";

                CanAdd = false;
                CanDelete = false;

                CurrentCustomerViewModel = customerAddViewModel;
                customerInfoViewModel.ClearInput();

                customerAddViewModel.FirstNameVM = SelectedItem.FirstName;
                customerAddViewModel.LastNameVM = SelectedItem.LastName;
                customerAddViewModel.PhoneNumberVM = SelectedItem.PhoneNumber;
                customerAddViewModel.EmailVM = SelectedItem.Email;
                customerAddViewModel.LoyaltyCardIdVM = SelectedItem.LoyaltyCardId;
                customerAddViewModel.DateOfBirthVM = SelectedItem.DateOfBirth;

                switch (SelectedItem.Gender)
                {
                    case "Male":
                        customerAddViewModel.IsMaleCheckedVM = true;
                        break;
                    case "Female":
                        customerAddViewModel.IsFemaleCheckedVM = true;
                        break;
                    case "Other":
                        customerAddViewModel.IsOtherCheckedVM = true;
                        break;
                    default:
                        break;
                }
            }
            else if(CheckValidation())
            {
                CustomerFront selectedOne = SelectedItem;
                CustomerFront newOne = customerAddViewModel.GetCustomer(SelectedItem.CustomerId, SelectedItem.Points);

                newOne.Exists = selectedOne.Exists;
                
                int index = CustomersSearch.IndexOf(SelectedItem);
                int indexReal = Customers.IndexOf(SelectedItem);
                CustomersSearch.RemoveAt(index);
                CustomersSearch.Insert(index,newOne);
                Customers.RemoveAt(indexReal);
                Customers.Insert(indexReal, newOne);

                commonCustomer.UpdateInDataBase(newOne);
                ValidationReset();
                CanAdd = true;
                CanAlter = false;
                CanDelete = false;

                OnNav("filter");
            }
        }

        private void OnSelect()
        {
            if (SelectedItem == null)
                return;
            CanAdd = true;
            CanAlter = true;
            CanDelete = true;
            OnNav("info");

            customerInfoViewModel.FirstNameVM = SelectedItem.FirstName;
            customerInfoViewModel.LastNameVM = SelectedItem.LastName;
            customerInfoViewModel.PhoneNumberVM = SelectedItem.PhoneNumber;
            customerInfoViewModel.DateOfBirthVM = SelectedItem.DateOfBirth;
            customerInfoViewModel.EmailVM = SelectedItem.Email;
            customerInfoViewModel.LoyaltyCardIdVM = SelectedItem.LoyaltyCardId;

            switch (SelectedItem.Gender)
            {
                case "Male":
                    customerInfoViewModel.IsMaleCheckedVM = true;
                    break;
                case "Female":
                    customerInfoViewModel.IsFemaleCheckedVM = true;
                    break;
                case "Other":
                    customerInfoViewModel.IsOtherCheckedVM = true;
                    break;
                default:
                    break;
            }
        }

        private void OnNav(string obj)
        {
            switch (obj)
            {
                case "add":
                    if (CurrentCustomerViewModel != customerAddViewModel)
                    {
                        customerAddViewModel.HeadText = "Add";
                        //FilterVisibility = "Collapsed";
                        CurrentCustomerViewModel = customerAddViewModel;
                        CanAlter = false;
                        CanDelete = false;
                        if (SelectedItem == null)
                            break;
                        SelectedItem = null;
                    }
                    else if(CheckValidation())
                    {
                        commonCustomer.AddToDataBase(customerAddViewModel.GetCustomer());

                        CustomersSearch.Add(commonCustomer.FindLastAdded());
                        Customers.Add(commonCustomer.FindLastAdded());

                        OnNav("filter");
                        ValidationReset();
                        CanAlter = false;
                        CanDelete = false;
                    }
                    break;
                case "filter":
                    CurrentCustomerViewModel = customerFilterViewModel;
                    break;
                case "info":
                    CurrentCustomerViewModel = customerInfoViewModel;
                    break;
                case "alter":
                    CurrentCustomerViewModel = customerAddViewModel;
                    break;
            }
        }

        private bool CheckValidation()
        {
            if (customerAddViewModel.FirstNameVM == null ||
                customerAddViewModel.LastNameVM == null ||
                customerAddViewModel.PhoneNumberVM == null||
                customerAddViewModel.DateOfBirthVM == null)
            {
                if (customerAddViewModel.FirstNameVM == null)
                    customerAddViewModel.IsFirstNameErrorVisible = "Visible";
                if (customerAddViewModel.LastNameVM == null)
                    customerAddViewModel.IsLastNameErrorVisible = "Visible";
                if (customerAddViewModel.PhoneNumberVM == null)
                    customerAddViewModel.IsPhoneNumberErrorVisible = "Visible";
                if (customerAddViewModel.DateOfBirthVM == null)
                    customerAddViewModel.IsBirthdayErrorVisible = "Visible";
                return false;
            }

            else if (!customerAddViewModel.IsFirstNameErrorVisible.Equals("Collapsed") ||
                !customerAddViewModel.IsLastNameErrorVisible.Equals("Collapsed") ||
                !customerAddViewModel.IsPhoneNumberErrorVisible.Equals("Collapsed") ||
                !customerAddViewModel.IsBirthdayErrorVisible.Equals("Collapsed") ||
                !customerAddViewModel.IsEmailErrorVisible.Equals("Collapsed") ||
                !customerAddViewModel.IsLoyaltyCardIdErrorVisible.Equals("Collapsed"))
                return false;
            else if (customerAddViewModel.FirstNameVM == null &&
                customerAddViewModel.LastNameVM == null &&
                customerAddViewModel.PhoneNumberVM == null)
                return false;
            return true;
        }

        public CustomerBindableBase CurrentCustomerViewModel
        {
            get { return currentCustomerViewModel; }
            set
            {
                SetProperty(ref currentCustomerViewModel, value);
            }
        }

        public bool CanAlter
        {
            get { return canAlter; }
            set
            {
                if (canAlter != value)
                {
                    canAlter = value;
                    OnPropertyChanged("CanAlter");
                }
            }
        }
        public bool CanDelete
        {
            get { return canDelete; }
            set
            {
                if (canDelete != value)
                {
                    canDelete = value;
                    OnPropertyChanged("CanDelete");
                }
            }
        }

        public bool CanAdd
        {
            get { return canAdd; }
            set
            {
                if (canAdd != value)
                {
                    canAdd = value;
                    OnPropertyChanged("CanAdd");
                }
            }
        }



        public CustomerFront SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }
    }
}
