using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class EmployeesController(
   IEmployeeReadModel readModel,
   EmployeeUcCreate ucCreate,
   ILogger<EmployeesController> logger
) : ControllerBase {

   /// <summary>
   /// Creates a new employee.
   /// </summary>
   /// <remarks>
   /// This endpoint creates a new employee resource and returns
   /// <c>201 Created</c> with the created employee as response body.
   /// </remarks>
   /// <param name="employeeCreateDto">Employee data transferred in the request body.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created employee resource.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpPost("employees", Name = nameof(CreateEmployeeAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> CreateEmployeeAsync(
      [FromBody] EmployeeCreateDto employeeCreateDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(EmployeesController)}.{nameof(CreateEmployeeAsync)}";

      var result = await ucCreate.ExecuteAsync(employeeCreateDto, ct);

      return this.ToCreatedAtRoute(
         routeName: nameof(GetEmployeeById),
         routeValues: new { id = result.IsSuccess ? result.Value.Id : Guid.Empty },
         result,
         logger,
         context,
         args: new { dto = employeeCreateDto }
      );
   }
   
   /// <summary>
   /// Returns an employee by its unique identifier.
   /// </summary>
   /// <remarks>
   /// This endpoint can be used as a directory lookup for employee data.
   /// </remarks>
   /// <param name="id">Unique identifier of the employee.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The employee resource if found.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/{id:guid}", Name = nameof(GetEmployeeById))]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(EmployeesController)}.{nameof(GetEmployeeById)}";

      var result = await readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }

   /// <summary>
   /// Returns an employee by email address.
   /// </summary>
   /// <remarks>
   /// This endpoint can be used as a directory lookup for employee data.
   /// </remarks>
   /// <param name="email">Email address of the employee.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The employee resource if a matching email address exists.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/email", Name = nameof(GetEmployeeByEmail))]
   [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<EmployeeDto>> GetEmployeeByEmail(
      [FromQuery] string email,
      CancellationToken ct
   ) {
      const string context = $"{nameof(EmployeesController)}.{nameof(GetEmployeeByEmail)}";

      var result = await readModel.FindByEmailAsync(email, ct);

      return this.ToActionResult(result, logger, context, args: new { email });
   }
   
   /// <summary>
   /// Returns employees by name.
   /// </summary>
   /// <param name="name">name for SQL %like.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of all employees.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees/name", Name = nameof(GetEmployeesByNameAsync))]
   [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByNameAsync(
      [FromQuery] string name,
      CancellationToken ct
   ) {
      const string context = $"{nameof(CustomersController)}.{nameof(GetEmployeesByNameAsync)}";

      var result = await readModel.SelectByNameAsync(name, ct);

      return this.ToActionResult(result, logger, context, args: name);
   }
   
   /// <summary>
   /// Returns all employees.
   /// </summary>
   /// <remarks>
   /// This endpoint is intended for administrative or directory use cases.
   /// Access is restricted to authenticated employees.
   /// </remarks>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of all employee resources.</returns>
   // [Authorize(Policy = "EmployeesOnly")]
   [HttpGet("employees", Name = nameof(GetAllEmployeesAsync))]
   [ProducesResponseType<IEnumerable<EmployeeDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployeesAsync(
      CancellationToken ct
   ) {
      const string context = $"{nameof(EmployeesController)}.{nameof(GetAllEmployeesAsync)}";

      var result = await readModel.SelectAllAsync(ct);

      return this.ToActionResult(result, logger, context, args: null);
   }
}
