using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace BankingApi._3_Infrastructure._2_Persistence.Database.Converter;


public sealed class DateTimeToIsoStringConverter() 
   : ValueConverter<DateTime, string>(
      dto => dto.ToUniversalTime()
         .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
      iso => DateTime.Parse(
         iso,
         CultureInfo.InvariantCulture,
         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
      )
   ) {
}



/*
// epoch ist leider schlecht lesbar
public sealed class DateTimeToUnixTimeConverter() 
   : ValueConverter<DateTime, long>(
      // ➜ C# → DB (write)
      dto => dto.ToUnixTimeMilliseconds(),
      // ➜ DB → C# (read)  ← THIS is the inverse direction
      millis => DateTime.FromUnixTimeMilliseconds(millis)
) { }
*/