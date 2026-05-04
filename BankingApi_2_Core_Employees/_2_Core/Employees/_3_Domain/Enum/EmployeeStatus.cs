namespace BankingApi._2_Core.Employees._3_Domain.Enum;

public enum EmployeeStatus {
   Pending = 0,     // Registered, identity not yet verified by an employee
   Active = 1,      // Identity verified and approved by an employee
   Rejected = 2,    // Registration rejected after identity check
   Deactivated = 3  // 
}
