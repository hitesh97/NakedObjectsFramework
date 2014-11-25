using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NakedObjects;

namespace AdventureWorksModel
{
    public partial class EmployeeDepartmentHistory
    {
    
        #region Primitive Properties
        #region EmployeeID (Int32)
    [MemberOrder(100)]
        public virtual int  EmployeeID {get; set;}

        #endregion

        #region DepartmentID (Int16)
    [MemberOrder(110)]
        public virtual short  DepartmentID {get; set;}

        #endregion

        #region ShiftID (Byte)
    [MemberOrder(120)]
        public virtual byte  ShiftID {get; set;}

        #endregion

        #region StartDate (DateTime)
    [MemberOrder(130), Mask("d")]
        public virtual System.DateTime  StartDate {get; set;}

        #endregion

        #region EndDate (DateTime)
    [MemberOrder(140), Optionally, Mask("d")]
        public virtual Nullable<System.DateTime>  EndDate {get; set;}

        #endregion

        #region ModifiedDate (DateTime)
    [MemberOrder(150), Mask("d")]
        public virtual System.DateTime  ModifiedDate {get; set;}

        #endregion


        #endregion

        #region Navigation Properties
        #region Department (Department)
    		
    [MemberOrder(160)]
    	public virtual Department Department {get; set;}

        #endregion

        #region Employee (Employee)
    		
    [MemberOrder(170)]
    	public virtual Employee Employee {get; set;}

        #endregion

        #region Shift (Shift)
    		
    [MemberOrder(180)]
    	public virtual Shift Shift {get; set;}

        #endregion


        #endregion

    }
}
