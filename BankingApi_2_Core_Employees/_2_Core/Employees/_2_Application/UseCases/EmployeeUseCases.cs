using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

// Facade for employee-related application use cases.
// - Provides a single entry point for all employee write operations
// - Delegates execution to specialized use case implementations
// - Simplifies dependency injection for API controllers
public sealed class EmployeeUseCases(
   EmployeeUcCreate createUc,
   EmployeeUcSetAdminRights setRightsUc,
   EmployeeUcUpdate updateUc,
   EmployeeUcDeactivate deactivateUc
) : IEmployeeUseCases {
   
   public Task<Result<EmployeeDto>> CreateAsync(
      EmployeeCreateDto dto,
      CancellationToken ct = default
   ) => createUc.ExecuteAsync(employeeCreateDto: dto, ct: ct);

   public Task<Result> SetAdminRightsAsync(
      Guid employeeId, 
      AdminRights adminRights, 
      CancellationToken ct = default
   ) => setRightsUc.ExecuteAsync(employeeId, adminRights, ct);

   public Task<Result<EmployeeDto>> UpdateAsync(
      Guid employeeId, 
      EmployeeUpdateDto employeeUpdateDto, 
      CancellationToken ct = default
   ) => updateUc.ExecuteAsync(employeeId, employeeUpdateDto, ct);

   public Task<Result> DeactivateAsync(
      Guid employeeId,
      DateTime deactivatedAt,
      CancellationToken ct = default
   ) => deactivateUc.ExecuteAsync(employeeId, deactivatedAt, ct);

}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist EmployeeUseCases?
 * -------------------------
 * EmployeeUseCases ist eine Fassade (Facade) über die konkreten
 * UseCase-Implementierungen im Employees-Bounded-Context.
 *
 * Zweck:
 * - Controller müssen nur EIN Interface (IEmployeeUseCases) kennen
 * - Die Fassade delegiert an einzelne UseCase-Klassen:
 *   - EmployeeUcCreate
 *   - EmployeeUcDeactivate
 *   - EmployeeUcSetAdminRights
 *
 *
 * Warum kein async/await?
 * -----------------------
 * Diese Klasse führt keine eigene asynchrone Logik aus,
 * sondern leitet nur Aufrufe durch.
 *
 * async/await würde hier keinen Mehrwert bringen,
 * sondern nur Boilerplate-Code erzeugen.
 *
 *
 * Abgrenzung:
 * -----------
 * - Fachliche Regeln: im Domain Model (Employee Aggregate)
 * - Orchestrierung + Persistenz: in den konkreten UseCases
 * - Lesen/Listen/Filtern: im ReadModel (IEmployeeReadModel)
 *
 * =====================================================================
 */
