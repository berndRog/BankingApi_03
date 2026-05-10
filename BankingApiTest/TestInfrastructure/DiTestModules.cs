using System.Data.Common;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Employees._1_Ports.Inbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._3_FakeInfrastructure._2_Persitence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Adapters;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApi._3_Infrastructure._2_Persistence.ReadModels;
using BankingApi._3_Infrastructure._2_Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest.TestInfrastructure;

public static class DiTestModules {
   
   public static IServiceCollection AddTestModules(
      this IServiceCollection services,
      DbConnection dbConnection,
      bool enableSensitiveDataLogging = true
   ) {
      services.AddSingleton(dbConnection);

      services.AddDbContext<AppDbContext>((sp, options) => {
         var connection = sp.GetRequiredService<DbConnection>();
         options.UseSqlite(connection);

         if (enableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();
      });

      // BC Db Contexts
      services.AddScoped<ICustomerDbContext, CustomerDbContextEf>();
      services.AddScoped<IAccountDbContext, AccountDbContextEf>();
      services.AddScoped<IEmployeeDbContext, EmployeeDbContextEf>();
      
      // Contracts
      services.AddScoped<ICustomerContract, CustomerContractEf>();
      services.AddScoped<IAccountContract, AccountContractEf>();
      services.AddScoped<IEmployeeContract, FakeEmployeeContractEf>();
      
      // Readmodels
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();
      services.AddScoped<IAccountReadModel, AccountReadModelEf>();
      services.AddScoped<IEmployeeReadModel, EmployeeReadModelEf>();
      
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      services.AddScoped<IAccountRepository, AccountRepositoryEf>();
      services.AddScoped<IEmployeeRepository, EmployeeRepositoryEf>();
     
      // Customer UseCases
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcUpdate>();
      services.AddScoped<CustomerUcDeactivate>();
      
      // Account UseCases
      services.AddScoped<IAccountUseCases, AccountUseCases>();
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcDeactivate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      
      // Employe UseCases
      services.AddScoped<IEmployeeUseCases, EmployeeUseCases>();
      services.AddScoped<EmployeeUcCreate>();
      services.AddScoped<EmployeeUcSetAdminRights>();
      services.AddScoped<EmployeeUcUpdate>();
      services.AddScoped<EmployeeUcDeactivate>();
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      // Clock 
      services.AddSingleton<IClock>(_ => new FakeClock(FakeClock.DefaultUtcNow));
      
      
      // IdentityGateway = CustomerRegister() from Seed
      // simulate loggedin customer
      services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway(
         subject: "70000000-0007-0000-0000-000000000000",
         username: "e.engel@freenet.de",
         createdAt: FakeClock.DefaultUtcNow,
         adminRights: 0
      ));
      
      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();
      
      return services;
   }
}