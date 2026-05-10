using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

public sealed class EmployeeUcUpdate(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcCreate> logger
) {
   public async Task<Result<EmployeeDto>> ExecuteAsync(
      Guid employeeId,
      EmployeeUpdateDto employeeUpdateDto,
      CancellationToken ct = default
   ) {
      
      if(employeeId == Guid.Empty)
         return Result<EmployeeDto>.Failure(EmployeeErrors.InvalidId);

      // 1) Is an employee logged in with required AdminRights?
      var loggedInSubject = identityGateway.Subject;
      var loggedInEmployee = await repository.FindByIdentitySubjectAsync(loggedInSubject, ct);
      if(loggedInEmployee == null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.NotFound);
      
      var loggedInAdminRights = (AdminRights) identityGateway.AdminRights;
      var required = AdminRights.ManageEmployees;
      bool hasRights = (loggedInAdminRights & required) == required;
      if(!hasRights)
         return Result<EmployeeDto>.Failure(EmployeeErrors.AdminRightsNotSufficient);
      
      // 2) Find existing employee
      var employee = await repository.FindByIdAsync(employeeId, ct);
      if (employee is null) {
         logger.LogWarning("Update failed: employee not found ({Id})", employeeId.To8());
         return Result<EmployeeDto>.Failure(EmployeeErrors.NotFound);
      }
      if(!employee.IsActive)
         return Result<EmployeeDto>.Failure(EmployeeErrors.AlreadyDeactivated);
      
      // 3) DomainModel
      // check Email
      EmailVo? newEmailVo;
      if (employeeUpdateDto.Email is null) {
         newEmailVo = null;
      }
      else {
         var resultEmail = EmailVo.Create(employeeUpdateDto.Email);
         if (resultEmail.IsFailure) return Result<EmployeeDto>.Failure(resultEmail.Error);
         newEmailVo = resultEmail.Value;
         // check uniqueness
         if (await repository.FindByEmailAsync(newEmailVo, ct) != null)
            return Result<EmployeeDto>.Failure(EmployeeErrors.EmailMustBeUnique);
      }
      
      // check Phone
      PhoneVo? newPhoneVo;
      if (employeeUpdateDto.Phone is null) {
         newPhoneVo = null;
      }
      else {
         var resultPhoneVo = PhoneVo.Create(employeeUpdateDto.Phone);
         if (resultPhoneVo.IsFailure) 
            return Result<EmployeeDto>.Failure(resultPhoneVo.Error);
         newPhoneVo = resultPhoneVo.Value;
      }
      
      // update existing employee 
      var resultUpdate = employee.Update(
         lastname: employeeUpdateDto.Lastname,
         emailVo: newEmailVo, 
         phoneVo: newPhoneVo,
         updatedAt: clock.UtcNow
      );
      if (resultUpdate.IsFailure) 
         return Result<EmployeeDto>.Failure(resultUpdate.Error);
      var updatedEmployee = resultUpdate.Value;
 
      // 3) Save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Employee updated", ct);

      logger.LogInformation(
         "EmployeeUcCreate Id={id}  rows={rows}", employee.Id,  rows);

      return Result<EmployeeDto>.Success(updatedEmployee.ToEmployeeDto());
   }
}