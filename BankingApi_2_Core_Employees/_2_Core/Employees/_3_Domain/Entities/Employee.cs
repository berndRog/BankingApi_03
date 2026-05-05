using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._3_Domain.Enum;
using BankingApi._2_Core.Employees._3_Domain.Errors;
namespace BankingApi._2_Core.Employees._3_Domain.Entities;

// Employee aggregate root.
public sealed class Employee : AggregateRoot {
   
   //--- properties ------------------------------------------------------------
   // inherited from Entity + Aggregate root base class
   // public Guid Id { get; private set; } 
   // public DateTimeOffset CreatedAt { get; private set; }
   // public DateTimeOffset UpdatedAt { get; private set; }
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname  { get; private set; } = string.Empty;
   public EmailVo EmailVo { get; private set; } = default!;
   public PhoneVo PhoneVo { get; private set; } = default!;
  
   public string  Subject { get; private set; } = default!; // IdentityAccessServer
   public EmployeeStatus Status { get; private set; } = EmployeeStatus.Pending;
   public string PersonnelNumber { get; private set; } = string.Empty;
   public AdminRights AdminRights { get; private set; } = AdminRights.ViewReports;

 
   private const AdminRights AllowedRights =
      AdminRights.ViewReports |
      AdminRights.ViewCustomers | AdminRights.ManageCustomers |
      AdminRights.ViewAccounts | AdminRights.ManageAccounts |
      AdminRights.ViewTransfers | AdminRights.ManageTransfers |
      AdminRights.ViewEmployees | AdminRights.ManageEmployees;

   public bool IsActive =>
      Status == EmployeeStatus.Active;
   
   //--- constructors ----------------------------------------------------------
   // EF Core ctor
   private Employee() { }

   // Domain ctor
   private Employee(
      Guid id,
      string firstname,
      string lastname,
      EmailVo emailVo,
      PhoneVo phoneVo,
      string subject,
      string personnelNumber, 
      AdminRights adminRights
   ) {
      Id = id;
      Firstname = firstname;
      Lastname  = lastname;
      EmailVo = emailVo;
      PhoneVo = phoneVo;
      Subject = subject;
      PersonnelNumber = personnelNumber;
      AdminRights = adminRights;
   }

   // --- static factory to create a Customer object ---------------------------
   public static Result<Employee> Create(
      string firstname,
      string lastname,
      EmailVo emailVo,
      PhoneVo phoneVo,
      string subject,
      string personnelNumber,
      AdminRights adminRights,
      DateTimeOffset createdAt = default!,
      string? id = null
   ) {
      // Normalize input early
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      personnelNumber = personnelNumber.Trim();

      // required firstname
      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Employee>.Failure(EmployeeErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstname);
      
      // required lastname
      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Employee>.Failure(EmployeeErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstname);
      
      // check required subject (identity)
      var resultSubject = IdentitySubject.Check(subject);
      if (resultSubject.IsFailure)
         return Result<Employee>.Failure(resultSubject.Error);
      
      // required personnel number
      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result<Employee>.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      // createdAt must be provided by caller (not default)
      if(createdAt == default)
         return Result<Employee>.Failure(EmployeeErrors.CreatedAtIsRequired);
      
      // Resolve an entity id from an optional raw string.
      var resultId = Resolve(id, EmployeeErrors.InvalidId);
      if (resultId.IsFailure)
         return Result<Employee>.Failure(resultId.Error);
      var localId = resultId.Value;

      // Admin rights must only contain allowed flags
      if ((adminRights & ~AllowedRights) != 0)
         return Result<Employee>.Failure(EmployeeErrors.InvalidAdminRightsBitmask);
      
      var employee = new Employee(
         id: localId, 
         firstname: firstname,
         lastname: lastname,
         emailVo: emailVo,
         phoneVo: phoneVo,
         subject: subject,
         personnelNumber: personnelNumber,
         adminRights: adminRights
      );
      
      // Creation timestamp is set to createdAt
      employee.Initialize(createdAt);
      
      return Result<Employee>.Success(employee);
   }
   
   //--- Domain methods -----------------------------------------------------------
   // Activates the employee.
   public Result Activate(
      DateTimeOffset activatedAt
   ) {
      if (!IsActive)
         return Result.Failure(EmployeeErrors.AlreadyDeactivated);
      
      Status = EmployeeStatus.Active;
      
      Touch(activatedAt);
      return Result.Success();
   }
   
   // Replaces the administrative rights of the employee.
   public Result SetAdminRights(
      AdminRights adminRights, 
      DateTimeOffset updatedAt
   ) {
      if ((adminRights & ~AllowedRights) != 0)
         return Result.Failure(EmployeeErrors.InvalidAdminRightsBitmask);

      AdminRights = adminRights;
      
      Touch(updatedAt);
      return Result.Success();
   }

   // update employee profile data
   public Result UpdateProfile(
      string? lastname,
      EmailVo? emailVo,
      PhoneVo? phoneVo,
      DateTimeOffset updatedAt
   ) {
      lastname  = lastname?.Trim();
      
      if (!string.IsNullOrWhiteSpace(lastname) && lastname.Length is < 2 or > 80)
         return Result.Failure(EmployeeErrors.InvalidLastname);
      
      // Apply changes
      if (lastname is not null) Lastname = lastname;
      if (emailVo is not null) EmailVo = emailVo;
      if (phoneVo is not null) PhoneVo = phoneVo;
      
      Touch(updatedAt);
      return Result.Success();
   }
   
   // Deactivates the employee.
   public Result Deactivate(
      DateTimeOffset deactivatedAt
   ) {
      if (!IsActive)
         return Result.Failure(EmployeeErrors.AlreadyDeactivated);
      
      Status = EmployeeStatus.Deactivated;
      
      Touch(deactivatedAt);
      return Result.Success();
   }
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist das Employee-Aggregat?
 * ------------------------------
 * Employee ist das Aggregate Root des Employees-Bounded-Contexts.
 *
 * Es modelliert:
 * - Identität und Personendaten (über die Basisklasse Person)
 * - administrative Berechtigungen (AdminRights)
 * - den fachlichen Lebenszyklus eines Mitarbeiters
 *
 *
 * Warum eine Result-basierte Factory?
 * -----------------------------------
 * Die statische Create-Methode stellt sicher, dass:
 * - alle fachlichen Invarianten beim Erzeugen geprüft werden
 * - kein ungültiges Employee-Objekt entstehen kann
 * - Fehler eindeutig als DomainErrors zurückgegeben werden
 *
 *
 * Wie werden AdminRights behandelt?
 * ---------------------------------
 * AdminRights werden IMMER als vollständiger Satz gesetzt.
 *
 * Das bedeutet:
 * - Der neue Wert ersetzt den bisherigen komplett
 * - Es gibt kein inkrementelles Hinzufügen oder Entfernen
 *
 * Vorteil:
 * - deterministischer, sicherer Rechtezustand
 * - einfache Autorisierungslogik
 * - keine schleichenden Berechtigungsreste
 *
 *
 * Aktiv / Inaktiv:
 * ----------------
 * Ein Employee ist entweder aktiv oder deaktiviert.
 * Die Deaktivierung ist:
 * - ein fachlicher Zustand
 * - irreversibel ohne expliziten Reaktivierungs-UseCase
 *
 *
 * Abgrenzung:
 * -----------
 * - Persistenz (EF Core): Infrastructure Layer
 * - Orchestrierung (Create, Deactivate, SetRights):
 *   Application UseCases
 * - Lesen / Suchen / Listen:
 *   EmployeeReadModel
 *
 *
 * Merksatz:
 * ---------
 * Aggregate schützen ihre Invarianten selbst.
 * UseCases orchestrieren – Aggregate entscheiden.
 *
 * =====================================================================
 */
