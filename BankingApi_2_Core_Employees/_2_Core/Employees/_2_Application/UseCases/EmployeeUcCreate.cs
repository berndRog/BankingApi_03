using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;


// Use case: Create a new employee (EM-1).
public sealed class EmployeeUcCreate(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcCreate> logger
) {
   public async Task<Result<EmployeeDto>> ExecuteAsync(
      EmployeeCreateDto employeeCreateDto,
      CancellationToken ct = default
   ) {
      if (string.IsNullOrWhiteSpace(employeeCreateDto.PersonnelNumber))
         return Result<EmployeeDto>.Failure(EmployeeErrors.PersonnelNumberIsRequired);
      
      // 1) Subject required
      var resultSubject = IdentitySubject.Check(employeeCreateDto.Subject);
      if (resultSubject.IsFailure) 
         return Result<EmployeeDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // 2) Value objects
      var resultEmail = EmailVo.Create(employeeCreateDto.Email);
      if (resultEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;
      
      var resultPhone = PhoneVo.Create(employeeCreateDto.Phone);
      if (resultPhone.IsFailure)
         return Result<EmployeeDto>.Failure(resultPhone.Error);
      var phoneVo = resultPhone.Value;
      
      // 3) Check uniqueness
      if (await repository.FindByEmailAsync(emailVo, ct) != null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.EmailMustBeUnique);

      if (await repository.FindByPersonnelNumberAsync(employeeCreateDto.PersonnelNumber, ct) != null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);

      // 4) Domain model
      var result = Employee.Create(
         firstname: employeeCreateDto.Firstname,
         lastname: employeeCreateDto.Lastname,
         emailVo: emailVo,
         phoneVo: phoneVo,
         subject: subject,
         personnelNumber: employeeCreateDto.PersonnelNumber,
         adminRights: (AdminRights) identityGateway.AdminRights,
         createdAt: clock.UtcNow,
         id: employeeCreateDto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<EmployeeDto>.Failure(result.Error);
      var employee = result.Value!;
    
      // 5) Activate employee
      employee.Activate(clock.UtcNow);
      
      // 6) Add to repository
      repository.Add(employee);

      // 7) Save all changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Employee created", ct);

      logger.LogInformation(
         "EmployeeUcCreate Id={id}  savedRows={rows}", employee.Id,  rows);

      return Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }
}