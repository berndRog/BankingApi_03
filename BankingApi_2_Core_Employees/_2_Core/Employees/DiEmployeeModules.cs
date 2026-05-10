using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApi._2_Core.Employees;

public static class DiAddEmployeeModules {

   public static IServiceCollection AddEmployeesModule(
      this IServiceCollection services
   ) {
      // Inbound ports Use Cases
      services.AddScoped<EmployeeUcCreate>();
      services.AddScoped<EmployeeUcSetAdminRights>();
      services.AddScoped<EmployeeUcUpdate>();
      services.AddScoped<EmployeeUcDeactivate>();
      services.AddScoped<IEmployeeUseCases, EmployeeUseCases>();
      // Policies
      return services;
   }
}