﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Model.FrontendModel;
using Model.DBModel;
using DatabaseLogic.Services;

namespace Common.Methods.CRUD
{
    public class AppointmentCRUD
    {
        private Transform transform=  new Transform();      
        private AppointmentService appointmentService = new AppointmentService();
        private SIAService sIAService = new SIAService();
        private CustomerService customerService = new CustomerService();

        public BindingList<AppointmentFront> LoadFromDataBase() {
            BindingList<AppointmentFront> newlist = new BindingList<AppointmentFront>();
            List<DBAppointment> dBAppointments = (List<DBAppointment>)appointmentService.FindAll();
            foreach (DBAppointment app in dBAppointments) { 
                List<Tuple<int,int>> dBSIAs = new List<Tuple<int, int>>();
                dBSIAs.Clear();
                dBSIAs = (List<Tuple<int, int>>)sIAService.GetAllServicesForId(app.appointmentId);
                newlist.Add(transform.DBToFE.Appointment(app, dBSIAs));
            }

            return newlist;
        }

        public void AddToDataBase(AppointmentFront appointment) {
            DBAppointment dBAppointment = transform.FEToDB.Appointment(appointment);
            dBAppointment.appointmentId = 0;
            BindingList<AppointmentItemFront> SIALIST = appointment.SIA;
            appointmentService.Save(dBAppointment);
            appointment = FindLastAdded();
            foreach (AppointmentItemFront x in SIALIST) {         
                sIAService.Save(transform.FEToDB.AppointmentItem(new Tuple<int, AppointmentItemFront>(appointment.AppointmentId, x)));
            }
        }

        public AppointmentFront FindLastAdded() {
            List<DBAppointment> bAppointments = new List<DBAppointment>();
            bAppointments = (List<DBAppointment>)appointmentService.FindAll();
            List<Tuple<int, int>> dBSIAs = new List<Tuple<int, int>>();
            dBSIAs = (List<Tuple<int, int>>)sIAService.GetAllServicesForId(bAppointments.Last().appointmentId);

            return transform.DBToFE.Appointment(bAppointments.Last(),dBSIAs);
        }

        public void DeleteFromDataBase(AppointmentFront appointment) {
            appointmentService.Delete(transform.FEToDB.Appointment(appointment));
        }

        public void UpdateInDataBase(AppointmentFront appointment) {
            DBAppointment dBAppointment = transform.FEToDB.Appointment(appointment);

            sIAService.DeleteAllByAppointmentId(appointment.AppointmentId);

            appointmentService.Save(dBAppointment);
            foreach (AppointmentItemFront x in appointment.SIA)
            {
                sIAService.Save(transform.FEToDB.AppointmentItem(new Tuple<int, AppointmentItemFront>(appointment.AppointmentId, x)));
            }
        }

        public void RegulatePoints(AppointmentFront appointment) {
            CustomerFront updateCustomer = appointment.Customer;
            foreach (var x in appointment.SIA) {
                if (x.PaymentMethod)
                {
                    updateCustomer.Points -= x.Service.PointsPrice;
                }
                else {
                    updateCustomer.Points += x.Service.PointsValue;                   
                }
            }
            customerService.Save(transform.FEToDB.Customer(updateCustomer));
            
        }

        public AppointmentFront UpdateCustomerInList(AppointmentFront paidAppointment,AppointmentFront appointmentFromList)
        {
            AppointmentFront newOne = appointmentFromList;
                foreach (var x in paidAppointment.SIA)
                {
                    if (x.PaymentMethod)
                    {
                        newOne.Customer.Points -= x.Service.PointsPrice;
                    }
                    else
                    {
                        newOne.Customer.Points += x.Service.PointsValue;
                    }
                }
            return newOne;

        }
    }
}

